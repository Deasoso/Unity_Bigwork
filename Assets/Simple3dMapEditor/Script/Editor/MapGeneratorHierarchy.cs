using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public class MapGeneratorHierarchy
{    
    static MapGeneratorHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            MapGenerator mapGenerator = obj.GetComponent<MapGenerator>();
            if (mapGenerator !=null)
            {
                GUILayout.BeginArea(new Rect(selectionRect.width - 40f, selectionRect.y, 70f, selectionRect.height + 3f));
               
                GUI.backgroundColor = mapGenerator.IsEditMode ? Color.green : Color.red;
                string editMoveText = mapGenerator.IsEditMode ? "On" : "Off";  
                if (GUILayout.Button(string.Format("Edit {0}", editMoveText)))
                {
                    mapGenerator.IsEditMode = !mapGenerator.IsEditMode;
                }             
                GUILayout.EndArea();
            }

            GUI.backgroundColor = Color.white;
        }
    }
}
#endif