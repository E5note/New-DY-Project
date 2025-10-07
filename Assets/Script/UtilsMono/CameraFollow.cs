using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 2D相机跟随 
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("相机坐标范围")]
    public Vector4 Range;
    [FieldName("相机视野范围")]
    public Vector2 SizeRange;
    [FieldName("视野变化平滑度")]
    public float SizeSmooth;
    [FieldName("与跟随目标的偏移，Z轴偏移一般为-10")]
    public Vector3 Offset;
    [FieldName("跟随平滑度")]
    public float FollowSmooth;
    [HideInInspector] public Transform Target;//要跟随的目标
    [HideInInspector] public bool Follow;//启用相机跟随效果(演出中一般会禁用跟随)
    [HideInInspector] public float TargetSize;//目标视野，会从当前视野根据平滑度过度到目标视野

    private Camera m_Camera;//相机组件引用
    private Vector3 m_InitPos;//相机初始坐标
    private float m_InitSize;//相机初始视野大小

    public static CameraFollow Instance;
    private void Awake()
    {
        //初始化 记录初始值 注册事件
        Instance = this;
        m_Camera = Camera.main;
        m_InitPos = transform.position;
        m_InitSize = m_Camera.orthographicSize;
        TargetSize = m_InitSize;
        EventMgr.RegisterEvent(EventName.InitGame, ResetGame);
    }

    private void OnDestroy()
    {
        EventMgr.UnRegisterEvent(EventName.InitGame, ResetGame);
    }

    private void LateUpdate()
    {
        //如果当前没开启跟随 或者 跟随目标不存在 就返回
        if (!Follow || Target == null) return;
        //相机跟随
        transform.position = Vector3.Lerp(transform.position, Target.position + Offset, Time.deltaTime / FollowSmooth);
        //相机视野大小调整过度
        m_Camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(m_Camera.orthographicSize, TargetSize, Time.deltaTime / SizeSmooth), SizeRange.x, SizeRange.y);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, Range.z, Range.w), Mathf.Clamp(transform.position.y, Range.y, Range.x), transform.position.z);
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private object ResetGame(object[] arg)
    {
        transform.position = m_InitPos;
        m_Camera.orthographicSize = m_InitSize;
        TargetSize = m_InitSize;
        return null;
    }

}
