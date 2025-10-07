using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;
/// <summary>
/// 界面淡入淡出工具
/// </summary>
public class PanelFade : MonoBehaviour
{
    #region 字段
    [FieldName("界面淡入用时")]
    public float FadeInTime = 0.3f;
    [FieldName("界面淡出用时")]
    public float FadeOutTime = 0.2f;
    [FieldName("显示时自动淡入")]
    public bool AutoFadeIn;
    [FieldName("自适应时间缩放")]
    public bool AutoSetTimeScale;

    [HideInInspector] public float HideTaskDelay;//关闭页面后，等待这个延时才会执行页面关闭的回调函数
    [HideInInspector] public bool m_FadeInFinish = true;//淡入效果是否已经结束， 如果之前淡入效果还未完成就进入淡出效果的话，需要先将之前的淡入动画的Tween给kill掉
    [HideInInspector] public bool m_FadeOutFinish = true;//同上

    private List<Image> m_Images = new List<Image>();//存储这个界面里所有的Image组件的引用
    private List<Color> m_ImageColors = new List<Color>();//存储这个界面里所有Image的颜色
    private List<bool> m_ImageRaycastTargets = new List<bool>();//存储这个界面所有Image组件是否接受射线检测的状态

    private List<Text> m_Texts = new List<Text>();
    private List<Color> m_TextColors = new List<Color>();
    private List<bool> m_TextRaycastTargets = new List<bool>();

    private List<TextMeshProUGUI> m_Meshpros = new List<TextMeshProUGUI>();
    private List<Color> m_MeshproColors = new List<Color>();
    private List<bool> m_MeshproRaycastTargets = new List<bool>();

    private int m_Task1;//不受时间缩放的延时任务ID
    private int m_Task2;//受时间缩放的延时任务ID
    //临时存储当前淡入或淡出效果的Tween动画，如果之前的淡入淡出效果还未彻底结束，就又要进入新的淡出淡出效果时，需要先将之前的Tween给kill掉，否则前后Tween动画叠加会出现bug
    private List<Tween> m_TempTweens = new List<Tween>();
    #endregion

    /// <summary>
    /// 获取这个物体以及子物体下所有Text和Image组件，以及他们的颜色状态和raycastTarget状态
    /// </summary>
    private void GetData()
    {
        //清空数组
        m_Images.Clear();
        m_ImageColors.Clear();
        m_ImageRaycastTargets.Clear();

        m_Texts.Clear();
        m_TextColors.Clear();
        m_TextRaycastTargets.Clear();

        m_Meshpros.Clear();
        m_MeshproColors.Clear();
        m_MeshproRaycastTargets.Clear();

        m_TempTweens.Clear();

        //获取Image数据
        m_Images = transform.GetTsInChildrenIncludeHide<Image>();
        m_Images.ForEach(a =>
        {
            m_ImageColors.Add(a.color);
            m_ImageRaycastTargets.Add(a.raycastTarget);
        });

        //获取Text数据
        m_Texts = transform.GetTsInChildrenIncludeHide<Text>();
        m_Texts.ForEach(a =>
        {
            m_TextColors.Add(a.color);
            m_TextRaycastTargets.Add(a.raycastTarget);
        });

        //获取TextMeshpro数据
        m_Meshpros = transform.GetTsInChildrenIncludeHide<TextMeshProUGUI>();
        m_Meshpros.ForEach(a =>
        {
            m_MeshproColors.Add(a.color);
            m_MeshproRaycastTargets.Add(a.raycastTarget);
        });

    }

    /// <summary>
    /// 淡入
    /// </summary>
    /// <param name="fadeInTime">淡入用时，-1则使用fade脚本上的时长</param>
    /// <param name="unscaled">不受时间缩放影响</param>
    /// <param name="finishCallBack">淡入结束事件</param>
    public void FadeIn(float fadeInTime = -1, bool unscaled = false, Action finishCallBack = null)
    {
        //如果淡入还没结束 就直接返回
        if (!m_FadeInFinish) return;
        //修改标识 显示这个界面
        m_FadeInFinish = false;
        gameObject.SetActive(true);

        //如果之前的淡出效果已经结束了 就重新获取一下数据，然后执行淡入逻辑
        if (m_FadeOutFinish)
        {
            GetData();
            RealFadeIn(fadeInTime, unscaled, finishCallBack);
        }
        else
        {
            //如果之前的淡出效果还未结束就进入了淡入效果 就先将之前淡出效果的tween动画全部杀死
            m_TempTweens.ForEach(a => a.Kill());
            m_TempTweens.Clear();
            //设置淡出标识为true ，因为上面我们强行将淡出效果结束掉了
            m_FadeOutFinish = true;

            //恢复组件的射线检测状态 因为淡出时会将射线检测统一关闭
            for (int i = 0; i < m_Images.Count; i++)
            {
                if (m_Images[i] != null)
                {
                    m_Images[i].raycastTarget = m_ImageRaycastTargets[i];
                }

            }
            for (int i = 0; i < m_Texts.Count; i++)
            {
                if (m_Texts[i] != null)
                {
                    m_Texts[i].raycastTarget = m_TextRaycastTargets[i];
                }

            }
            for (int i = 0; i < m_Meshpros.Count; i++)
            {
                if (m_Meshpros[i] != null)
                {
                    m_Meshpros[i].raycastTarget = m_MeshproRaycastTargets[i];
                }

            }
            //执行淡入逻辑
            RealFadeIn(fadeInTime, unscaled, finishCallBack);
        }

    }

    /// <summary>
    /// 真正的淡入逻辑
    /// </summary>
    /// <param name="fadeInTime"></param>
    /// <param name="unscaled"></param>
    /// <param name="finishCallBack"></param>
    private void RealFadeIn(float fadeInTime = -1, bool unscaled = false, Action finishCallBack = null)
    {
        //删除可能存在的旧的延时任务 注册新的延时任务，淡入结束修改标识以及执行淡入结束事件
        TimeMgr.Timer.DeleteTimeTask(m_Task1);
        TimeMgr.Timer.DeleteUnscaledTimeTask(m_Task2);
        if (!unscaled)
        {
            m_Task1 = TimeMgr.Timer.AddTimeTask(a => FadeInFinish(finishCallBack), 1000 * (fadeInTime == -1 ? FadeInTime : fadeInTime));
        }
        else
        {
            m_Task2 = TimeMgr.Timer.AddUnscaledTimeTask(a => FadeInFinish(finishCallBack), 1000 * (fadeInTime == -1 ? FadeInTime : fadeInTime));

        }

        //对所有显示着的 Text 和 Image MeshPro做淡入效果 
        for (int i = 0; i < m_Images.Count; i++)
        {
            //先将透明度设置为0
            Image image = m_Images[i];
            image.color = image.color.SetAlpha(0);
            //淡入
            Tween tween = image.DOFade(m_ImageColors[i].a, fadeInTime == -1 ? FadeInTime : fadeInTime).SetUpdate(unscaled).SetEase(Ease.Linear);
            m_TempTweens.Add(tween);
        }

        for (int i = 0; i < m_Texts.Count; i++)
        {
            Text text = m_Texts[i];
            text.color = text.color.SetAlpha(0);
            Tween tween = text.DOFade(m_TextColors[i].a, fadeInTime == -1 ? FadeInTime : fadeInTime).SetUpdate(unscaled).SetEase(Ease.Linear);
            m_TempTweens.Add(tween);
        }

        for (int i = 0; i < m_Meshpros.Count; i++)
        {
            TextMeshProUGUI text = m_Meshpros[i];
            text.color = text.color.SetAlpha(0);
            Tween tween = m_Meshpros[i].DOFade(m_MeshproColors[i].a, fadeInTime == -1 ? FadeInTime : fadeInTime).SetUpdate(unscaled).SetEase(Ease.Linear);
            m_TempTweens.Add(tween);
        }

    }

    /// <summary>
    /// 淡入结束
    /// </summary>
    /// <param name="finishCallBack">淡入结束的回调函数</param>
    private void FadeInFinish(Action finishCallBack = null)
    {
        if (m_FadeInFinish) return;
        m_FadeInFinish = true;
        finishCallBack?.Invoke();
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    /// <param name="fadeOutTime">淡出用时</param>
    /// <param name="unscaled">不受时间缩放影响</param>
    /// <param name="finishCallBack">结束事件</param>
    public void FadeOut(float fadeOutTime = -1, bool unscaled = false, Action finishCallBack = null)
    {
        //如果淡出还没结束 就直接返回
        if (!m_FadeOutFinish) return;
        //修改标识
        m_FadeOutFinish = false;
        //如果已经淡入完毕  就更新一下数据  因为淡入或者淡出还未结束时候的数据是有问题的，所以这些情况下都用上次记录的数据
        if (m_FadeInFinish)
        {
            GetData();
        }
        //如果还没有淡入完毕  就先将之前的tween动画杀死  然后在现有的alpha基础上淡出
        else
        {
            m_TempTweens.ForEach(a => a.Kill());
            m_TempTweens.Clear();
            m_FadeInFinish = true;
        }

        //淡出过程中，禁用一下射线检测 让这个页面处于不可交互的状态，避免各种乱七八糟的bug
        m_Images.ForEach(a => a.raycastTarget = false);
        m_Texts.ForEach(a => a.raycastTarget = false);
        m_Meshpros.ForEach(a => a.raycastTarget = false);

        //删除可能存在的旧的延时任务 注册新的延时任务，淡出结束修改标识以及执行淡出结束事件
        TimeMgr.Timer.DeleteTimeTask(m_Task1);
        TimeMgr.Timer.DeleteUnscaledTimeTask(m_Task2);
        if (!unscaled)
        {
            m_Task1 = TimeMgr.Timer.AddTimeTask(a => FadeOutFinish(), 1000 * (fadeOutTime == -1 ? FadeOutTime : fadeOutTime));
        }
        else
        {
            m_Task2 = TimeMgr.Timer.AddUnscaledTimeTask(a => FadeOutFinish(), 1000 * (fadeOutTime == -1 ? FadeOutTime : fadeOutTime));
        }

        //让数组里所有的Image 和Text 淡出
        foreach (var item in m_Images)
        {
            Tween tween = item.DOFade(0, fadeOutTime == -1 ? FadeOutTime : fadeOutTime).SetUpdate(unscaled).SetEase(Ease.Linear);
            m_TempTweens.Add(tween);
        }
        foreach (var item in m_Texts)
        {
            Tween tween = item.DOFade(0, fadeOutTime == -1 ? FadeOutTime : fadeOutTime).SetUpdate(unscaled).SetEase(Ease.Linear);
            m_TempTweens.Add(tween);
        }
        foreach (var item in m_Meshpros)
        {
            Tween tween = item.DOFade(0, fadeOutTime == -1 ? FadeOutTime : fadeOutTime).SetUpdate(unscaled).SetEase(Ease.Linear);
            m_TempTweens.Add(tween);
        }

    }

    /// <summary>
    /// 淡出结束
    /// </summary>
    /// <param name="finishCallBack">淡出结束回调函数</param>
    private void FadeOutFinish(Action finishCallBack = null)
    {
        if (m_FadeOutFinish) return;
        m_FadeOutFinish = true;
        HideAndReset();
        if (Time.timeScale == 0)
        {
            m_Task2 = TimeMgr.Timer.AddUnscaledTimeTask(a =>
             {
                 finishCallBack?.Invoke();
             }, HideTaskDelay * 1000);
        }
        else
        {
            m_Task1 = TimeMgr.Timer.AddTimeTask(a =>
             {
                 finishCallBack?.Invoke();
             }, HideTaskDelay * 1000);
        }
    }

    /// <summary>
    /// 隐藏界面以及重置数据
    /// </summary>
    /// <param name="hide">是否隐藏界面</param>
    private void HideAndReset(bool hide = true)
    {
        //清理可能残留的动画
        m_TempTweens.ForEach(a => a.Kill());
        m_TempTweens.Clear();

        //隐藏界面
        if (hide) gameObject.SetActive(false);

        //将颜色 和 射线检测状态复原回去  
        for (int i = 0; i < m_Images.Count; i++)
        {
            m_Images[i].color = m_ImageColors[i];
            m_Images[i].raycastTarget = m_ImageRaycastTargets[i];
        }
        for (int i = 0; i < m_Texts.Count; i++)
        {
            m_Texts[i].color = m_TextColors[i];
            m_Texts[i].raycastTarget = m_TextRaycastTargets[i];
        }
        for (int i = 0; i < m_Meshpros.Count; i++)
        {
            m_Meshpros[i].color = m_MeshproColors[i];
            m_Meshpros[i].raycastTarget = m_MeshproRaycastTargets[i];
        }

    }

    /// <summary>
    /// 界面显示的时候，如果勾选了自动淡入，那么就自动执行淡入逻辑
    /// </summary>
    private void OnEnable()
    {
        if (AutoFadeIn)
        {
            if (AutoSetTimeScale)
            {
                FadeIn(unscaled: Time.timeScale == 0);
            }
            else
            {
                FadeIn();
            }
        }
    }

    /// <summary>
    /// 界面隐藏的时候 把元素状态重置规整一下
    /// </summary>
    private void OnDisable()
    {
        HideAndReset();
    }

}
