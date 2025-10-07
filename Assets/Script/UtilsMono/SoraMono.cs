using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// 扩展Mono类
/// </summary>
public class SoraMono : MonoBehaviour
{
    //存放这个类开启的tween动画、延时任务引用的数组，方便销毁和禁用时统一清理
    protected List<Tween> Tweens = new List<Tween>();
    protected List<int> DelayTasks = new List<int>();
    protected List<int> DelayUnscaledTasks = new List<int>();
    protected List<int> DelayFrameTasks = new List<int>();
    protected List<int> DelayFixedFrameTasks = new List<int>();

    public virtual void ClearTasks()
    {
        //关闭全部可能存在的协程
        StopAllCoroutines();
        //杀死全部可能存在的Tween动画
        Tweens.ForEach(a => a.Kill());
        //清除全部可能存在的延时任务
        DelayTasks.ForEach(a =>
        {
            TimeMgr.Timer.DeleteTimeTask(a);
        });
        DelayUnscaledTasks.ForEach(a =>
        {
            TimeMgr.Timer.DeleteUnscaledTimeTask(a);
        });
        DelayFrameTasks.ForEach(a =>
        {
            TimeMgr.Timer.DeleteFrameTask(a);
        });
        DelayFixedFrameTasks.ForEach(a =>
        {
            TimeMgr.Timer.DeleteFixedFrameTask(a);
        });

        Tweens.Clear();
        DelayTasks.Clear();
        DelayUnscaledTasks.Clear();
        DelayFrameTasks.Clear();
        DelayFixedFrameTasks.Clear();
    }

    public virtual void OnDisable()
    {
        ClearTasks();
    }

    public virtual void OnDestroy() { }

}
