using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Vector2Int 
{
    private int x_pos;
    private int y_pos;
    public Vector2Int(int x, int y)
    {
        x_pos = x;
        y_pos = y;
    }
    public int getX()
    {
        return x_pos;
    }
    public int getY()
    {
        return y_pos;
    }
    public void setX(int x)
    {
        x_pos = x;
    }
    public void setY(int y)
    {
        y_pos = y;
    }
    public bool Equals(Vector2Int other)
    {
        return other.getX() == x_pos && other.getY() == y_pos;
    }
}
