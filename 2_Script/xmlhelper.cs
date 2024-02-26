using UnityEngine;
using System.Collections;
using System.Xml;
using System;

public class xmlhelper
{
    public static int GetChildNodeCountByName(XmlNode node, string nodeName)
    {
        int found = 0;
        for(int i = 0; i < node.ChildNodes.Count; ++i)
        {
            if (node.ChildNodes[i].Name == nodeName)
                found++;
        }
        return found;
    }

    public static int GetInt(XmlNode node, string attrName, int def = 0)
    {
        if (node.Attributes[attrName] == null)
        {
            return def;
        }
        int ret = 0;
        try
        {
            ret = Convert.ToInt32(node.Attributes[attrName].Value);
        }
        catch (Exception e)
        {
            return def;
        }

        return ret;

    }

    public static bool GetBool(XmlNode node, string attrName, bool def = false)
    {
        if (node.Attributes[attrName] == null)
        {
            return def;
        }
        bool ret = def;
        try
        {
            ret = Convert.ToBoolean(node.Attributes[attrName].Value);
        }
        catch (Exception e)
        {
            return def;
        }

        return ret;

    }

    public static float GetFloat(XmlNode node, string attrName, float def = 0)
    {
        if (node.Attributes[attrName] == null)
        {
            return def;
        }
        float ret = 0;
        try
        {
            ret = Convert.ToSingle(node.Attributes[attrName].Value);
        }
        catch (Exception e)
        {
            return def;
        }

        return ret;

    }

    public static string GetString(XmlNode node, string attrName, string def = "")
    {
        if (node.Attributes[attrName] == null)
        {
            return def;
        }
        string ret;
        ret = node.Attributes[attrName].Value;

        return ret;

    }

    public static Vector2 GetVector2(XmlNode node, string attrName)
    {
        Vector2 ret = new Vector2();
        if (node.Attributes[attrName] == null)
        {
            return ret;
        }

        string valString = node.Attributes[attrName].Value;
        string[] xyStr = valString.Split(',');
        if (xyStr.Length < 2)
            return ret;

        try
        {
            ret.x = Convert.ToSingle(xyStr[0]);
            ret.y = Convert.ToSingle(xyStr[1]);
        }
        catch (Exception e)
        {
            return ret;
        }

        return ret;
    }

    public static Vector3 GetVector3(XmlNode node, string attrName)
    {
        Vector3 ret = new Vector3();
        if (node.Attributes[attrName] == null)
        {
            return ret;
        }

        string valString = node.Attributes[attrName].Value;
        string[] xyStr = valString.Split(',');
        if (xyStr.Length < 2)
            return ret;

        try
        {
            ret.x = Convert.ToSingle(xyStr[0]);
            ret.y = Convert.ToSingle(xyStr[1]);
            if (xyStr.Length >= 3)
                ret.z = Convert.ToSingle(xyStr[2]);
            else ret.z = 0.0f;
        }
        catch (Exception e)
        {
            return ret;
        }

        return ret;
    }

    public static Color GetColor(XmlNode node, string attrName)
    {
        Color ret = new Color(1f, 1f, 1f, 1f);
        if (node.Attributes[attrName] == null)
        {
            return ret;
        }

        string valString = node.Attributes[attrName].Value;
        string[] xyStr = valString.Split(',');
        if (xyStr.Length < 3)
            return ret;

        try
        {
            ret.r = Convert.ToSingle(xyStr[0]);
            ret.g = Convert.ToSingle(xyStr[1]);
            ret.b = Convert.ToSingle(xyStr[2]);
            if (xyStr.Length >= 4)
                ret.a = Convert.ToSingle(xyStr[3]);
            else ret.a = 1.0f;
        }
        catch (Exception e)
        {
            return ret;
        }

        return ret;
    }

    public static Color GetHexColor(XmlNode node, string attrName)
    {   
        Color ret = new Color(1f, 1f, 1f, 1f);
        if (node.Attributes[attrName] == null)
        {
            return ret;
        }
        string valString = node.Attributes[attrName].Value;
        int valInt = Convert.ToInt32(valString, 16);
        try
        {
            ret.r = ((float)((valInt & 0xFF000000) >> 24)) / 255f;
            ret.g = ((float)((valInt & 0xFF0000) >> 16)) / 255f;
            ret.b = ((float)((valInt & 0xFF00) >> 8)) / 255f;
            ret.a = 1f;
        }
        catch (Exception e)
        {
            return ret;
        }

        return ret;
    }
}
