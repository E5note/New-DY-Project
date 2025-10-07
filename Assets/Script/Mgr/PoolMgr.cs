using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Script.Mgr;

/// <summary>
/// 池子管理器(引用池+对象池)
/// </summary>
public class PoolMgr : MonoSingleton<PoolMgr>
{
    static PoolMgr()
    {
        //创建这个类时使用一下Instance字段，让其先自动创建一个Mono类对象，因为我们需要一个Update函数来检测对象的活跃度，动态释放池子里一些闲置了很久的对象
        Debug.Log(Instance);
    }

    #region 静态字段
    /// <summary>
    /// 类对象的池子的字典，键是池子名，值是这个池子的管理器
    /// </summary>
    private static Dictionary<string, ClassPool> m_ClassPoolDic = new Dictionary<string, ClassPool>();
    /// <summary>
    /// 游戏对象的池子的字典，键是池子名，值是这个池子的管理器
    /// </summary>
    private static Dictionary<string, GameObjectPool> m_GameObjectPoolDic = new Dictionary<string, GameObjectPool>();

    /// <summary>
    /// 对象池初始化时默认创建对象的数量
    /// </summary>
    private static int m_DefaultInitCount = 50;
    /// <summary>
    /// 对象池默认的最小缓存对象的数量，少于这个数量后便不会继续缓释下去
    /// </summary>
    private static int m_DefaultMinCacheCount = 40;
    /// <summary>
    /// 对象池默认的启动缓释的判断时长，对象闲置多久会被缓释掉
    /// </summary>
    private static float m_DefaultMaxUnusedTime = 120;
    /// <summary>
    /// 对象池默认的缓释状态释放对象的间隔，单位秒
    /// </summary>
    private static float m_DefaultReleaseInterval = 1f;
    /// <summary>
    /// 对象池默认的每次释放对象的数量
    /// </summary>
    private static int m_DefaultMaxReleaseCount = 3;
    #endregion

    #region 初始化池子的静态方法
    /// <summary>
    /// 初始化类对象的池子，使用池子之前最好先初始化池子，否则将会用默认的数据来初始化池子，比如调用无参构造new一个类对象
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="createFun">创建对象的方法，为空则会使用无参构造new一个对象</param>
    /// <param name="resetFun">重置对象的方法，为空时，对象取出时需要自己手动重置一下</param>
    /// <param name="initCount">初始化时创建的对象个数</param>
    /// <param name="minCacheCount">对象池最小缓存对象的数量</param>
    /// <param name="maxUnusedTime">多久没使用的对象会被缓释掉</param>
    /// <param name="releaseInterval">对象池缓释时释放对象的间隔</param>
    /// <param name="maxReleaseCount">对象池缓释时每次最大释放对象的个数</param>
    public static void InitPool<T>(Func<object> createFun = null, Func<object, object> resetFun = null,
        int initCount = 0, int minCacheCount = 0, float maxUnusedTime = 0, float releaseInterval = 0, int maxReleaseCount = 0) where T : new()
    {
        //使用类名作为类对象池子的名字
        string poolName = typeof(T).ToString();
        //如果不存在此名字的池子，就初始化一个池子，如果已经存在则报错提示一下
        if (!m_ClassPoolDic.ContainsKey(poolName))
        {
            ClassPool newPool = new ClassPool();
            newPool.Items = new Stack<object>();
            //如果没有传递参数则使用默认的配置
            initCount = initCount == 0 ? m_DefaultInitCount : initCount;
            newPool.MinCacheCount = minCacheCount == 0 ? m_DefaultMinCacheCount : minCacheCount;
            newPool.MaxUnusedTime = maxUnusedTime == 0 ? m_DefaultMaxUnusedTime : maxUnusedTime;
            newPool.ReleaseInterval = releaseInterval == 0 ? m_DefaultReleaseInterval : releaseInterval;
            newPool.MaxReleaseCount = maxReleaseCount == 0 ? m_DefaultMaxReleaseCount : maxReleaseCount;

            //如果没有传递创建对象的方法，就用默认的new作为创建对象的方法
            if (createFun == null)
            {
                createFun = () => { return new T(); };
            }
            newPool.CreateFun = createFun;
            newPool.ResetFun = resetFun;

            //创建一批对象备用
            for (int i = 0; i < initCount; i++)
            {
                newPool.Recycle(createFun());
            }
            //将池子加入字典
            m_ClassPoolDic.Add(poolName, newPool);
        }
        else
        {
            Debug.LogError($"已存在名为{poolName}的类对象池子，初始化池子失败");
        }
    }

    /// <summary>
    /// 初始化游戏对象的池子，使用池子之前最好先初始化池子，否则将会用默认的数据来初始化池子，比如调用Instantiate创建一个与预制体一样的游戏对象
    /// </summary>
    /// <param name="prefab">游戏对象的预制体</param>
    /// <param name="assetName">预制体资源名，与预制体选填一个即可</param>
    /// <param name="loadType">如果是通过资源名加载资源的话，使用哪种加载类型</param>
    /// <param name="createFun">创建对象的方法，为空则会使用Instantiate创建一个跟预制体一样的对象</param>
    /// <param name="resetFun">重置对象的方法，为空时，对象取出时需要自己手动重置一下</param>
    /// <param name="initCount">初始化时创建的对象个数</param>
    /// <param name="minCacheCount">对象池最小缓存对象的数量</param>
    /// <param name="maxUnusedTime">多久没使用的对象会被缓释掉</param>
    /// <param name="releaseInterval">对象池缓释时释放对象的间隔</param>
    /// <param name="maxReleaseCount">对象池缓释时每次最大释放对象的个数</param>
    /// <param name="poolName">池子的名字，为空则会使用预制体的名字</param>
    /// <param name="parent">对象的父物体，为空则会在默认池子父物体下创建一个父物体作为对象父物体</param>
    public async static void InitPool(GameObject prefab, string assetName = "", AssetLoadType loadType = AssetLoadType.Temp, Func<GameObject> createFun = null, Func<GameObject, GameObject> resetFun = null,
        int initCount = 0, int minCacheCount = 0, float maxUnusedTime = 0, float releaseInterval = 0, int maxReleaseCount = 0, string poolName = null, Transform parent = null)
    {
        //如果没有传入池子名 就使用资源名或者预制体名字
        if (string.IsNullOrEmpty(poolName))
        {
            poolName = !string.IsNullOrEmpty(assetName) ? assetName : prefab.name;
        }
        //如果资源名不为空 就使用资源名加载预制体作为游戏对象的模板
        if (!string.IsNullOrEmpty(assetName))
        {
            prefab = await AssetMgr.LoadAssetAsync<GameObject>(DataMgr.AssetPathDic[assetName], loadType);
        }

        //如果不存在此名字的池子，就初始化一个池子，如果已经存在则报错提示一下
        if (!m_GameObjectPoolDic.ContainsKey(poolName))
        {
            GameObjectPool newPool = new GameObjectPool();
            //提前将池子加入字典，用于防止后续异步操作时存在的一些问题
            m_GameObjectPoolDic.Add(poolName, newPool);
            newPool.Items = new Stack<GameObject>();

            //如果没有传递参数则使用默认的配置
            newPool.Prefab = prefab;
            initCount = initCount == 0 ? m_DefaultInitCount : initCount;
            newPool.MinCacheCount = minCacheCount == 0 ? m_DefaultMinCacheCount : minCacheCount;
            newPool.MaxUnusedTime = maxUnusedTime == 0 ? m_DefaultMaxUnusedTime : maxUnusedTime;
            newPool.ReleaseInterval = releaseInterval == 0 ? m_DefaultReleaseInterval : releaseInterval;
            newPool.MaxReleaseCount = maxReleaseCount == 0 ? m_DefaultMaxReleaseCount : maxReleaseCount;

            //如果没有传递创建对象的方法，就默认用预制体实例化一个一模一样的游戏对象
            if (createFun == null)
            {
                createFun = () => { return UnityEngine.Object.Instantiate(prefab); };
            }
            newPool.CreateFun = createFun;
            newPool.ResetFun = resetFun;

            //如果自定义的父物体不为空，就将池子的父物体设置为传入的transform
            if (parent != null)
            {
                newPool.Parent = parent;

            }
            //如果不自定义父物体则创建一个空物体作为对象们的父物体，并设置到默认的池子父物体下
            else
            {
                GameObject poolParent = new GameObject();
                poolParent.name = poolName;
                poolParent.transform.parent = Instance.gameObject.transform;
                newPool.Parent = poolParent.transform;
            }

            //创建一批对象备用
            for (int i = 0; i < initCount; i++)
            {
                GameObject go = createFun();
                go.name = poolName;
                go.transform.SetParent(newPool.Parent);
                go.SetActive(false);
                newPool.Recycle(go);
            }

            //将初始化结束的标识设置为true 用作后面一些异步逻辑的判断
            newPool.InitFinish = true;
        }
        else
        {
            Debug.LogError($"已存在名为{poolName}的游戏对象池子，初始化池子失败");
        }
    }
    #endregion

    #region 取对象的静态方法
    /// <summary>
    /// 取得一个类对象，如果池子不存在则会使用默认的数据创建一个池子
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="poolName">池子名字，为空则使用类名</param>
    /// <returns></returns>
    public static T Spawn<T>(string poolName = null) where T : new()
    {
        //如果不传入池子名称则会使用类名作为池子名称查找
        if (string.IsNullOrEmpty(poolName))
        {
            poolName = typeof(T).ToString();
        }

        //如果池子存在
        if (m_ClassPoolDic.ContainsKey(poolName))
        {
            ClassPool pool = m_ClassPoolDic[poolName];
            return (T)pool.Spawn();
        }
        //如果池子不存在，则会使用默认数据构建一个池子，并且返回一个对象
        else
        {
            InitPool<T>();
            return Spawn<T>(poolName);
        }
    }

    /// <summary>
    /// 取一个游戏对象
    /// </summary>
    /// <param name="assetName">池子名(一般是预制体资源名)</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public static GameObject Spawn(string assetName, AssetLoadType loadType = AssetLoadType.Temp)
    {
        //如果池子已经存在 
        if (m_GameObjectPoolDic.ContainsKey(assetName))
        {
            //如果初始化完毕 就从池子里取出一个元素返回
            if (m_GameObjectPoolDic[assetName].InitFinish)
            {
                return m_GameObjectPoolDic[assetName].Spawn(assetName);
            }
            //否则就创建一个元素返回

        }
        //如果池子不存在 就初始化一个池子
        InitPool(null, assetName);
        return m_GameObjectPoolDic[assetName].Spawn(assetName);
    }
    #endregion

    #region 回收对象的静态方法
    /// <summary>
    /// 回收类对象
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="obj">待回收的对象</param>
    /// <param name="poolName">池子名，为空则使用类名</param>
    /// <returns></returns>
    public static bool Recycle<T>(T obj, string poolName = null)
    {
        //如果池子名为空 就使用类名
        if (string.IsNullOrEmpty(poolName))
        {
            poolName = typeof(T).ToString();
        }

        //如果对象为空 则无视
        if (obj == null) return false;

        //将其放回目标池子
        if (m_ClassPoolDic.ContainsKey(poolName))
        {
            ClassPool pool = m_ClassPoolDic[poolName];
            pool.Recycle(obj);
            return true;
        }
        else
        {
            Debug.LogWarning("并不存在名为 " + poolName + " 的类对象池");
            return false;
        }
    }

    /// <summary>
    /// 回收游戏对象
    /// </summary>
    /// <param name="obj">待回收的对象</param>
    /// <param name="poolName">池子名(一般是预制体名字或者预制体资源名)</param>
    /// <returns></returns>
    public static bool Recycle(GameObject obj, string poolName = null)
    {
        //如果池子名为空 则使用游戏对象的名字
        if (string.IsNullOrEmpty(poolName))
        {
            poolName = obj.name;
        }

        //如果对象为空 则无视
        if (obj == null) return false;

        //如果池子存在且初始化完毕 就将其放回目标池子
        if (m_GameObjectPoolDic.ContainsKey(poolName) && m_GameObjectPoolDic[poolName].InitFinish)
        {
            GameObjectPool pool = m_GameObjectPoolDic[poolName];
            pool.Recycle(obj);
            return true;
        }
        else
        {
            obj.DestroyGameObjectSafe();//直接销毁
            Debug.LogWarning("名为 " + poolName + " 的对象池不存在或者还是初始化中");
            return false;
        }
    }
    #endregion

    #region 清理池子的静态方法
    /// <summary>
    /// 清除指定池子里的对象，池子外的对象是不会被清掉的，但是在回收的时候，因为没有对应池子了就会直接被销毁掉
    /// </summary>
    /// <param name="poolName">池子的名字</param>
    public static async void ClearPool(string poolName)
    {
        //类对象池只需要将池子从字典里移除 让对象没人引用就行了
        if (m_ClassPoolDic.ContainsKey(poolName))
        {
            ClassPool pool = m_ClassPoolDic[poolName];
            m_ClassPoolDic.Remove(poolName);
            Debug.Log("池子" + poolName + "清理完成！");
            return;
        }

        if (m_GameObjectPoolDic.ContainsKey(poolName))
        {
            //如果游戏对象池子初始化还未完毕 那么等初始化完毕再清理
            while (!m_GameObjectPoolDic[poolName].InitFinish)
            {
                await Task.Delay(20);
            }

            GameObjectPool pool = m_GameObjectPoolDic[poolName];
            //销毁栈里的对象
            foreach (var item in pool.Items)
            {
                item.DestroyGameObjectSafe();
            }
            //清除引用
            pool.Items.Clear();
            m_GameObjectPoolDic.Remove(poolName);
            Debug.Log("池子" + poolName + "清理完成！");
            return;
        }
        Debug.LogError($"池子清除错误：不存在名为{poolName}的池子");
    }

    /// <summary>
    /// 清除全部的类对象池
    /// </summary>
    public static void ClearAllClassPools()
    {
        //类对象池子只需清空对象池子的字典就行
        m_ClassPoolDic.Clear();      
    }

    /// <summary>
    /// 清除全部的游戏对象池
    /// </summary>
    public static async void ClearAllGameobjectPools()
    {
        //游戏对象需要遍历删除字典里所有池子里的游戏对象，然后清空字典
        foreach (GameObjectPool pool in m_GameObjectPoolDic.Values)
        {
            //先等待初始化完毕再清理
            while (!pool.InitFinish)
            {
                await Task.Delay(20);
            }
            foreach (var item in pool.Items)
            {
                item.DestroyGameObjectSafe();
            }
            pool.Items.Clear();
        }
        m_GameObjectPoolDic.Clear();

    }

    /// <summary>
    /// 清除全部池子
    /// </summary>
    public static void ClearAllPools()
    {
        ClearAllClassPools();
        ClearAllGameobjectPools();
    }
    #endregion

    #region 缓释
    private void Update()
    {
        foreach (var item in m_ClassPoolDic.Values)
        {
            item.Update();
        }

        foreach (var item in m_GameObjectPoolDic.Values)
        {
            item.Update();
        }
    }
    #endregion

    #region 调整池子
    /// <summary>
    /// 调整类对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetCount">目标对象数量</param>
    /// <param name="newMinCacheCount">新的最小缓存数量</param>
    /// <param name="maxUnusedTime">对象闲置多久进入缓释</param>
    /// <param name="newReleaseInterval">新的缓释间隔</param>
    /// <param name="newMaxReleaseCount">新的每次最大缓释数量</param>
    public static void AdjustPool<T>(int targetCount, int newMinCacheCount, float maxUnusedTime, float newReleaseInterval, int newMaxReleaseCount) where T : new()
    {
        //如果池子已经存在
        if (m_ClassPoolDic.ContainsKey(typeof(T).ToString()))
        {
            //拿到池子 然后修改参数
            ClassPool pool = m_ClassPoolDic[typeof(T).ToString()];
            pool.MinCacheCount = newMinCacheCount;
            pool.MaxUnusedTime = maxUnusedTime;
            pool.ReleaseInterval = newReleaseInterval;
            pool.MaxReleaseCount = newMaxReleaseCount;

            //如果当前数量 少于目标数量，就创建一个实例到池子里
            while (targetCount > pool.Items.Count)
            {
                pool.Recycle(pool.CreateFun());
            }
            //如果当前数量比目标数量大 就弹出一个对象
            while (pool.Items.Count > targetCount)
            {
                pool.Spawn();
            }
        }
        //如果池子不存在 就用传入的数据初始化一个池子
        else
        {
            InitPool<T>(null, null, targetCount, newMinCacheCount, maxUnusedTime, newReleaseInterval, newMaxReleaseCount);
        }
    }

    /// <summary>
    /// 调整游戏对象池
    /// </summary>
    /// <param name="poolName">要调整的池子的名字</param>
    /// <param name="targetCount">目标对象数量</param>
    /// <param name="newMinCacheCount">新的最小缓存数量</param>
    /// <param name="maxUnusedTime">对象闲置多久进入缓释</param>
    /// <param name="newReleaseInterval">新的缓释间隔</param>
    /// <param name="newMaxReleaseCount">新的每次最大缓释数量</param>
    /// <param name="loadType">如果是通过资源名加载资源的话，使用哪种加载类型</param>
    public static async void AdjustPool(string poolName, int targetCount, int newMinCacheCount, float maxUnusedTime, float newReleaseInterval, int newMaxReleaseCount, AssetLoadType loadType = AssetLoadType.Temp)
    {
        //如果池子已经存在 
        if (m_GameObjectPoolDic.ContainsKey(poolName))
        {
            //如果还没初始化完毕 就先等待初始化完毕
            while (!m_GameObjectPoolDic[poolName].InitFinish)
            {
                await Task.Delay(20);
            }

            //拿到池子 然后修改参数
            GameObjectPool pool = m_GameObjectPoolDic[poolName];
            pool.MinCacheCount = newMinCacheCount;
            pool.MaxUnusedTime = maxUnusedTime;
            pool.ReleaseInterval = newReleaseInterval;
            pool.MaxReleaseCount = newMaxReleaseCount;

            //如果当前数量 少于目标数量，就创建一个实例到池子里
            while (targetCount > pool.RemainCount)
            {
                pool.Recycle(pool.CreateFun());
            }
            //如果当前数量比目标数量大 就弹出一个对象销毁
            while (pool.RemainCount > targetCount)
            {
                Destroy(pool.Spawn(poolName));
            }
        }
        else
        {
            InitPool(null, poolName, loadType, null, null, targetCount, newMinCacheCount, maxUnusedTime, newReleaseInterval, newMaxReleaseCount);
        }
    }
    #endregion

}

/// <summary>
/// 游戏物体对象池
/// </summary>
public class GameObjectPool
{
    #region 字段
    /// <summary>
    /// 存储游戏对象的栈
    /// </summary>
    public Stack<GameObject> Items;
    /// <summary>
    /// 记录对象闲置时长的字典，键是对象，值是对象闲置的时长，闲置时长超出给定值之后就会被缓释掉
    /// </summary>
    public Dictionary<GameObject, float> UnusedTimeDic = new Dictionary<GameObject, float>();
    /// <summary>
    /// 预制体的引用，创建新对象的时候以此为模板创建
    /// </summary>
    public GameObject Prefab;
    /// <summary>
    /// 最小缓存数量，少于这个数量，池子里的对象不会被缓释掉
    /// </summary>
    public int MinCacheCount;
    /// <summary>
    /// 对象闲置多久会被缓释掉
    /// </summary>
    public float MaxUnusedTime;
    /// <summary>
    /// 释放间隔
    /// </summary>
    public float ReleaseInterval;
    /// <summary>
    /// 每次缓释，释放对象的最大数量
    /// </summary>
    public int MaxReleaseCount;
    /// <summary>
    /// 创建对象的方法，一般教程里会用工厂模式，或者是用默认的创建方法，前者过于麻烦后者过于简单，因此折中一下让使用者自己传递一个创建方法或者不传递创建方法使用默认的创建方法
    /// </summary>
    public Func<GameObject> CreateFun;
    /// <summary>
    /// 重置对象的方法，因为每个对象重置的内容不同，因此只能通过传入一个方法才能实现重置。当然也可以不传递重置方法，取出后手动重置下
    /// </summary>
    public Func<GameObject, GameObject> ResetFun;
    /// <summary>
    /// 游戏对象的父物体
    /// </summary>
    public Transform Parent;
    /// <summary>
    /// 池子初始化完毕的标识，池子初始化完毕后才能存储对象
    /// </summary>
    public bool InitFinish;
    /// <summary>
    /// 当前池子里剩余的对象数量
    /// </summary>
    public int RemainCount;
    private float m_ReleaseCD;//当前剩余的缓释CD
    #endregion

    /// <summary>
    /// 从池子里取出一个游戏对象
    /// </summary>
    /// <param name="poolName">池子名</param>
    /// <returns></returns>
    public GameObject Spawn(string poolName)
    {
        //从栈里取出一个元素 因为栈里有些对象被缓释掉了 所以可能为空 所以需要过滤一下
        GameObject go = null;
        do
        {
            if (Items.Count > 0)
            {
                go = Items.Pop();
                if (go != null)
                {
                    RemainCount--;
                }
            }
            else
            {
                //如果池子里没有对象，就临时创建一个返回
                go = CreateFun();
                go.name = poolName;
                go.transform.SetParent(Parent);
                go.SetActive(true);
            }

        } while (go == null);
        //被取出的对象的闲置时长可以设置为-999999999 这样就不会使用中被缓释掉
        UnusedTimeDic[go] = -999999999;
        if (ResetFun != null)
        {
            return ResetFun(go);
        }
        return go;
    }

    /// <summary>
    /// 将一个游戏对象放回池子
    /// </summary>
    /// <param name="go">要放回的对象</param>
    public void Recycle(GameObject go)
    {
        //如果对象不为空 就将其放回 且将闲置时长刷新为0
        if (go != null)
        {
            go.SetActive(false);
            Items.Push(go);
            UnusedTimeDic[go] = 0;
            RemainCount++;
        }
    }

    public void Update()
    {
        //等待缓释CD
        m_ReleaseCD += Time.deltaTime;
        if (m_ReleaseCD > ReleaseInterval)
        {
            m_ReleaseCD = 0;
            int count = 0;
            //遍历字典 给元素增加闲置时长
            for (int i = UnusedTimeDic.Count - 1; i > 0; i--)
            {
                var element = UnusedTimeDic.ElementAt(i);
                UnusedTimeDic[element.Key] += Time.deltaTime;
                //如果某个对象闲置时长比给定值大了 且此轮缓释还没达到数量上限 以及剩余的物体数量还没小于最小数量 就释放掉这个对象
                if (UnusedTimeDic[element.Key] > MaxUnusedTime)
                {
                    if (count > MaxReleaseCount || RemainCount < MinCacheCount) continue;
                    count++;
                    //从字典里移除 并且销毁
                    UnusedTimeDic.Remove(element.Key);
                    RemainCount--;
                    UnityEngine.Object.Destroy(element.Key);
                }

            }
        }
    }

}

/// <summary>
/// 类对象池
/// </summary>
public class ClassPool
{
    public Stack<object> Items;//存放对象的栈
    public int MinCacheCount;//最小缓存数量，低于这个数量的对象不会被缓释掉
    public float MaxUnusedTime;//池子多久没取对象开始缓释
    public float ReleaseInterval;//释放间隔
    public int MaxReleaseCount;//每次最大缓释数量
    public Func<object> CreateFun;//创建对象的方法
    public Func<object, object> ResetFun;//重置对象的方法
    private float m_ReleaseCD;//当前缓释CD   
    private float m_CurUnusedTime;//多久没取对象了

    /// <summary>
    /// 从池子里取出一个对象
    /// </summary>
    /// <returns></returns>
    public object Spawn()
    {
        //刷新一下计时
        m_CurUnusedTime = 0;
        //如果是从池子里取出的 就修改一下计数
        object obj = null;
        if (Items.Count > 0)
        {
            obj = Items.Pop();
            ResetFun?.Invoke(obj);
        }
        else
        {
            obj = CreateFun();
        }
        return obj;
    }

    /// <summary>
    /// 将一个游戏对象放回池子
    /// </summary>
    /// <param name="go">要放回的对象</param>
    public void Recycle(object obj)
    {
        //如果对象不为空 就将其放回 
        if (obj != null)
        {
            Items.Push(obj);
        }
    }

    public void Update()
    {
        //如果没取对象时长比给定值小 就无视
        m_CurUnusedTime += Time.deltaTime;
        if (m_CurUnusedTime < MaxUnusedTime) return;

        //如果缓释间隔CD还没冷却好就无视
        m_ReleaseCD += Time.deltaTime;
        if (m_ReleaseCD < ReleaseInterval) return;

        m_ReleaseCD = 0;//重置CD
        //释放给定数量个对象 如果剩余数量比最小缓存数量大
        for (int i = 0; i < MaxReleaseCount; i++)
        {
            if (Items.Count > MinCacheCount)
            {
                Items.Pop();
            }
        }
    }

}