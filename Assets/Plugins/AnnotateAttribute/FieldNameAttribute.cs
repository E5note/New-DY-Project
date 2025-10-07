// ******************************************************************
//       /\ /|       @file       FieldNameAttribute.cs
//       \ V/        @brief      Inspector字段命名
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2022-03-31 08:57:22
//    *(__\_\        @Copyright  Copyright (c) 2022, Shadowrabbit
// ******************************************************************

using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class FieldNameAttribute : PropertyAttribute
{
    public readonly string label; //要显示的字符
    public readonly string htmlColor; //颜色

    public FieldNameAttribute(string label)
    {
        this.label = label;
        htmlColor = "#FFFFFF";
    }

    public FieldNameAttribute(string label, string htmlColor)
    {
        this.label = label;
        this.htmlColor = htmlColor;
    }
}