using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
/// <summary>
/// 时间单位枚举
/// </summary>
public enum TimeUnit
{
    /// <summary>
    /// 毫秒
    /// </summary>
    Millisecond = 0,
    /// <summary>
    /// 秒
    /// </summary>
    Second = 1,
    /// <summary>
    /// 分
    /// </summary>
    Minute = 2,
    /// <summary>
    /// 小时
    /// </summary>
    Hour = 3,
    /// <summary>
    /// 天
    /// </summary>
    Day = 4
}

/// <summary>
/// 计时任务工具类
/// </summary>
public class PETimer
{
    #region 字段
    //三个锁 解决多线程的数据问题
    private static readonly string m_LockId = "lockId";
    private static readonly string m_LockTime = "lockTime";
    private static readonly string m_LockFrame = "lockFrame";

    private int m_UniqueId;//任务id  可以通过此ID删除 或者 修改任务                          
    //两组集合 任务数组  记录新添任务时的临时数组 记录将要删除的任务的临时数组  用临时数组可以让逻辑更加清晰 
    private double m_NowTime;//当前时间
    private List<PETimeTask> m_TmpTimeLst = new List<PETimeTask>();//刚添加的任务，下一帧会转存到下面的任务数组里
    private List<PETimeTask> m_TaskTimeLst = new List<PETimeTask>();//任务数组
    private List<int> m_TmpDelTimeLst = new List<int>();//待删除的任务

    private double m_NowTimeUnscaled;
    private List<PETimeTask> m_TmpUnscaledTimeLst = new List<PETimeTask>();
    private List<PETimeTask> m_TaskUnscaledTimeLst = new List<PETimeTask>();
    private List<int> m_TmpDelUnscaledTimeLst = new List<int>();

    private int m_FrameCounter;
    private List<PEFrameTask> m_TmpFrameLst = new List<PEFrameTask>();
    private List<PEFrameTask> m_TaskFrameLst = new List<PEFrameTask>();
    private List<int> m_TmpDelFrameLst = new List<int>();

    private int m_FixedFrameCounter;
    private List<PEFrameTask> m_TmpFixedFrameLst = new List<PEFrameTask>();
    private List<PEFrameTask> m_TaskFixedFrameLst = new List<PEFrameTask>();
    private List<int> m_TmpDelFixedFrameLst = new List<int>();
    #endregion

    /// <summary>
    /// 创建PETimer对象
    /// </summary>
    public PETimer()
    {
        m_TmpTimeLst.Clear();
        m_TaskTimeLst.Clear();

        m_TmpUnscaledTimeLst.Clear();
        m_TaskUnscaledTimeLst.Clear();

        m_TmpFrameLst.Clear();
        m_TaskFrameLst.Clear();

        m_TmpFixedFrameLst.Clear();
        m_TaskFixedFrameLst.Clear();
    }

    /// <summary>
    /// 需要找个Mono类的Update函数来调用这个函数
    /// </summary>
    public void Update()
    {
        //检查是否有任务已经到了要执行的时间
        CheckTimeTask();
        DelTimeTask();

        CheckUnscaledTimeTask();
        DelUnscaledTimeTask();

        CheckFrameTask();
        DelFrameTask();
    }

    /// <summary>
    /// 需要找个Mono类的FixedUpdate函数来调用这个函数
    /// </summary>
    public void FixedUpdate()
    {
        CheckFixedFrameTask();
        DelFixedFrameTask();
    }

    #region 受时间缩放影响的任务
    private void CheckTimeTask()
    {
        //将新增任务转存到实际的任务数组里
        if (m_TmpTimeLst.Count > 0)
        {
            lock (m_LockTime)
            {
                for (int i = 0; i < m_TmpTimeLst.Count; i++)
                {
                    m_TaskTimeLst.Add(m_TmpTimeLst[i]);
                }
                m_TmpTimeLst.Clear();
            }
        }

        //遍历 找到满足执行条件的任务执行
        m_NowTime = Time.time * 1000;
        for (int i = 0; i < m_TaskTimeLst.Count; i++)
        {
            PETimeTask task = m_TaskTimeLst[i];
            if (m_NowTime < task.DestTime)
            {
                continue;
            }
            else
            {
                task.Callback?.Invoke(task.ID);
                //移除已经完成的任务
                if (task.Count == 1)
                {
                    //回收对象
                    PoolMgr.Recycle(task);
                    m_TaskTimeLst.RemoveAt(i);
                    //因为还在遍历taskTimeLst这个数组，这里移除了一个元素，导致数组元素的索引变化了，因此需要index--来校正一下
                    i--;
                }
                else
                {
                    //count默认为1 代表任务是只执行一次的  如果参数给0 代表是无限次，给其他值代表执行对应的次数
                    if (task.Count != 0)
                    {
                        task.Count -= 1;
                    }
                    //更新一下 下次任务执行的时间
                    task.DestTime += task.Delay;
                }
            }
        }
    }
    private void DelTimeTask()
    {
        if (m_TmpDelTimeLst.Count > 0)
        {
            lock (m_LockTime)
            {
                for (int i = 0; i < m_TmpDelTimeLst.Count; i++)
                {
                    bool isDel = false;
                    int delTid = m_TmpDelTimeLst[i];

                    //先从实际任务数组里尝试删除
                    for (int j = 0; j < m_TaskTimeLst.Count; j++)
                    {
                        PETimeTask task = m_TaskTimeLst[j];
                        if (task.ID == delTid)
                        {
                            isDel = true;
                            PoolMgr.Recycle(task);
                            m_TaskTimeLst.RemoveAt(j);
                            break;
                        }
                    }

                    if (isDel)
                        continue;

                    //再从新增任务数组里尝试删除
                    for (int j = 0; j < m_TmpTimeLst.Count; j++)
                    {
                        PETimeTask task = m_TmpTimeLst[j];
                        if (task.ID == delTid)
                        {
                            PoolMgr.Recycle(task);
                            m_TmpTimeLst.RemoveAt(j);
                            break;
                        }
                    }
                }
                m_TmpDelTimeLst.Clear();
            }
        }
    }
    public int AddTimeTask(Action<int> callback, double delay, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        if (timeUnit != TimeUnit.Millisecond)
        {
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    delay = delay * 1000;
                    break;
                case TimeUnit.Minute:
                    delay = delay * 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay = delay * 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay = delay * 1000 * 60 * 60 * 24;
                    break;
            }
        }
        int tid = GetUniqueId();
        m_NowTime = Time.time * 1000;
        lock (m_LockTime)
        {
            m_TmpTimeLst.Add(PoolMgr.Spawn<PETimeTask>().SetData(tid, callback, m_NowTime + delay, delay, count));
        }
        return tid;
    }
    public void DeleteTimeTask(int tid)
    {
        lock (m_LockTime)
        {
            m_TmpDelTimeLst.Add(tid);
        }
    }
    public bool ReplaceTimeTask(int tid, Action<int> callback, float delay, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        if (timeUnit != TimeUnit.Millisecond)
        {
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    delay = delay * 1000;
                    break;
                case TimeUnit.Minute:
                    delay = delay * 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay = delay * 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay = delay * 1000 * 60 * 60 * 24;
                    break;
            }
        }
        m_NowTime = Time.time * 1000;
        PETimeTask newTask = PoolMgr.Spawn<PETimeTask>().SetData(tid, callback, m_NowTime + delay, delay, count);

        bool isRep = false;
        for (int i = 0; i < m_TaskTimeLst.Count; i++)
        {
            if (m_TaskTimeLst[i].ID == tid)
            {
                m_TaskTimeLst[i] = newTask;
                isRep = true;
                break;
            }
        }

        if (!isRep)
        {
            for (int i = 0; i < m_TmpTimeLst.Count; i++)
            {
                if (m_TmpTimeLst[i].ID == tid)
                {
                    m_TmpTimeLst[i] = newTask;
                    isRep = true;
                    break;
                }
            }
        }
        return isRep;
    }
    public PETimeTask GetTask(int id)
    {
        for (int i = 0; i < m_TaskTimeLst.Count; i++)
        {
            if (m_TaskTimeLst[i].ID == id)
            {
                return m_TaskTimeLst[i];
            }
        }
        return null;
    }
    #endregion

    #region 不受时间缩放影响的任务
    private void CheckUnscaledTimeTask()
    {
        if (m_TmpUnscaledTimeLst.Count > 0)
        {
            lock (m_LockTime)
            {
                for (int i = 0; i < m_TmpUnscaledTimeLst.Count; i++)
                {
                    m_TaskUnscaledTimeLst.Add(m_TmpUnscaledTimeLst[i]);
                }
                m_TmpUnscaledTimeLst.Clear();
            }
        }

        //遍历检测任务是否达到条件
        m_NowTimeUnscaled = Time.realtimeSinceStartup * 1000;
        for (int i = 0; i < m_TaskUnscaledTimeLst.Count; i++)
        {
            PETimeTask task = m_TaskUnscaledTimeLst[i];
            if (m_NowTimeUnscaled < task.DestTime)
            {
                continue;
            }
            else
            {
                task.Callback?.Invoke(task.ID);
                if (task.Count == 1)
                {
                    PoolMgr.Recycle(task);
                    m_TaskUnscaledTimeLst.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (task.Count != 0)
                    {
                        task.Count -= 1;
                    }
                    task.DestTime += task.Delay;
                }
            }
        }
    }
    private void DelUnscaledTimeTask()
    {
        if (m_TmpDelUnscaledTimeLst.Count > 0)
        {
            lock (m_LockTime)
            {
                for (int i = 0; i < m_TmpDelUnscaledTimeLst.Count; i++)
                {
                    bool isDel = false;
                    int delTid = m_TmpDelUnscaledTimeLst[i];
                    for (int j = 0; j < m_TaskUnscaledTimeLst.Count; j++)
                    {
                        PETimeTask task = m_TaskUnscaledTimeLst[j];
                        if (task.ID == delTid)
                        {
                            isDel = true;
                            PoolMgr.Recycle(task);
                            m_TaskUnscaledTimeLst.RemoveAt(j);
                            break;
                        }
                    }

                    if (isDel)
                        continue;

                    for (int j = 0; j < m_TmpUnscaledTimeLst.Count; j++)
                    {
                        PETimeTask task = m_TmpUnscaledTimeLst[j];
                        if (task.ID == delTid)
                        {
                            PoolMgr.Recycle(task);
                            m_TmpUnscaledTimeLst.RemoveAt(j);
                            break;
                        }
                    }
                }
                m_TmpDelUnscaledTimeLst.Clear();
            }
        }
    }
    public int AddUnscaledTimeTask(Action<int> callback, double delay, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        if (timeUnit != TimeUnit.Millisecond)
        {
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    delay = delay * 1000;
                    break;
                case TimeUnit.Minute:
                    delay = delay * 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay = delay * 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay = delay * 1000 * 60 * 60 * 24;
                    break;
            }
        }
        int tid = GetUniqueId();
        m_NowTimeUnscaled = Time.realtimeSinceStartup * 1000;
        lock (m_LockTime)
        {
            m_TmpUnscaledTimeLst.Add(PoolMgr.Spawn<PETimeTask>().SetData(tid, callback, m_NowTimeUnscaled + delay, delay, count));
        }
        return tid;
    }
    public void DeleteUnscaledTimeTask(int tid)
    {
        lock (m_LockTime)
        {
            m_TmpDelUnscaledTimeLst.Add(tid);
        }
    }
    public bool ReplaceUnscaledTimeTask(int tid, Action<int> callback, float delay, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        if (timeUnit != TimeUnit.Millisecond)
        {
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    delay = delay * 1000;
                    break;
                case TimeUnit.Minute:
                    delay = delay * 1000 * 60;
                    break;
                case TimeUnit.Hour:
                    delay = delay * 1000 * 60 * 60;
                    break;
                case TimeUnit.Day:
                    delay = delay * 1000 * 60 * 60 * 24;
                    break;
            }
        }
        m_NowTimeUnscaled = Time.realtimeSinceStartup * 1000;
        PETimeTask newTask = PoolMgr.Spawn<PETimeTask>().SetData(tid, callback, m_NowTimeUnscaled + delay, delay, count);


        bool isRep = false;
        for (int i = 0; i < m_TaskUnscaledTimeLst.Count; i++)
        {
            if (m_TaskUnscaledTimeLst[i].ID == tid)
            {
                m_TaskUnscaledTimeLst[i] = newTask;
                isRep = true;
                break;
            }
        }

        if (!isRep)
        {
            for (int i = 0; i < m_TmpUnscaledTimeLst.Count; i++)
            {
                if (m_TmpUnscaledTimeLst[i].ID == tid)
                {
                    m_TmpUnscaledTimeLst[i] = newTask;
                    isRep = true;
                    break;
                }
            }
        }
        return isRep;
    }
    public PETimeTask GetUnscaledTask(int id)
    {
        for (int i = 0; i < m_TaskUnscaledTimeLst.Count; i++)
        {
            if (m_TaskUnscaledTimeLst[i].ID == id)
            {
                return m_TaskUnscaledTimeLst[i];

            }
        }
        return null;
    }
    #endregion

    #region Update帧任务
    private void CheckFrameTask()
    {
        if (m_TmpFrameLst.Count > 0)
        {
            lock (m_LockFrame)
            {
                //加入缓存区中的定时任务
                for (int i = 0; i < m_TmpFrameLst.Count; i++)
                {
                    m_TaskFrameLst.Add(m_TmpFrameLst[i]);
                }
                m_TmpFrameLst.Clear();
            }
        }
        m_FrameCounter += 1;
        //遍历检测任务是否达到条件
        for (int i = 0; i < m_TaskFrameLst.Count; i++)
        {
            PEFrameTask task = m_TaskFrameLst[i];
            if (m_FrameCounter < task.DestFrame)
            {
                continue;
            }
            else
            {
                task.Callback?.Invoke(task.ID);

                //移除已经完成的任务
                if (task.Count == 1)
                {
                    PoolMgr.Recycle(task);
                    m_TaskFrameLst.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (task.Count != 0)
                    {
                        task.Count -= 1;
                    }
                    task.DestFrame += task.Delay;
                }
            }
        }
    }
    private void DelFrameTask()
    {
        if (m_TmpDelFrameLst.Count > 0)
        {
            lock (m_LockFrame)
            {
                for (int i = 0; i < m_TmpDelFrameLst.Count; i++)
                {
                    bool isDel = false;
                    int delTid = m_TmpDelFrameLst[i];
                    for (int j = 0; j < m_TaskFrameLst.Count; j++)
                    {
                        PEFrameTask task = m_TaskFrameLst[j];
                        if (task.ID == delTid)
                        {
                            isDel = true;
                            PoolMgr.Recycle(task);
                            m_TaskFrameLst.RemoveAt(j);
                            break;
                        }
                    }

                    if (isDel)
                        continue;

                    for (int j = 0; j < m_TmpFrameLst.Count; j++)
                    {
                        PEFrameTask task = m_TmpFrameLst[j];
                        if (task.ID == delTid)
                        {
                            PoolMgr.Recycle(task);
                            m_TmpFrameLst.RemoveAt(j);
                            break;
                        }
                    }
                }
                m_TmpDelFrameLst.Clear();
            }
        }
    }
    public int AddFrameTask(Action<int> callback, int delay, int count = 1)
    {
        int tid = GetUniqueId();
        lock (m_LockFrame)
        {
            m_TmpFrameLst.Add(PoolMgr.Spawn<PEFrameTask>().SetData(tid, callback, m_FrameCounter + delay, delay, count));
        }
        return tid;
    }
    public void DeleteFrameTask(int tid)
    {
        lock (m_LockFrame)
        {
            m_TmpDelFrameLst.Add(tid);
        }
    }
    public bool ReplaceFrameTask(int tid, Action<int> callback, int delay, int count = 1)
    {
        PEFrameTask newTask = PoolMgr.Spawn<PEFrameTask>().SetData(tid, callback, m_FrameCounter + delay, delay, count);

        bool isRep = false;
        for (int i = 0; i < m_TaskFrameLst.Count; i++)
        {
            if (m_TaskFrameLst[i].ID == tid)
            {
                m_TaskFrameLst[i] = newTask;
                isRep = true;
                break;
            }
        }

        if (!isRep)
        {
            for (int i = 0; i < m_TmpFrameLst.Count; i++)
            {
                if (m_TmpFrameLst[i].ID == tid)
                {
                    m_TmpFrameLst[i] = newTask;
                    isRep = true;
                    break;
                }
            }
        }

        return isRep;
    }
    public PEFrameTask GetFrameTask(int id)
    {

        for (int i = 0; i < m_TaskFrameLst.Count; i++)
        {
            if (m_TaskFrameLst[i].ID == id)
            {
                return m_TaskFrameLst[i];

            }
        }
        return null;
    }
    #endregion

    #region FixedUpdate帧任务
    private void CheckFixedFrameTask()
    {
        if (m_TmpFixedFrameLst.Count > 0)
        {
            lock (m_LockFrame)
            {
                //加入缓存区中的定时任务
                for (int i = 0; i < m_TmpFixedFrameLst.Count; i++)
                {
                    m_TaskFixedFrameLst.Add(m_TmpFixedFrameLst[i]);
                }
                m_TmpFixedFrameLst.Clear();
            }
        }
        m_FixedFrameCounter += 1;
        //遍历检测任务是否达到条件
        for (int i = 0; i < m_TaskFixedFrameLst.Count; i++)
        {
            PEFrameTask task = m_TaskFixedFrameLst[i];
            if (m_FixedFrameCounter < task.DestFrame)
            {
                continue;
            }
            else
            {
                task.Callback?.Invoke(task.ID);

                //移除已经完成的任务
                if (task.Count == 1)
                {
                    PoolMgr.Recycle(task);
                    m_TaskFixedFrameLst.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (task.Count != 0)
                    {
                        task.Count -= 1;
                    }
                    task.DestFrame += task.Delay;
                }
            }
        }
    }
    private void DelFixedFrameTask()
    {
        if (m_TmpDelFixedFrameLst.Count > 0)
        {
            lock (m_LockFrame)
            {
                for (int i = 0; i < m_TmpDelFixedFrameLst.Count; i++)
                {
                    bool isDel = false;
                    int delTid = m_TmpDelFixedFrameLst[i];
                    for (int j = 0; j < m_TaskFixedFrameLst.Count; j++)
                    {
                        PEFrameTask task = m_TaskFixedFrameLst[j];
                        if (task.ID == delTid)
                        {
                            isDel = true;
                            PoolMgr.Recycle(task);
                            m_TaskFixedFrameLst.RemoveAt(j);
                            break;
                        }
                    }

                    if (isDel)
                        continue;

                    for (int j = 0; j < m_TmpFixedFrameLst.Count; j++)
                    {
                        PEFrameTask task = m_TmpFixedFrameLst[j];
                        if (task.ID == delTid)
                        {
                            PoolMgr.Recycle(task);
                            m_TmpFixedFrameLst.RemoveAt(j);
                            break;
                        }
                    }
                }
                m_TmpDelFixedFrameLst.Clear();
            }
        }
    }
    public int AddFixedFrameTask(Action<int> callback, int delay, int count = 1)
    {
        int tid = GetUniqueId();
        lock (m_LockFrame)
        {
            m_TmpFixedFrameLst.Add(PoolMgr.Spawn<PEFrameTask>().SetData(tid, callback, m_FixedFrameCounter + delay, delay, count));
        }
        return tid;
    }
    public void DeleteFixedFrameTask(int tid)
    {
        lock (m_LockFrame)
        {
            m_TmpDelFixedFrameLst.Add(tid);
        }
    }
    public bool ReplaceFixedFrameTask(int tid, Action<int> callback, int delay, int count = 1)
    {
        PEFrameTask newTask = PoolMgr.Spawn<PEFrameTask>().SetData(tid, callback, m_FixedFrameCounter + delay, delay, count);
        bool isRep = false;
        for (int i = 0; i < m_TaskFixedFrameLst.Count; i++)
        {
            if (m_TaskFixedFrameLst[i].ID == tid)
            {
                m_TaskFixedFrameLst[i] = newTask;
                isRep = true;
                break;
            }
        }

        if (!isRep)
        {
            for (int i = 0; i < m_TmpFixedFrameLst.Count; i++)
            {
                if (m_TmpFixedFrameLst[i].ID == tid)
                {
                    m_TmpFixedFrameLst[i] = newTask;
                    isRep = true;
                    break;
                }
            }
        }

        return isRep;
    }
    public PEFrameTask GetFixedFrameTask(int id)
    {

        for (int i = 0; i < m_TaskFixedFrameLst.Count; i++)
        {
            if (m_TaskFixedFrameLst[i].ID == id)
            {
                return m_TaskFixedFrameLst[i];

            }
        }
        return null;
    }
    #endregion

    #region 其他函数
    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        m_UniqueId = 0;

        m_TmpTimeLst.Clear();
        m_TaskTimeLst.Clear();

        m_TmpUnscaledTimeLst.Clear();
        m_TaskUnscaledTimeLst.Clear();

        m_TmpFrameLst.Clear();
        m_TaskFrameLst.Clear();

        m_TmpFixedFrameLst.Clear();
        m_TaskFixedFrameLst.Clear();
    }

    /// <summary>
    /// 获取一个唯一ID
    /// </summary>
    /// <returns></returns>
    private int GetUniqueId()
    {
        lock (m_LockId)
        {
            m_UniqueId += 1;
            //避免超出int上限，重置一下ID
            if (m_UniqueId > 2100000000)
            {
                m_UniqueId = 1;
            }
        }
        return m_UniqueId;
    }

    /// <summary>
    ///得到当前时间的字符串，按  时:分:秒 的格式
    /// </summary>
    /// <returns></returns>
    public string GetLocalTimeStr()
    {
        DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
        return GetTimeStr(dt.Hour) + ":" + GetTimeStr(dt.Minute) + ":" + GetTimeStr(dt.Second);
    }

    /// <summary>
    /// 不足10就补0
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private string GetTimeStr(int time)
    {
        return time < 10 ? "0" + time : time.ToString();
    }
    #endregion

    #region 数据类
    public class PETimeTask
    {
        public int ID;//任务ID
        public Action<int> Callback;//需要延时执行的回调函数
        public double DestTime;//目标时间点
        public double Delay;//延时多久        
        public int Count;//执行次数

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="callback">需要延时执行的回调函数</param>
        /// <param name="destTime">目标时间点</param>
        /// <param name="delay">延时多久</param>
        /// <param name="count">执行次数</param>
        /// <returns></returns>
        public PETimeTask SetData(int id, Action<int> callback, double destTime, double delay, int count)
        {
            ID = id;
            Callback = callback;
            DestTime = destTime;
            Delay = delay;
            Count = count;
            return this;
        }
    }

    public class PEFrameTask
    {
        public int ID;//任务ID
        public Action<int> Callback;//需要延时执行的回调函数
        public int DestFrame;//目标帧
        public int Delay;//延时几帧 
        public int Count;//执行次数

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="callback">需要延时执行的回调函数</param>
        /// <param name="destFrame">目标帧</param>
        /// <param name="delay">延时几帧</param>
        /// <param name="count">执行几次</param>
        /// <returns></returns>
        public PEFrameTask SetData(int id, Action<int> callback, int destFrame, int delay, int count)
        {
            ID = id;
            Callback = callback;
            DestFrame = destFrame;
            Delay = delay;
            Count = count;
            return this;
        }
    }
    #endregion

}
