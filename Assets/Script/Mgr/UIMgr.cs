using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// UI管理器
/// </summary>
public class UIMgr : MonoBehaviour
{
    [Header("界面")]
    public List<PanelBase> Panels;
    [Tooltip("淡入淡出画面用的遮罩，层级应该最高，可以盖住全部界面")]
    public Image FadeMask;
    private Tween m_FadeMaskTween;//淡入淡出的tween动画，每次淡入淡出之前都需要先关闭一下之前可能存在的tween动画，防止新旧动画叠加
    //简单单例模式
    public static UIMgr Instance;
    private void Awake()
    {
        Instance = this;
        //页面初始化 一般是注册一下这个页面的一些输入监听事件
        Panels.ForEach(a => a.Init());
    }

    /// <summary>
    /// 用淡入淡出遮罩的方式，隐藏旧界面显示新界面
    /// </summary>
    /// <param name="hidePage">需要隐藏的界面，会在纯黑时隐藏</param>
    /// <param name="showPage">需要显示的界面，会在纯黑时显示</param>
    /// <param name="action">回调函数，会在纯黑时执行的回调</param>
    /// <param name="fadeInTime">淡入用时</param>
    /// <param name="stayTime">停留用时</param>
    /// <param name="fadeOutTime">淡出用时</param>
    /// <param name="maskColor">遮罩颜色，默认为纯黑</param>
    /// <param name="unscaled">不受时间缩放影响</param>
    public void ShowPage(List<GameObject> hidePage, GameObject showPage, Action action = null, float fadeInTime = 1, float stayTime = 0, float fadeOutTime = 1, Color maskColor = default, bool unscaled = false)
    {
        //关闭之前可能存在的tween动画 防止新旧动画叠加错乱
        m_FadeMaskTween.Kill();
        //规整颜色 遮罩淡入
        FadeMask.color = maskColor == default ? new Color(0, 0, 0, 0) : maskColor;
        FadeMask.gameObject.SetActive(true);
        m_FadeMaskTween = FadeMask.DOFade(1, fadeInTime).SetUpdate(unscaled);
        m_FadeMaskTween.onComplete += () =>
        {
            //遮罩淡入完毕后，隐藏需要隐藏的界面，显示需要显示的界面
            hidePage.ForEach(a => a.Hide());
            showPage?.SetActive(true);
            //执行回调函数
            action?.Invoke();
            //等待一个时长后淡出遮罩
            if (unscaled)
            {
                TimeMgr.Timer.AddUnscaledTimeTask(a =>
                {
                    m_FadeMaskTween = FadeMask.DOFade(0, fadeOutTime).SetUpdate(true);
                }, stayTime * 1000);
            }
            else
            {
                TimeMgr.Timer.AddTimeTask(a =>
                {
                    m_FadeMaskTween = FadeMask.DOFade(0, fadeOutTime).SetUpdate(false);
                }, stayTime * 1000);
            }
        };
    }
}
