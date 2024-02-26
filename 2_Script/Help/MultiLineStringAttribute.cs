using UnityEngine;

public class MultiLineStringAttribute : PropertyAttribute
{
    public float height = 0.0f;

    public MultiLineStringAttribute(float _height)
    {
        height = _height;
    }
}