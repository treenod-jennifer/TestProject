using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pos
{
    int _x;
    int _y;

    public int x
    {
        set { _x = value; }
        get { return _x; }
    }

    public int y
    {
        set { _y = value; }
        get { return _y; }
    }

    public Pos() { }
    public Pos(int in_x, int in_y)
    {
        x = in_x;
        y = in_y;
    }
}
