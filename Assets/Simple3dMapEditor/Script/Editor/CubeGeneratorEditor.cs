using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeGenerator))]
public class CubeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawLabel();
        EditorGUILayout.BeginHorizontal();
        SetCreateFolder();
        CreateObject();
        EditorGUILayout.EndHorizontal();

        Repaint();
    }
    void DrawLabel()
    {
        CubeGenerator cubeGen = serializedObject.targetObject as CubeGenerator;

        if (cubeGen.FolderPath != null)
        {
            if (cubeGen.FolderPath != string.Empty)
            {
                int startIndex = cubeGen.FolderPath.IndexOf("Assets");
                string savePath = cubeGen.FolderPath.Substring(startIndex);
                GUILayout.Label(savePath);
            }
        }
    }
    void SetCreateFolder()
    {
        CubeGenerator cubeGen = serializedObject.targetObject as CubeGenerator;
        try
        {
            if (GUILayout.Button(new GUIContent("SaveFolder", "Set SaveFolder Path"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
            {
                string path = EditorUtility.OpenFolderPanel("CreateFolder", Application.dataPath, "");
                cubeGen.FolderPath = path;
                Debug.Log(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogAssertion(e);
        }
    }
    void CreateObject()
    {
        if (GUILayout.Button(new GUIContent("Create", "Create Object"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
        {
            CubeGenerator cubeGen = serializedObject.targetObject as CubeGenerator;
            cubeGen.Create((type, message) =>
            {
                if (type == CreateError.PrefabName)
                    EditorUtility.DisplayDialog("Failed", "Please, Insert name", "OK");

                if (type == CreateError.FoloderPath)
                    EditorUtility.DisplayDialog("Failed", "Incorret folderPath, Check Prefab SaveFolder", "OK");

                if (type == CreateError.None)
                    Debug.Log(message);

            });
        }
    }
}
