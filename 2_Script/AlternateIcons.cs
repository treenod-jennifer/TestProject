using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "AppIconChanger/AlternateIcons")]
public class AlternateIcons : ScriptableObject
{
    public AppIcon[] _icons;
}

[Serializable]
public class AppIcon
{
    public int        _version;
    public IconData[] _iconDatas;

    [Serializable]
    public class IconData
    {
        public string    _name;
        public Texture2D _texture;
    }
}
