using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using MapObjectList = System.Collections.Generic.List<MapObject>;
using MapObjectDic  = System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<MapObject>>;
using MapDataList   = System.Collections.Generic.List<MapData>;
using JsonMapData   = System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>;
using NodeDic       = System.Collections.Generic.Dictionary<int, Node[,]>;

public static class Extension
{
    static public bool ContainMapObject(this MapObjectDic MapObjectDictionary, int offsetIndex, int gridIndex)
    {
        MapObjectList gridList;
        if (MapObjectDictionary.TryGetValue(offsetIndex, out gridList))
        {
            if (gridList != null)
            {
                for (int i = 0; i < gridList.Count; i++)
                {
                    if (gridList[i].Data.Node.GridIndex == gridIndex)
                        return true;
                }
            }
        }
        return false;
    }
    static public MapObject GetMapObject(this MapObjectDic MapObjectDictionary, int offsetIndex, int gridIndex)
    {
        MapObject mapObject = new MapObject();

        MapObjectList gridList;
        if (MapObjectDictionary.TryGetValue(offsetIndex, out gridList))
        {
            if (gridList != null)
            {
                for (int i = 0; i < gridList.Count; i++)
                {
                    if (gridList[i].Data.Node.GridIndex == gridIndex)
                    {
                        mapObject = gridList[i];
                    }
                }
            }
        }

        return mapObject;
    }
    static public bool TryGetMapObject(this MapObjectDic MapObjectDictionary, int offsetIndex, int gridIndex, out MapObject mapObject)
    {
        mapObject = null;

        MapObjectList gridList;
        if (MapObjectDictionary.TryGetValue(offsetIndex, out gridList))
        {
            if (gridList != null)
            {
                for (int i = 0; i < gridList.Count; i++)
                {
                    if (gridList[i].Data.Node.GridIndex == gridIndex)
                    {
                        mapObject = gridList[i];
                        return true;
                    }
                }
            }
        }
        return false;
    }
    static public void AddMapObject(this MapObjectDic MapObjectDictionary, int offsetIndex, int gridIndex, MapObject mapObj)
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
    static public bool RemoveMapObject(this MapObjectDic MapObjectDictionary, int offsetIndex, int gridIndex, out MapObjectList removeList)
    {
        removeList = new MapObjectList();

        MapObjectList gridList;
        if (MapObjectDictionary.TryGetValue(offsetIndex, out gridList))
        {
            if (gridList != null)
            {
                if (gridList != null)
                {
                    for (int i = 0; i < gridList.Count; i++)
                    {
                        if (gridList[i].Data.Node.GridIndex == gridIndex)
                        {
                            removeList.Add(gridList[i]);
                            gridList.Remove(gridList[i]);
                            
                            return true;
                        }
                    }
                }

            }
        }
        return false;
    }
}
