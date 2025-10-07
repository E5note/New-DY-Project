using System.Text;
using Script.Mgr;

/// <summary>
/// 日志工具，尽量通过这个工具输出各种日志(个人还是习惯性用print)，好方便统一管理
/// </summary>
public static class LogUtil
{
    /// <summary>
    /// 是否输出Debug级别的日志，测试bug时可以输出，打包时可以关闭，因为Debug级别的日志含金量低，没必要让玩家看到以及占用性能
    /// </summary>
    public static bool DebugMode
    {
        get => GameManager.Instance.logDebug;
    }

    #region 一般日志(用于测试的随意输出的内容)
    /// <summary>
    /// 输出一条日志
    /// </summary>
    /// <param name="obj">日志内容</param>
    public static void Debug(object obj)
    {
        if (!DebugMode) return;
        Debug(obj.ToString());
    }

    /// <summary>
    /// 输出一条日志
    /// </summary>
    /// <param name="separator">分隔符</param>
    /// <param name="objs">日志内容，输出时参数之间会用给定的分割符隔开</param>
    public static void Debug(string separator = "", params object[] objs)
    {
        if (!DebugMode) return;
        Debug(CombiningStrings(objs, separator));
    }

    /// <summary>
    /// 输出一条日志
    /// </summary>
    /// <param name="msg">日志内容</param>
    public static void Debug(string msg)
    {
        if (!DebugMode) return;
        UnityEngine.Debug.Log(msg);
    }
    #endregion

    #region 重要信息(一些关键节点，比如场景切换完毕，大演出完毕) 
    /// <summary>
    /// 输出一条重要信息
    /// </summary>
    /// <param name="obj">日志内容</param>
    public static void Info(object obj)
    {
        Info(obj.ToString());
    }

    /// <summary>
    /// 输出一条重要信息
    /// </summary>
    /// <param name="separator">分隔符</param>
    /// <param name="objs">信息内容，输出时参数之间会用给定的分割符隔开</param>
    public static void Info(string separator = "", params object[] objs)
    {
        Info(CombiningStrings(objs, separator));
    }

    /// <summary>
    /// 输出一条重要信息
    /// </summary>
    /// <param name="msg">信息内容</param>
    public static void Info(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }
    #endregion

    #region 警告 和 错误
    /// <summary>
    /// 输出一条警告
    /// </summary>
    /// <param name="obj">警告内容</param>
    public static void Warning(object obj)
    {
        Warning(obj.ToString());
    }

    /// <summary>
    /// 输出一条警告
    /// </summary>
    /// <param name="separator">分隔符</param>
    /// <param name="objs">警告内容，输出时参数之间会用给定的分割符隔开</param>
    public static void Warning(string separator = "", params object[] objs)
    {
        Warning(CombiningStrings(objs, separator));
    }

    /// <summary>
    /// 输出一条警告
    /// </summary>
    /// <param name="msg">警告内容</param>
    public static void Warning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
    }

    /// <summary>
    /// 输出一条错误提示
    /// </summary>
    /// <param name="obj">错误内容</param>
    public static void Error(object obj)
    {
        Error(obj.ToString());
    }

    /// <summary>
    /// 输出一条错误提示
    /// </summary>
    /// <param name="objs">错误内容，输出时参数之间会用给定的分割符隔开</param>
    public static void Error(string separator = "", params object[] objs)
    {
        Error(CombiningStrings(objs, separator));
    }

    /// <summary>
    /// 输出一条错误提示
    /// </summary>
    /// <param name="msg">错误内容</param>
    public static void Error(string msg)
    {
        UnityEngine.Debug.LogError(msg);
    }
    #endregion

    /// <summary>
    /// 将数组里的元素合并成一个字符串用给定的分隔符
    /// </summary>
    /// <param name="objs">数组</param>
    /// <param name="separator">元素之间的分隔符</param>
    /// <returns></returns>
    public static string CombiningStrings(object[] objs, string separator = "")
    {
        //用Stringbuilder拼接字符串似乎性能更好，底层可以考虑用这种方式拼接字符串
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < objs.Length; i++)
        {
            //不是最后一个元素 就在末尾添加一个分隔符
            if (i != objs.Length - 1)
            {
                sb.Append(objs[i].ToString() + separator);
            }
            else
            {
                sb.Append(objs[i].ToString());
            }

        }
        return sb.ToString();
    }

}
