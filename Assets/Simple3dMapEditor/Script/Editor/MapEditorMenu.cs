using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class MapEditorMenu
{
    static MapGenerator _mapGenerator;

    [MenuItem("GameObject/MapEditor/MapGenerator",false,10)]    
    static void CreateMapGenerator()
    {
        MapGenerator mapGenerator = GameObject.FindObjectOfType<MapGenerator>();
        if (mapGenerator == null)
        {
            GameObject go = new GameObject("MapGenerator");
            Undo.RegisterCreatedObjectUndo(go, "Created" + go.name);

            go.transform.SetParent(null);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector4.one;

            _mapGenerator = go.AddComponent<MapGenerator>();

            MapEditor editorWindow = EditorWindow.GetWindow<MapEditor>();
            editorWindow.Initialize(_mapGenerator);
        }
        else
        {
            _mapGenerator = mapGenerator;
            MapEditor editorWindow = EditorWindow.GetWindow<MapEditor>();
            editorWindow.Initialize(_mapGenerator);
        }
    }

    public static void ShowMenu()
    {
        MapGenerator mapGenerator = GameObject.FindObjectOfType<MapGenerator>();
        if (mapGenerator != null)
        {
            _mapGenerator = mapGenerator;
            MapEditor editorWindow = EditorWindow.GetWindow<MapEditor>();
            editorWindow.Initialize(_mapGenerator);
        }
    }
}
