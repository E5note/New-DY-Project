using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 用于记录判断结果的数据类
/// </summary>
public class Mark
{
    public int Result = 0; // 0是False 1是True
}

/// <summary>
/// 事件项，包含委托和优先级
/// </summary>
public class EventItem
{
    public Func<object[], object> Action;
    public int Priority;
    
    public EventItem(Func<object[], object> action, int priority = 0)
    {
        Action = action;
        Priority = priority;
    }
}

/// <summary>
/// 增强版事件管理器，支持自定义执行顺序
/// </summary>
public class EventMgr
{
    /// <summary>
    /// 存储全部事件的字典，键是事件名，值是事件项列表
    /// </summary>
    private static Dictionary<string, List<EventItem>> m_Events = 
        new Dictionary<string, List<EventItem>>();

    /// <summary>
    /// 注册事件（带优先级）
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="func">委托函数</param>
    /// <param name="priority">优先级，数字越小优先级越高</param>
    public static void RegisterEvent(string eventName, Func<object[], object> func, int priority = 0)
    {
        if (!m_Events.ContainsKey(eventName))
        {
            m_Events.Add(eventName, new List<EventItem>());
        }
        
        var eventItem = new EventItem(func, priority);
        m_Events[eventName].Add(eventItem);
        
        // 按优先级排序（优先级数字小的先执行）
        m_Events[eventName] = m_Events[eventName]
            .OrderBy(e => e.Priority)
            .ToList();
    }

    /// <summary>
    /// 执行事件（按优先级顺序执行）
    /// </summary>
    public static object ExecuteEvent(string eventName, object[] param = null, int ignoreParamsCount = 0)
    {
        if (!m_Events.ContainsKey(eventName)) return null;
        
        var eventItems = m_Events[eventName];
        if (eventItems == null || eventItems.Count == 0) return null;

        object result = null;
        
        foreach (var eventItem in eventItems)
        {
            if (eventItem.Action == null) continue;
            
            try
            {
                if (ignoreParamsCount == 0)
                {
                    if (param != null && param.Length > 0 && param[param.Length - 1] is Mark mark)
                    {
                        eventItem.Action(param);
                        result = mark.Result == 1;
                    }
                    else
                    {
                        result = eventItem.Action(param);
                    }
                }
                else
                {
                    var args = (param == null || param.Length == 0) 
                        ? param 
                        : param.Skip(ignoreParamsCount).ToArray();
                        
                    if (args != null && args.Length > 0 && args[args.Length - 1] is Mark mark)
                    {
                        eventItem.Action(args);
                        result = mark.Result == 1;
                    }
                    else
                    {
                        result = eventItem.Action(args);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"执行事件{eventName}时出错: {e}");
            }
        }
        
        return result;
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    public static void UnRegisterEvent(string eventName, Func<object[], object> func)
    {
        if (!m_Events.ContainsKey(eventName)) return;
        
        var itemToRemove = m_Events[eventName].FirstOrDefault(e => e.Action == func);
        if (itemToRemove != null)
        {
            m_Events[eventName].Remove(itemToRemove);
        }
    }
}