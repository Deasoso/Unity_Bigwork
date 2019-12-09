using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

using MapObjectList = System.Collections.Generic.List<MapObject>;
using MapObjectDic  = System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<MapObject>>;
using NodeDic       = System.Collections.Generic.Dictionary<int, Node[,]>;

public class MapGenerator : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    public string ResourcePath = "";
    [HideInInspector]
    [SerializeField]
    public string FilePath     = "";
    public Ray ray;

    [HideInInspector]
    [SerializeField]
    bool _isEditMode      = false;

    [HideInInspector]
    [SerializeField]
    bool _isDispalyGrid   = true;

    [HideInInspector]
    [SerializeField]
    bool _isVertical      = true;

    [HideInInspector]
    [SerializeField]
    int _offsetMax        = 50;

    [HideInInspector]
    [SerializeField]
    int _prevOffsetMax    = 50;

    [HideInInspector]
    [SerializeField]
    Vector2 _gridWorldSize= new Vector2(10f,10f);

    [HideInInspector]
    [SerializeField]
    Vector2 _prevWorldSize= new Vector2(10f, 10f);

    [HideInInspector]
    [SerializeField]
    float _nodeRadius     = 0.5f;

    [HideInInspector]
    [SerializeField]
    float _prevNodeRadius = 0.5f;

    float _currentOffsetValue = 0;
    float _prevOffsetValue= 0;

    bool _isInitGrid = false;
    int _gridSizeX, _gridSizeY;
    Vector3 _orginPosition = Vector3.zero;
    
    bool _isOnlyGrid = false;

    Vector3 Direction          { get { return _isVertical ? Vector3.up : Vector3.forward; } }
    Vector3 Offset             { get { return _isVertical ? Vector3.forward * _currentOffsetValue : Vector3.up * _currentOffsetValue; } }


    public bool IsEditMode     { get { return _isEditMode; } set { _isEditMode = value; } }
    public bool IsDispalyGrid  { get { return _isDispalyGrid; } set { _isDispalyGrid = value; } }
    public bool IsVertical     { get { return _isVertical; } set { _isVertical = value; }  }
    public bool IsOnlyGrid     { set { _isOnlyGrid = value; } }
    public int OffsetMax       { get { return _offsetMax; } set { _offsetMax = value; } }
    public int MaxSize         { get { return _gridSizeX * _gridSizeY; } }
    public int OffsetIndex     { get { return Mathf.Max((int)(_currentOffsetValue / (_nodeRadius * 2f)),0); } }
    public float NodeRadius    { get { return _nodeRadius; } set { _nodeRadius = value; } }

    public Vector2 GridWorldSize { get { return _gridWorldSize; } set { _gridWorldSize = value; } }

    public Vector3 SelectPoint  { set; get; }
    public Node SpawnNode       { set; get; }
    public Node SelectNode      { set; get; }
    
    public List<Node> SelectNodes { set; get; }
    
    public MapObjectDic MapObjectDictionary { set; get; }
    public NodeDic NodeDictionary { set; get; }
    public void ResetGrid()
    {
        if (MapObjectDictionary == null)
            MapObjectDictionary = new MapObjectDic();
        
        //World size Limit
        _gridWorldSize.x = Mathf.Min(_gridWorldSize.x, EditorConstants.WORLD_GRID_X);
        _gridWorldSize.y = Mathf.Min(_gridWorldSize.y, EditorConstants.WORLD_GRID_Y);
        _offsetMax       = Mathf.Min(_offsetMax, EditorConstants.WORLD_OFFSET_MAX);

        float nodeDiameter = _nodeRadius * 2;
        int sizeX = Mathf.RoundToInt(_gridWorldSize.x / nodeDiameter);
        int sizeY = Mathf.RoundToInt(_gridWorldSize.y / nodeDiameter);

        
        if (sizeX <= 0 || sizeY<=0)
        {
            Debug.Log("Grid Reset Fail");
            return;
        }

        if (OffsetIndex >= _offsetMax)
        {
            _currentOffsetValue = _offsetMax * (_nodeRadius * 2f);
        }
        
        Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2f - Direction * _gridWorldSize.y / 2;

        NodeDictionary = new Dictionary<int, Node[,]>();
        for (int i = 0; i < _offsetMax; i++)
        {
            Node[,] grid = new Node[sizeX, sizeY];
            int index = 0;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    float offsetValue = i * nodeDiameter;
                    Vector3 offset    = _isVertical ? Vector3.forward * offsetValue : Vector3.up * offsetValue;
                    Vector3 worldPoint = (worldBottomLeft + Vector3.right * (x * nodeDiameter + _nodeRadius) + Direction * (y * nodeDiameter + _nodeRadius)) + offset;
                    grid[x, y] = new Node(i, index, worldPoint, x, y);
                    index++;
                }
            }
            NodeDictionary.Add(i, grid);
        }
        _gridSizeX = sizeX;
        _gridSizeY = sizeY;

        _isInitGrid = true;
    }
    
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (NodeDictionary == null || !_isInitGrid)
        {
            Debug.Log("Node is not Initialize");
            return null;
        }

        float offsetPos = _isVertical ? worldPosition.z : worldPosition.y;
        float root      = _isVertical ? transform.position.z : transform.position.y;

        int offsetIndex = Mathf.RoundToInt(((offsetPos - root) / (_nodeRadius * 2f)));

        offsetIndex = Mathf.Max(0, offsetIndex);

        Vector3 localPos = worldPosition - transform.position;
        float posY = _isVertical ? localPos.y : localPos.z;

        float percentX = (localPos.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
        float percentY = (posY + _gridWorldSize.y / 2) / _gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

        if (!NodeDictionary.ContainsKey(offsetIndex))
            return null;

        return NodeDictionary[offsetIndex][x, y];
    }
    
    public void GridUpFoward()
    {
        if (OffsetIndex >= _offsetMax - 1)
            return;

        float offset = _currentOffsetValue + (_nodeRadius * 2f);
        _currentOffsetValue = offset;
    }
    public void GridDownBack()
    {
        if (OffsetIndex <= 0)
            return;

        float offset = _currentOffsetValue - (_nodeRadius * 2f);
        _currentOffsetValue = offset;
    }
    
    public void AddSelectNode(Node node)
    {
        if (SelectNodes == null)
            SelectNodes = new List<Node>();

        if (node == null)
            ClearSelectNode();

        if (!SelectNodes.Contains(node))
        {
            SelectNodes.Add(node);
            MapObject mapObject;
            if (MapObjectDictionary.TryGetMapObject(node.OffsetIndex, node.GridIndex, out mapObject))
                mapObject.IsSelection = true;
        }
        else
        {
            SelectNodes.Remove(node);

            MapObject mapObject;
            if (MapObjectDictionary.TryGetMapObject(node.OffsetIndex, node.GridIndex, out mapObject))
                mapObject.IsSelection = false;
        }
    }
    public void RemoveSelectNode(Node node)
    {
        if (SelectNodes == null)
            return;

        if (SelectNodes.Contains(node))
        {
            SelectNodes.Remove(node);
            MapObject mapObject;
            if (MapObjectDictionary.TryGetMapObject(node.OffsetIndex, node.GridIndex, out mapObject))
                mapObject.IsSelection = false;
        }
    }

    public void ClearSelectNode()
    {
        if (SelectNodes == null)
            return;
        
        for (int i = 0; i < SelectNodes.Count; i++)
        {
            MapObject mapObject;
            if (MapObjectDictionary.TryGetMapObject(SelectNodes[i].OffsetIndex, SelectNodes[i].GridIndex, out mapObject))
            {
                //Recovery Object Scale
                mapObject.IsSelection = false;
            }
        }

        SelectNodes.Clear();
    }

    public void CreateCubeGenerator()
    {
        CubeGenerator cubeGen = gameObject.AddComponent<CubeGenerator>();
        cubeGen.Initialized(ResourcePath, _nodeRadius * 2f);
    }

    MapObject CreateMapObject(string path,Node node)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        GameObject go = Instantiate(prefab, node.WorldPosition, prefab.transform.rotation);
        go.transform.SetParent(transform);
        MapObject mapObj = go.AddComponent<MapObject>();
        mapObj.Initialized(path, new Node(node.OffsetIndex, node.GridIndex, node.WorldPosition, node.GridX, node.GridY));

        return mapObj;
    }
    Vector3 GetNearPlanePoint(Bounds bound, Vector3 point)
    {
        Vector3 nearPoint = Vector3.zero;

        List<Vector3> sixPlanePoints = new List<Vector3>();
        sixPlanePoints.Add(bound.center + (Vector3.left * bound.extents.x));
        sixPlanePoints.Add(bound.center + (Vector3.right * bound.extents.x));
        sixPlanePoints.Add(bound.center + (Vector3.up * bound.extents.y));
        sixPlanePoints.Add(bound.center + (Vector3.down * bound.extents.y));
        sixPlanePoints.Add(bound.center + (Vector3.forward * bound.extents.z));
        sixPlanePoints.Add(bound.center + (Vector3.back * bound.extents.z));

        float nearDist = Vector3.Distance(sixPlanePoints[0], point);
        nearPoint = sixPlanePoints[0];
        for (int i = 1; i < sixPlanePoints.Count; i++)
        {
            float dist = Vector3.Distance(sixPlanePoints[i], point);

            if (nearDist > dist)
            {
                nearPoint = sixPlanePoints[i];
                nearDist = dist;
            }
        }
        return nearPoint;
    }


    void DrawSelection()
    {
        float nodeDiameter = _nodeRadius * 2;
        if (SelectNodes != null)
        {
            for (int i = 0; i < SelectNodes.Count; i++)
            {
                Gizmos.color = Color.green - new Color(0, 0, 0, 0.7f);
                Gizmos.DrawCube(SelectNodes[i].WorldPosition, Vector3.one * nodeDiameter);
            }
        }
    }
    void DrawGrid()
    {
        if (NodeDictionary == null)
            return;

        float nodeDiameter = _nodeRadius * 2;
        int sizeX = Mathf.RoundToInt(_gridWorldSize.x / nodeDiameter);
        int sizeY = Mathf.RoundToInt(_gridWorldSize.y / nodeDiameter);

        List<Node> selectList = new List<Node>();
        for (int i = 0; i < _offsetMax; i++)
        {
            int gridIndex = 0;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    MapObject mapObj;
                    if (MapObjectDictionary.TryGetMapObject(i, gridIndex, out mapObj) && !_isOnlyGrid)
                    {
                        Bounds mapObjectBounds = new Bounds(mapObj.Data.Node.WorldPosition, Vector3.one * nodeDiameter);
                        float distance;
                        if (mapObjectBounds.IntersectRay(ray, out distance))
                        {
                            if (i == _offsetMax - 1)
                            {
                                Node node = NodeFromWorldPoint(mapObj.Data.Node.WorldPosition);
                                if (node != null)
                                    selectList.Add(node);
                            }
                            else
                            {
                                Vector3 point = ray.origin + (ray.direction * distance);
                                Vector3 nearPlane = GetNearPlanePoint(mapObjectBounds, point);
                                Vector3 dir = nearPlane - mapObjectBounds.center;
                                Vector3 nearBoundPos = mapObj.Data.Node.WorldPosition + dir.normalized * nodeDiameter;

                                Node nearNode = NodeFromWorldPoint(nearBoundPos);
                                if (nearNode != null)
                                {
                                    selectList.Add(nearNode);
                                    selectList.Add(mapObj.Data.Node);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (OffsetIndex == i)
                        {
                            Vector3 worldPoint = NodeDictionary[i][x, y].WorldPosition;
                            Bounds bounds = new Bounds(worldPoint, Vector3.one * nodeDiameter);
                            float distance;
                            if (bounds.IntersectRay(ray, out distance))
                            {
                                Node node = NodeFromWorldPoint(worldPoint);
                                if (node != null)
                                    selectList.Add(node);
                            }
                        }

                    }

                    if (_isDispalyGrid)
                    {
                        if (OffsetIndex == i)
                        {
                            Vector3 worldPoint = NodeDictionary[i][x, y].WorldPosition;
                            Gizmos.DrawWireCube(worldPoint, Vector3.one * nodeDiameter);
                        }
                    }

                    gridIndex++;
                }
            }
        }

        float selectNearest = 1000f;
        float spawnNearest  = 1000f;

        Node selectNode = null;
        Node spawnNode  = null;

        for (int i = 0; i < selectList.Count; i++)
        {
            float distance = Vector3.Distance(ray.origin, selectList[i].WorldPosition);
            if (selectNearest > distance)
            {
                spawnNode     = selectList[i];
                selectNearest = distance;
            }
            if (spawnNearest > distance)
            {
                if (MapObjectDictionary.ContainMapObject(selectList[i].OffsetIndex, selectList[i].GridIndex) || selectList[i].OffsetIndex == OffsetIndex)
                {
                    selectNode   = selectList[i];
                    spawnNearest = distance;
                }
            }
        }

        SpawnNode  = spawnNode;
        SelectNode = selectNode;

        if (SpawnNode != null)
        {
            if (SpawnNode.OffsetIndex == OffsetIndex)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(SpawnNode.WorldPosition, Vector3.one * nodeDiameter);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(SpawnNode.WorldPosition, Vector3.one * nodeDiameter);
            }
        }
    }


    void OnDrawGizmos()
    {
        if (!_isEditMode)
            return;

        if (NodeDictionary == null ||
            !NodeDictionary.ContainsKey(OffsetIndex)||
            _orginPosition != transform.position ||
             _prevWorldSize != _gridWorldSize ||
             _prevNodeRadius != _nodeRadius ||
              _prevOffsetMax != _offsetMax)
        {
            ResetGrid();
        }
        
        DrawGrid();
        DrawSelection();
        
        _orginPosition   = transform.position;
        _prevWorldSize   = _gridWorldSize;
        _prevNodeRadius  = _nodeRadius;
        _prevOffsetValue = _currentOffsetValue;
        _prevOffsetMax   = _offsetMax;
    }
}
