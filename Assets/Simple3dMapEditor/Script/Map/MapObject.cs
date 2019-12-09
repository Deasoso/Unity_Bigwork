using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class MapData
{
    string _path;
    Node   _node;
    
    public string ResPath { get { return _path; } }
    public Node Node      { get { return _node; } }

    public MapData(string path, Node node)
    {
        _path   = path;
        _node   = node;
    }
}

public class MapObject : MonoBehaviour
{
    readonly float selectionSize = 0.7f;

    MapData _mapData;

    Vector3 _originScale;

    [SerializeField]
    int _offsetIndex = 0;
    [SerializeField]
    int _gridIndex   = 0;

    [SerializeField]
    bool _isSelection = false;
    
    public bool IsSelection
    {
        get { return _isSelection; }
        set
        {
            _isSelection = value;
            if (_isSelection)
                Selection();
            else
                DeSelection();
        }
    }
    public MapData Data
    {
        get
        {
            if (_mapData != null)
                return _mapData;

            return null;
        }
    }
    
    public void Initialized(string path, Node node)
    {
        _mapData     = new MapData(path, node);

        _offsetIndex = _mapData.Node.OffsetIndex;
        _gridIndex   = _mapData.Node.GridIndex;

        _originScale = transform.lossyScale;
    }

    void Selection()
    {
        transform.localScale = _originScale *  selectionSize;
    }
    void DeSelection()
    {
        transform.localScale = _originScale;
    }
}
