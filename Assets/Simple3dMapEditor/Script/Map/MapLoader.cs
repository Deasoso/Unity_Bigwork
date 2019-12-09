using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using MapObjectList = System.Collections.Generic.List<MapObject>;
using MapObjectDic  = System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<MapObject>>;

public static class MapLoader
{
    static Transform _root;

    static bool _isVertical       = false;
    static Vector2 _gridWorldSize = new Vector2(2f, 2f);
    static float _nodeRadius      = 0.5f;

    public static bool IsVertical { get { return _isVertical; } }
    public static Vector2 GridWorldSize{ get { return _gridWorldSize; } }
    public static float NodeRadius { get { return _nodeRadius; } }

    public static MapObjectDic MapObjectDictionary { set; get; }
    
    public static bool Load(string filepath, Transform root)
    {
        MapObjectDictionary = new MapObjectDic();
        MapObjectDictionary.Clear();

        _root = root;
        if(!File.Exists(filepath))
        {
            Debug.LogFormat("Can't found Data File\n {0}", filepath);
            return false;
        }

        try
        {
            var streamReader = new StreamReader(filepath, Encoding.Default);
            using (streamReader)
            {
                var str = streamReader.ReadToEnd();
                MapDataDeserialize(str);
                streamReader.Close();
                streamReader.Dispose();
                
                Debug.LogFormat("Load file : {0}, {1}", filepath, str);
                return true;
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
        finally
        {

        }
    }

    static  void MapDataDeserialize(string json)
    {
        //Deserialize
        var jsonObject = JsonFx.Json.JsonReader.Deserialize(json);
        var jsonDic = jsonObject as Dictionary<string, object>;

        string version;
        if (!jsonDic.ContainsKey("Version"))
        {
            version = "No Version";
            MapDataDeserialize_Old(json);
            Debug.Log("The Mapdata is Old Version");
            return;
        }
        else
        {
            version = jsonDic["Version"].ToString();
        }

        _isVertical      = (bool)jsonDic["Vertical"];
        _gridWorldSize.x = float.Parse(jsonDic["WorldSizeX"].ToString());
        _gridWorldSize.y = float.Parse(jsonDic["WorldSizeY"].ToString());
        _nodeRadius      = float.Parse(jsonDic["NodeRadius"].ToString());
        
        var mapData = jsonDic["MapData"] as object[];
        for (int i = 0; i < mapData.Length; i++)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)mapData[i];

            string path    = data["ResourcePath"].ToString();
            string strNode = data["Node"].ToString();

            MemoryStream memoryStream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            byte[] buffer = System.Convert.FromBase64String(strNode);
            memoryStream.Write(buffer, 0, buffer.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            Node node = formatter.Deserialize(memoryStream) as Node;

            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogFormat("Can't Found Prefab : {0}", path);
                continue;
            }
            Vector3 worldPosition = node.WorldPosition;
            GameObject go = MonoBehaviour.Instantiate(prefab);

            if (_root != null)
            {
                worldPosition = _root.transform.position + node.WorldPosition;
                go.transform.SetParent(_root);
            }

            node.WorldPosition    = worldPosition;
            go.transform.position = worldPosition;
            go.transform.rotation = prefab.transform.rotation;

            MapObject mapObj = go.GetComponent<MapObject>();
            if (mapObj == null)
                mapObj = go.AddComponent<MapObject>();
            
            mapObj.Initialized(path, new Node(node.OffsetIndex, node.GridIndex, node.WorldPosition, node.GridX, node.GridY));
            AddMapObject(node.OffsetIndex, node.GridIndex, mapObj);
        }
    }

    static void MapDataDeserialize_Old(string json)
    {
        //Deserialize
        var jsonObject = JsonFx.Json.JsonReader.Deserialize(json);
        var jsonDic = jsonObject as Dictionary<string, object>;

        _isVertical = (bool)jsonDic["Vertical"];
        _gridWorldSize.x = float.Parse(jsonDic["WorldSizeX"].ToString());
        _gridWorldSize.y = float.Parse(jsonDic["WorldSizeY"].ToString());
        _nodeRadius = float.Parse(jsonDic["NodeRadius"].ToString());

        var mapData = jsonDic["MapData"] as object[];
        for (int i = 0; i < mapData.Length; i++)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)mapData[i];

            string path = data["ResourcePath"].ToString();
            string strNode = data["Node"].ToString();

            MemoryStream memoryStream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            byte[] buffer = System.Convert.FromBase64String(strNode);
            memoryStream.Write(buffer, 0, buffer.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            Node node = formatter.Deserialize(memoryStream) as Node;

            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogFormat("Can't Found Prefab : {0}", path);
                continue;
            }

            Vector3 worldPosition = node.WorldPosition;
            GameObject go = MonoBehaviour.Instantiate(prefab, node.WorldPosition, prefab.transform.rotation);

            if (_root != null)
            {
                worldPosition = _root.transform.position + node.WorldPosition;
                go.transform.SetParent(_root);
            }
            node.WorldPosition = worldPosition;
            go.transform.position = worldPosition;
            go.transform.rotation = prefab.transform.rotation;

            MapObject mapObj = go.GetComponent<MapObject>();
            if (mapObj == null)
                mapObj = go.AddComponent<MapObject>();

            mapObj.Initialized(path, new Node(0, node.Index, node.WorldPosition, node.GridX, node.GridY));
            AddMapObject(0, node.Index, mapObj);
        }
    }

    static void AddMapObject(int offsetIndex, int gridIndex, MapObject mapObj)
    {
        MapObjectList gridList;
        if (MapObjectDictionary.TryGetValue(offsetIndex, out gridList))
        {
            if (gridList != null)
            {
                bool isFind = false;
                for (int i = 0; i < gridList.Count; i++)
                {
                    if (gridList[i].Data.Node.GridIndex == gridIndex)
                    {
                        isFind = true;
                        gridList[i] = mapObj;
                        break;
                    }
                }
                if (!isFind)
                    gridList.Add(mapObj);
            }
        }
        else
        {
            gridList = new MapObjectList();
            gridList.Add(mapObj);
            MapObjectDictionary.Add(offsetIndex, gridList);
        }
    }
}
