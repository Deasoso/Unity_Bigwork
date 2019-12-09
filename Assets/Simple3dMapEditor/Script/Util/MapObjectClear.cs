using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectClear : MonoBehaviour
{
    public string Name         =  "NewMap";
    public float originOffsetY = 0.5f;

    [Button]
    void Clear()
    {
        int nCount = 0;
        MapObject[] mapObj = GetComponentsInChildren<MapObject>();
        for (int i = 0; i < mapObj.Length; i++)
        {
            DestroyImmediate(mapObj[i]);
            nCount++;
        }
        Debug.LogFormat("Clear Complete({0} Object)", nCount);
    }

    [Button]
    void ClearAndIndependent()
    {
        Name = Name.Trim();

        if (Name == string.Empty)
            Name = "NewMap";

        GameObject parent = new GameObject(Name);
        parent.transform.SetParent(null);
        parent.transform.position      = Vector3.zero;
        parent.transform.localScale    = Vector3.one;
        parent.transform.localRotation = Quaternion.identity;

        int nCount = 0;
        MapObject[] mapObj = GetComponentsInChildren<MapObject>();

        Vector3 total  = Vector3.zero;
        for (int i = 0; i < mapObj.Length; i++)
        {
            total.x += mapObj[i].transform.position.x;
            total.z += mapObj[i].transform.position.z;

            if (total.y > mapObj[i].transform.position.y)
                total.y = mapObj[i].transform.position.y;
            
            nCount++;
        }
        
        parent.transform.position = new Vector3(total.x / nCount, total.y + originOffsetY, total.z / nCount);
        for (int i = 0; i < mapObj.Length; i++)
        {
            mapObj[i].transform.SetParent(parent.transform);
            DestroyImmediate(mapObj[i]);
        }
        parent.transform.position = Vector3.zero;
        Debug.LogFormat("Clear Complete({0} Object)", nCount);
    }
}
