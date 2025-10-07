using UnityEngine;
using System.Collections.Generic;
using System;
/// <summary>
/// *27Csv文件解析工具
/// </summary>
public class CsvReader
{
    private struct RowText
    {
        public string[] colValueArray; //列值数组
    }

    private readonly List<RowText> _rowTextList = new List<RowText>(); //行文本

    /// <summary>
    /// 加载数据文件
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    /// <param name="skipLine">跳过的行数 前3行特殊字段</param>
    public async void LoadText(string assetPath, int skipLine)
    {
        var textAsset = await AssetMgr.LoadAssetAsync<TextAsset>(assetPath, AssetLoadType.Permanent);

        var strText = textAsset.text;
        if (strText == null)
        {
            Debug.LogError($"数据读取失败:{assetPath}");
            return;
        }

        var lines = strText.Split('\n');
        var iStartLine = Mathf.Max(0, skipLine);
        for (var i = iStartLine; i < lines.Length; i++)
        {
            //去掉数据分隔符没有任何数据 忽略
            if (string.IsNullOrWhiteSpace(lines[i].Trim('#')))
            {
                continue;
            }

            ParseLine(lines[i]);
        }
    }

    /// <summary>
    /// 解析当前行
    /// </summary>
    /// <param name="lineText"></param>
    private void ParseLine(string lineText)
    {
        var colValue = lineText.Split('#'); //当前行的数据列数
        var item = new RowText
        {
            colValueArray = new string[colValue.Length]
        };

        //当前行每列的数据
        for (var i = 0; i < colValue.Length; i++)
        {
            item.colValueArray[i] = colValue[i].Trim();
        }
        _rowTextList.Add(item);

    }

    /// <summary>
    /// 获取表行数
    /// </summary>
    /// <returns></returns>
    public int GetRowCount()
    {
        return _rowTextList.Count;
    }

    /// <summary>
    /// 获取某行的列数据数组
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public string[] GetColValueArray(int row)
    {
        return _rowTextList[row].colValueArray;
    }
   
    
    /// <summary>
    /// 从 TextAsset 的文本内容中加载数据
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="skipLine">跳过行数（如3）</param>
    public void LoadTextFromString(string text, int skipLine)
    {
        _rowTextList.Clear();

        var lines = text.Split('\n');
        var iStartLine = Mathf.Max(0, skipLine);
        for (var i = iStartLine; i < lines.Length; i++)
        {
            // 忽略空行或注释行
            if (string.IsNullOrWhiteSpace(lines[i].Trim('#')))
            {
                continue;
            }

            ParseLine(lines[i]);
        }
    }

}

public static class CsvUtility
{
    private const string EmptyStr = "None"; //空字符

    /// <summary>
    /// 字符串转换为整型
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ToInt(string str)
    {
        if (str.Equals(EmptyStr))
        {
            return 0;
        }

        try
        {
            return str.ToInt();
        }
        catch (Exception ex)
        {
            LogUtil.Warning("字符串转换为整形失败:"+str);
            return 0;
        }
    }

    /// <summary>
    /// 字符串转换成浮点型 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static float ToFloat(string str)
    {
        if (str.Equals(EmptyStr))
        {
            return 0;
        }

        try
        {
            return str.ToFloat();
        }
        catch (Exception ex)
        {
            LogUtil.Warning(ex.ToString());
            return 0;
        }
    }

    /// <summary>
    /// 转换为布尔型
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool ToBool(string str)
    {
        if (str.Equals(EmptyStr))
        {
            return false;
        }

        try
        {
            return str.ToBool();
        }
        catch (Exception ex)
        {
            LogUtil.Warning(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// 转换为二维向量
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Vector2 ToVector2(string str)
    {
        if (str.Equals(EmptyStr))
        {
            return Vector2.zero;
        }

        try
        {
            return str.ToVector2();
        }
        catch (Exception ex)
        {
            LogUtil.Warning(ex.ToString());
            return Vector2.zero;
        }
    }

    /// <summary>
    /// 转换为三维向量
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Vector3 ToVector3(string str)
    {
        if (str.Equals(EmptyStr))
        {
            return Vector3.zero;
        }

        try
        {
            return str.ToVector3();
        }
        catch (Exception ex)
        {
            LogUtil.Warning(ex.ToString());
            return Vector3.zero;
        }
    }

    /// <summary>
    /// 规整字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToString(string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Equals("None"))
        {
            return "";
        }
        return str;
    }

    /// <summary>
    /// 转换为泛型字典
    /// </summary>
    /// <param name="str"></param>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    /// <returns></returns>
    public static Dictionary<TK, TV> ToDictionary<TK, TV>(string str)
    {
        var dict = new Dictionary<TK, TV>();
        if (str.Equals(EmptyStr))
        {
            return dict;
        }
      
        //键值对"k:v"
        var kvPairsStrArray = str.Split(str.Contains("，") ? '，' : ',');
        foreach (var kvPairStr in kvPairsStrArray)
        {
            var kvPair = kvPairStr.Split(str.Contains(":")?':':'：');
            if (kvPair.Length != 2)
            {
                Debug.LogError($"解析字典失败:{str}");
                continue;
            }

            var k = ToT<TK>(kvPair[0]);
            var v = ToT<TV>(kvPair[1]);
            if (!dict.ContainsKey(k))
            {
                dict.Add(k, v);
            }
        }

        return dict;
    }

    /// <summary>
    /// 转换为泛型动态数组
    /// </summary>
    /// <param name="str"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> ToList<T>(string str)
    {
        var list = new List<T>();
        if (str.Equals(EmptyStr))
        {
            return list;
        }  
        
        var valueStrArray = str.Split(str.Contains("，")? '，': ',');
        foreach (var valueStr in valueStrArray)
        {
            var v = ToT<T>(valueStr);
            list.Add(v);
        }

        return list;
    }

    /// <summary>
    /// 泛型解析
    /// </summary>
    /// <param name="readNextCol"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ToT<T>(this string readNextCol)
    {
        object value = null;
        if (typeof(T) == typeof(int))
        {
            value = ToInt(readNextCol);
        }
        else if (typeof(T) == typeof(float))
        {
            value = ToFloat(readNextCol);
        }
        else if (typeof(T) == typeof(string))
        {
            value = ToString(readNextCol);
        }
        else if (typeof(T) == typeof(bool))
        {
            value = ToBool(readNextCol);
        }
        else if (typeof(T) == typeof(Vector2))
        {
            value = ToVector2(readNextCol);
        }
        else if (typeof(T) == typeof(Vector3))
        {
            value = ToVector3(readNextCol);
        }

        return (T) value;
    }
}

public class RowHelper
{
    private readonly string[] _col; //当前行的每列数据
    private int _idx; //当前索引

    public RowHelper(string[] rs)
    {
        _col = rs;
        _idx = 0;
    }

    /// <summary>
    /// 读取下一列
    /// </summary>
    /// <returns></returns>
    public string ReadNextCol()
    {
        return _col[_idx++];
    }
}