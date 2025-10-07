using UnityEngine;
/// <summary>
/// 时间相关管理器(主要是计时任务)
/// </summary>
public class TimeMgr : MonoSingleton<TimeMgr>
{
    private static PETimer m_Timer;
    public static PETimer Timer
    {
        get
        {
            if (m_Timer == null)
            {
                //随便调用下Instance，让单例模板自动创建一个mono物体来调用Timer里面的Update函数
                print(Instance.name);
                m_Timer = new PETimer();
            }
            return m_Timer;
        }
    }

    /// <summary>
    /// 真实的Update帧用时
    /// </summary>
    public static float RealDeltaTime;
    private float m_OldTime = DefaultDef.Float;

    /// <summary>
    /// 真实的FixedUpdate帧用时
    /// </summary>
    public static float RealFiexedDeltaTime;

    private float m_OldFixedTime;

    public void Update()
    {
        //获取真实的Update帧用时
        m_OldTime = m_OldTime == DefaultDef.Float ? Time.realtimeSinceStartup : m_OldTime;
        RealDeltaTime = Time.realtimeSinceStartup - m_OldTime;
        m_OldTime = Time.realtimeSinceStartup;

        //更新普通帧任务 普通计时任务 和不受timescale影响的计时任务  
        Timer.Update();
    }

    public void FixedUpdate()
    {
        //获取真实的FixedUpdate帧用时
        m_OldFixedTime = m_OldFixedTime == DefaultDef.Float ? Time.realtimeSinceStartup : m_OldFixedTime;
        RealFiexedDeltaTime = Time.realtimeSinceStartup - m_OldFixedTime;
        m_OldFixedTime = Time.realtimeSinceStartup;

        //更新Fixed帧任务
        Timer.FixedUpdate();
    }

}