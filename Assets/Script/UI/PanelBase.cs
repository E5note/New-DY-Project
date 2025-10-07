using System;
using UnityEngine; 
/// <summary>
/// UI界面基类
/// </summary>
public abstract class PanelBase : SoraMono
{
    #region 字段
    [FieldName("淡入用时")]
    public float FadeInTime = 0.3f;
    [FieldName("淡出用时")]
    public float FadeOutTime = 0.2f;
    [FieldName("界面显示延时")]
    public float ShowDelay;//等待这个延时才真正执行显示页面的逻辑
    [FieldName("界面关闭延时")]
    public float HideDelay;//等待这个延时才真正执行关闭页面的逻辑
    [FieldName("界面关闭回调函数延时")]
    public float HideTaskDelay;//关闭页面后 等待这个延时才会执行页面关闭的回调函数

    protected float HideCD;//有些时候为了防止玩家误操作 以及避免出现一些bug，会在显示界面的时候给一个零点几秒的CD，CD期间不允许关闭界面
    [HideInInspector] public Action HideEvent;//界面关闭后要执行的事件
    [HideInInspector] public bool ShowDuration;//界面是否淡入中，淡入中不会重复淡入
    [HideInInspector] public bool HideDuration;//界面是否淡出中，淡出中不会重复淡出

    private PanelFade m_PanelFade;
    /// <summary>
    /// 辅助界面淡入淡出的脚本
    /// </summary>
    public PanelFade PanelFade
    {
        get
        {
            if (m_PanelFade == null)
            {
                //获取身上的Fade组件，如果没有则添加一个
                m_PanelFade = gameObject.GetOrAddComponent<PanelFade>();
            }
            return m_PanelFade;
        }
    }
    #endregion

    #region 初始化 显示 隐藏 界面
    /// <summary>
    /// 界面初始化
    /// </summary>
    public virtual void Init(params object[] objs) { }

    /// <summary>
    /// 对外的显示界面的函数
    /// </summary>
    /// <param name="hideEvent">页面关闭后的回调</param>
    /// <param name="hideCD">最快多久允许关闭界面</param>
    public virtual void Show(Action hideEvent = null, float hideCD = 0.5f)
    {        
        //如果当前界面显示着 或者是已经处于淡入状态了 就返回
        if (gameObject.activeInHierarchy || ShowDuration) return;

        //清理任务 和 设置状态 
        ClearTasks();
        HideCD = hideCD;
        ShowDuration = true;
        HideDuration = false;

        //如果当前处于时间暂停状态 则走时间暂停的逻辑 否则走普通逻辑
        if (Time.timeScale == 0)
        {
            DelayUnscaledTasks.Add(TimeMgr.Timer.AddUnscaledTimeTask(a => { DelayShow(hideEvent); }, ShowDelay * 1000));
        }
        else
        {
            DelayTasks.Add(TimeMgr.Timer.AddTimeTask(a => { DelayShow(hideEvent); }, ShowDelay * 1000));
        }
    }

    /// <summary>
    /// 真正的显示界面的逻辑
    /// </summary>
    /// <param name="hideEvent">页面关闭后的回调</param>
    protected virtual void DelayShow(Action hideEvent = null) { }

    /// <summary>
    /// 对外的关闭界面的函数
    /// </summary>
    public virtual void Hide()
    {
        //如果还不允许关闭 那么就不响应此次的关闭操作
        if (HideCD > 0) return;
        //如果当前界面隐藏着 或者是已经处于淡出状态了 就返回
        if (!gameObject.activeInHierarchy || HideDuration) return;

        //清理任务 和 设置状态 
        ClearTasks();
        HideDuration = true;
        ShowDuration = false;

        //如果当前处于时间暂停状态 则走时间暂停的逻辑 否则走普通逻辑
        if (Time.timeScale == 0)
        {
            DelayUnscaledTasks.Add(TimeMgr.Timer.AddUnscaledTimeTask(a => DelayHide(), HideDelay * 1000));
        }
        else
        {
            DelayTasks.Add(TimeMgr.Timer.AddTimeTask(a => DelayHide(), HideDelay * 1000));
        }

    }

    /// <summary>
    /// 真正的关闭界面的逻辑
    /// </summary>
    protected virtual void DelayHide() { }

    /// <summary>
    /// 界面淡入
    /// </summary>
    /// <param name="unscaled">不受时间缩放影响</param>
    /// <param name="endAction">淡入结束要做的事情</param>
    public virtual void FadeIn(bool unscaled = false, Action endAction = null)
    {
        //淡入完成后 修改状态
        endAction += () =>
        {
            ShowDuration = false;
            HideDuration = false;
        };
        PanelFade.FadeIn(FadeInTime, unscaled, endAction);
    }

    /// <summary>
    /// 界面淡出
    /// </summary>
    /// <param name="unscaled">不受时间缩放影响</param>
    /// <param name="endAction">淡出结束要做的事情</param>
    public virtual void FadeOut(bool unscaled = false, Action endAction = null)
    {
        //淡出完成后 修改状态
        endAction += () =>
        {
            ShowDuration = false;
            HideDuration = false;
        };
        PanelFade.HideTaskDelay = HideTaskDelay;
        PanelFade.FadeOut(FadeOutTime, unscaled, endAction);
    }
    #endregion

    #region 生命周期函数
    public virtual void Update()
    {
        //刷新CD 
        HideCD -= TimeMgr.RealDeltaTime;
    }

    public override void OnDisable()
    {
        //界面隐藏的时候 规整一下标识
        ShowDuration = false;
        HideDuration = false;
        base.OnDisable();
    }
    #endregion

}
