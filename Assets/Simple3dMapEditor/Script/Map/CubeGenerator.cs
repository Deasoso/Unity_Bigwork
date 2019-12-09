using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum CreateError
{
    PrefabName,
    FoloderPath,
    None,
}

public class CubeGenerator : MonoBehaviour
{
    [SerializeField]
    string _name = "";

    [SerializeField]
    float _size = 1f;

    [SerializeField]
    Color _color = Color.black;

    string _folderPath;  //CreateFolderPath

    public string FolderPath { get { return _folderPath; } set { _folderPath = value; } }
    public void Initialized(string folderPath, float size)
    {
        _folderPath = folderPath;
        _size = size;
        _color = Color.black;
    }

    public void Create(Action<CreateError, string> OnComplete)
    {
        string prefabName = _name.Trim();
        if (prefabName == "")
        {
            OnComplete(CreateError.PrefabName, "");
            return;
        }

        string path = _folderPath.Trim();
        if (path == string.Empty)
        {
            OnComplete(CreateError.FoloderPath, "");
            return;
        }

        GameObject cube   = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Collider collider = cube.GetComponent<Collider>();
        if(collider !=null)
            DestroyImmediate(collider);

        cube.transform.SetParent(null);
        cube.transform.position = Vector3.zero;
        cube.transform.rotation = Quaternion.identity;
        cube.transform.localScale = new Vector3(_size, _size, _size);
        cube.name = prefabName;

        CreateMaterial(cube);
        CreatePrefab(cube);

        DestroyImmediate(cube);
        
        OnComplete(CreateError.None, string.Format("Create Cube({0}/{1}.prefab)", _folderPath, prefabName));
    }
    void CreatePrefab(GameObject createObject)
    {
        try
        {
            int startIndex = _folderPath.IndexOf("Assets");
            string resPath = _folderPath.Substring(startIndex);
            string prefabPath = string.Format("{0}/{1}.prefab", resPath, createObject.name);

            UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, createObject);
            PrefabUtility.ReplacePrefab(createObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    void CreateMaterial(GameObject createObject)
    {
        Shader shader = Shader.Find("Transparent/Diffuse");
        Material material = new Material(shader);
        material.color = _color;

        try
        {
            string matFolder = string.Format("{0}/Materials", _folderPath);
            DirectoryInfo directoryInfo = new DirectoryInfo(matFolder);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            int assstsIndex = matFolder.IndexOf("Assets");
            string assetPath = matFolder.Substring(assstsIndex);
            string matPath = string.Format("{0}/{1}.mat", assetPath, createObject.name);

            AssetDatabase.CreateAsset(material, matPath);
            Material createdMaterial = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
            Renderer rend = createObject.GetComponent<Renderer>();
            if (rend != null)
                rend.material = createdMaterial;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
