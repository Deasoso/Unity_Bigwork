using System;
using UnityEngine;
using System.Collections;


[Serializable]
public class Node
{
    int _gridIndex;  //gridIndex
    int _offsetIndex;
    float _worldPositionX;
    float _worldPositionY;
    float _worldPositionZ;

    int _gridX;
    int _gridY;
    
    int _index;  //Old Version
    public int Index { get { return _index; } set { _index = value; } } //Old Version

    public int OffsetIndex { get { return _offsetIndex; } set { _offsetIndex = value; } }
    public int GridIndex   { get { return _gridIndex; } set { _gridIndex = value; } }
    public int GridX       { get { return _gridX; } set { _gridX = value; } }
    public int GridY       { get { return _gridY; } set { _gridY = value; } }

    public Vector3 WorldPosition {
        get { return new Vector3(_worldPositionX, _worldPositionY, _worldPositionZ); }
        set
        {
            _worldPositionX = value.x;
            _worldPositionY = value.y;
            _worldPositionZ = value.z;
        }
    }
    
    public Node(int offsetIndex, int gridIndex,Vector3 wordPos, int gridX, int gridY)
    {
        _offsetIndex   = offsetIndex;
        _gridIndex     = gridIndex;
        
        _worldPositionX = wordPos.x;
        _worldPositionY = wordPos.y;
        _worldPositionZ = wordPos.z;

        _gridX = gridX;
        _gridY = gridY;
    }
}
