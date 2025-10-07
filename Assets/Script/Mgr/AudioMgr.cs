using System.Collections;
using System.Collections.Generic;
using Script.ConfigEnum;
using Script.Mgr;
using UnityEngine;
/// <summary>
/// 功能基本齐全的2D音效管理器
/// </summary>
public class AudioMgr : MonoBehaviour
{
    #region 字段
    /// <summary>
    /// 播放音乐用的音频组件
    /// </summary>
    private static AudioSource m_MusicAudio;
    /// <summary>
    /// 闲置的音频组件数组
    /// </summary>
    private static Stack<AudioSource> m_IdleAudios = new Stack<AudioSource>();
    /// <summary>
    /// 正在播放音效的音频组件数组，用于调整音效音量大小时同步调整播放中的音效音量大小
    /// </summary>
    private static List<AudioSource> m_SoundAudios = new List<AudioSource>();
    /// <summary>
    /// 挂载当前脚本的游戏对象，配合静态函数使用，静态函数只能使用静态变量
    /// </summary>
    private static GameObject m_SelfGo;
    /// <summary>
    /// 当前Mono类的引用，配合静态函数使用，静态函数只能使用静态变量
    /// </summary>
    private static MonoBehaviour m_SelfMono;
    /// <summary>
    /// 播放多首BGM的时候用的协程，播放新的音乐的时候会先关闭一下这个协程，避免效果叠加
    /// </summary>
    private static Coroutine m_MusicCo;

    /// <summary>
    /// 各种高频音效当前剩余CD的字典，用字典存储方便取对应的值
    /// </summary>
    private static Dictionary<EnumAudioClip, RemainCD> CurrentRemainCDDic = new Dictionary<EnumAudioClip, RemainCD>();
    /// <summary>
    /// 各种高频音效当前剩余CD的数组，字典不方便在遍历过程中修改值，因此需要一个list来配合使用，为了字典和list能够同步数值，这里将CD值放在了RemainCD这个类里
    /// </summary>
    private static List<RemainCD> RemainCDList = new List<RemainCD>();
    private class RemainCD
    {
        public float CD;
    }
    #endregion

    #region 静态 以及 单例相关的内容
    //静态函数只用类名.函数名就能使用，比单例的类名.Instance.函数名会短一些，所以有些管理器脚本我会选用静态函数而非单例模式
    static AudioMgr()
    {
        //调用静态字段 函数 的时候会先调用静态构造(可能?)，所以在静态构造里随便调用一下Instance，就会去创建一个Mono单例，Mono单例可以提供一些Mono类才能支持的功能，比如开启协程和挂载其他Unity组件
        print(Instance.name);
    }
    //基础的单例模式写法
    private static AudioMgr m_Instance;
    public static AudioMgr Instance
    {
        get
        {
            //如果还没有实例  就new个游戏对象，然后添加一个音频管理器返回
            if (m_Instance == null)
            {
                m_Instance = new GameObject().AddComponent<AudioMgr>();
            }
            return m_Instance;
        }
    }
    private void Awake()
    {
        //赋值一下静态变量
        m_SelfMono = this;
        m_SelfGo = gameObject;
        //设置一下卸载场景不删除
        DontDestroyOnLoad(gameObject);
        name = this.GetType().ToString();
        //添加一个音频组件专门作为播放音乐用的音频组件
        m_MusicAudio = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        for (int i = 0; i < RemainCDList.Count; i++)
        {
            RemainCDList[i].CD -= TimeMgr.RealDeltaTime;
        }
    }
    #endregion

    #region 对外的属性和函数
    #region 播放
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioClip">音频枚举</param>
    /// <param name="autoRecycle">等待音效时长，自动将音频组件放回闲置数组</param>
    /// <returns>播放当前音效用的音频组件</returns>
    public static AudioSource PlaySound(EnumAudioClip audioClip, bool autoRecycle = true)
    {
        //如果没有主动设置音效,那么就视作不播放音效
        if (audioClip == EnumAudioClip.None) return null;

        //2.如果是第一次使用这个高频音效，那么就先将其加入数组和字典
        if (!CurrentRemainCDDic.ContainsKey(audioClip))
        {
            RemainCD a = new RemainCD();
            RemainCDList.Add(a);
            CurrentRemainCDDic.Add(audioClip, a);
        }

        float cd = DataMgr.AudioClipPathDic1[audioClip].cD;
        //3.如果调用的时候，这个音效的CD小于0了，那么就刷新CD以及播放音效，否则无视
        if (CurrentRemainCDDic[audioClip].CD <= 0)
        {
            //4.如果调用的时候没有传入CD，那么就用配置里的CD
            cd = cd == 0 ? DataMgr.AudioClipPathDic1[audioClip].cD : cd;
            //更新CD 以及 播放音效
            CurrentRemainCDDic[audioClip].CD = cd;
        }
        else
        {
            return null;
        }

        //如果闲置数组里还有音频组件 就取出一个，否则就新增一个
        AudioSource audio = m_IdleAudios.Count > 0 ? m_IdleAudios.Pop() : m_SelfGo.AddComponent<AudioSource>();
        //将其加入数组，方便后续控制
        m_SoundAudios.Add(audio);

        //异步加载资源
        AssetMgr.LoadAssetAsync<AudioClip>(DataMgr.AudioClipPathDic[audioClip], (clip) =>
        {
            //设置音频 音量 静音状态   然后播放
            audio.clip = clip;
            audio.volume = SoundVolume;
            audio.Play();
            //如果需要自动回收音频组件就等待音频播放完毕之后 将其放回闲置数组复用
            if (autoRecycle)
            {
                m_SelfMono.StartCoroutine(Delay(audio));
            }
        });
        //返回当前音频组件 以便外部做更多的操作(不常用)
        return audio;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="audioClip">音频枚举</param>
    /// <summary>
    /// 播放音乐（带配置等待）
    /// </summary>
    public static AudioSource PlayMusic(EnumAudioClip audioClip)
    {
        if (audioClip == EnumAudioClip.None)
        {
            print("没有音乐");
            return null;
        }
        // 关闭之前的音乐协程
        if (m_MusicCo != null)
        {
            m_SelfMono.StopCoroutine(m_MusicCo);
        }

        // 开始新的协程，等待配置加载完后播放
        m_MusicCo = m_SelfMono.StartCoroutine(WaitAndPlayMusic(audioClip));
        return m_MusicAudio;
    }

    /// <summary>
    /// 协程：等待配置加载完成后再播放
    /// </summary>
    private static IEnumerator WaitAndPlayMusic(EnumAudioClip audioClip)
    {
        string path = DataMgr.AudioClipPathDic[audioClip];
        AssetMgr.LoadAssetAsync<AudioClip>(path, (clip) =>
        {
            
            if (clip == null)
            {
                Debug.LogError($"音频资源加载失败: {path}");
                return;
            }

            m_MusicAudio.clip = clip;
            m_MusicAudio.volume = MusicVolume;
            m_MusicAudio.loop = true;
            m_MusicAudio.Play();
        });
        yield return null;
    }


    /// <summary>
    /// 随机或顺序播放列表里的音乐
    /// </summary>
    /// <param name="clips">要播放的音乐的枚举数组</param>
    /// <param name="random">随机播放</param>
    /// <returns></returns>
    public static AudioSource PlayMusic(List<EnumAudioClip> clips, bool random = true)
    {
        if (clips == null || clips.Count == 0) return null;
        //关闭之前的音乐协程，避免效果叠加
        if (m_MusicCo != null)
        {
            m_SelfMono.StopCoroutine(m_MusicCo);
        }

        m_MusicCo = m_SelfMono.StartCoroutine(PlayMusicList(clips, random));
        return m_MusicAudio;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="clips">要播放的音乐的枚举数组</param>
    /// <param name="random">随机播放还是顺序播放</param>
    /// <returns></returns>
    private static IEnumerator PlayMusicList(List<EnumAudioClip> clips, bool random)
    {
        int index = 0;
        //这种播放方式不loop
        m_MusicAudio.loop = false;
        //获取静音状态 以及音量
        m_MusicAudio.volume = MusicVolume;
        
        while (true)
        {
            //获取当前要播放的音乐的资源路径
            EnumAudioClip clip = EnumAudioClip.None;
            //如果是随机播放 就随机一个索引
            if (random)
            {
                clip = clips[Random.Range(0, clips.Count)];
            }
            //如果是顺序播放 就根据index来获取资源名 
            else
            {
                clip = clips[index % clips.Count];//这里取余一下 就可以让他循环播放下去，而不会数组索引越界
            }
            bool loadFinish = false;
            //异步加载资源
            AssetMgr.LoadAssetAsync<AudioClip>(DataMgr.AudioClipPathDic[clip], (c) =>
            {
                loadFinish = true;
                m_MusicAudio.clip = c;
            });
            while (!loadFinish)
            {
                yield return null;
            }
            //播放 等待音频时长播放下一曲
            m_MusicAudio.Play();
            yield return new WaitForSeconds(m_MusicAudio.clip.length);
            index++;
        }
    }
    #endregion

    #region 音量
    private static float m_SoundVolume = 1;
    /// <summary>
    /// 音效音量
    /// </summary>
    public static float SoundVolume
    {
        get => m_SoundVolume;
        set
        {
            //同步数值
            m_SoundVolume = value;
            //修改当前正在播放的音频组件的音量大小
            m_SoundAudios.ForEach(a => a.volume = value);
        }

    }

    private static float m_MusicVolume = -1;
    /// <summary>
    /// 音乐音量
    /// </summary>
    public static float MusicVolume
    {
        get => m_MusicVolume;
        set
        {
            m_MusicVolume = value;
            m_MusicAudio.volume = value;
        }

    }
    #endregion
  
    #region 静音设置
    /// <summary>
    /// 音效静音
    /// </summary>
    /// <param name="mute">静音</param>
    public static void SetSoundMuteState(bool mute)
    {
        //静音当前正在播放的音频组件  后续新播放的在播放时就会静音
        m_SoundAudios.ForEach(a => a.mute = mute);
    }

    /// <summary>
    /// 音乐静音
    /// </summary>
    /// <param name="mute">静音</param>
    public static void SetMusicMuteState(bool mute)
    {
        m_MusicAudio.mute = mute;
    }
    #endregion

    #region 暂停和继续音乐 音效
    /// <summary>
    /// 暂停范围
    /// </summary>
    public enum AudioPauseMode
    {
        All,//暂停全部类型的声音
        Sound,//仅暂停音效
        Music,//仅暂停音乐
    }
    /// <summary>
    /// 暂停声音
    /// </summary>
    /// <param name="mode">暂停哪些类型的音</param>
    public static void Pause(AudioPauseMode mode = AudioPauseMode.All)
    {
        switch (mode)
        {
            case AudioPauseMode.All:
                m_MusicAudio.Pause();
                m_SoundAudios.ForEach(a => { a.Pause(); });
                break;
            case AudioPauseMode.Sound:
                m_SoundAudios.ForEach(a => { a.Pause(); });
                break;
            case AudioPauseMode.Music:
                m_MusicAudio.Pause();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 继续播放声音
    /// </summary>
    /// <param name="mode">暂停哪些类型音</param>
    public static void UnPause(AudioPauseMode mode = AudioPauseMode.All)
    {
        switch (mode)
        {
            case AudioPauseMode.All:
                m_MusicAudio.UnPause();
                m_SoundAudios.ForEach(a => { a.UnPause(); });
                break;
            case AudioPauseMode.Sound:
                m_SoundAudios.ForEach(a => { a.UnPause(); });
                break;
            case AudioPauseMode.Music:
                m_MusicAudio.UnPause();
                break;
            default:
                break;
        }
    }
    #endregion

    #region 组件回收
    /// <summary>
    /// 等待音频时长后将音频组件回收
    /// </summary>
    /// <param name="audio">目标音频组件</param>
    /// <returns></returns>
    private static IEnumerator Delay(AudioSource audio)
    {
        yield return new WaitForSeconds(audio.clip.length);
        Recycle(audio);
    }

    /// <summary>
    /// 回收音频组件
    /// </summary>
    /// <param name="audio">待回收的音频组件</param>
    public static void Recycle(AudioSource audio)
    {
        if (m_SoundAudios.Contains(audio))
        {
            m_SoundAudios.Remove(audio);
        }
        if (!m_IdleAudios.Contains(audio))
        {
            m_IdleAudios.Push(audio);
        }
    }
    #endregion
    #endregion
}
