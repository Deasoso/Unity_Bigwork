using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using MapDataList = System.Collections.Generic.List<MapData>;
using JsonMapData = System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>;

public class MapEditor : EditorWindow
{
    MapGenerator _mapGenerator;

    Dictionary<string,GameObject> prefabs;
    GameObject selectedPrefab;
    string selectedPrefabPath;
    int    selectedPrefabIndex  = -1;
    bool   isAssetLoaded        = false;
    
    bool _isDispalyGrid    = true;
    bool _isVertical       = true;
    bool _isPrevVertical   = true;
    int _offsetMax         = 50;
    Vector2 _gridWorldSize = new Vector2(10f, 10f);
    float _nodeRadius      = 0.5f;

    MapDataList _mapDataList;

    public void Initialize(MapGenerator targetObject)
    {
        Debug.Log("init");
        _mapGenerator = targetObject;
        
        _isDispalyGrid = _mapGenerator.IsDispalyGrid;
        _isVertical    = _mapGenerator.IsVertical;
        _offsetMax     = _mapGenerator.OffsetMax;
        _gridWorldSize = _mapGenerator.GridWorldSize;
        _nodeRadius    = _mapGenerator.NodeRadius;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (_mapGenerator == null)
        {
            Close();
            return;
        }
        if (!_mapGenerator.IsEditMode)
            return;
        
        Handles.BeginGUI();

        EditModeGUI();

        _mapGenerator.IsOnlyGrid = Event.current.shift;

        Ray ray = Camera.current.ScreenPointToRay(new Vector2(Event.current.mousePosition.x, Camera.current.pixelHeight - Event.current.mousePosition.y));
        _mapGenerator.ray = ray;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            _mapGenerator.ClearSelectNode();

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (!Event.current.control)
                _mapGenerator.ClearSelectNode();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.E)// Deaso
        {
            Debug.Log("drawing..");
            if (!SelectionSpawn())
                Spawn(_mapGenerator.SpawnNode);
            Event.current.Use();
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
        {
            if (!SelectionRemove())
                Remove(_mapGenerator.SelectNode);
            Event.current.Use();
        }

        //Ctrl + Left Mouse Click
        if (Event.current.control && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            AddSelection(_mapGenerator.SelectNode);
            Event.current.Use();
        }

        if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.UpArrow)
        {
            GridUpFoward();
        }

        if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.DownArrow)
        {
            GridDownBack();
        }

        Selection.activeObject = null;

        Handles.EndGUI();

        SceneView.RepaintAll();
    }
    void EditModeGUI()
    {
        Handles.BeginGUI();
        GUI.Label(new Rect(10, 10, 1000, 30), "Keys: Spawn(E) Remove(R) MultiSelect(Ctrl + Click) SelectOnlyGrid(Shift) GridUp(Ctrl + ↑) GridDown(Ctrl + ↓)");
        if (selectedPrefab == null)
            GUI.Label(new Rect(10, 30, 300, 30), "No prefab selected!");
        else
            GUI.Label(new Rect(10, 30, 300, 30), selectedPrefab.name + " prefab selected!");

        if (_mapGenerator.SelectNode != null)
        {
            if (_mapGenerator.SelectNode.GridIndex >= 0)
            {
                string nodeText = string.Format("Node Info\nOffsetIndex:{0}\nGridIndex:{1}\nPosition{2}\nGrid{3},{4}",
                    _mapGenerator.SelectNode.OffsetIndex,
                    _mapGenerator.SelectNode.GridIndex,
                    _mapGenerator.SelectNode.WorldPosition,
                    _mapGenerator.SelectNode.GridX,
                    _mapGenerator.SelectNode.GridY);

                GUI.Label(new Rect(10, 50, 300, 300), nodeText);
            }
        }
        Handles.EndGUI();
    }
    void OnGUI()
    {
        if (_mapGenerator == null)
        {
            Close();
            return;
        }

        SettingValue();

        EditorGUILayout.Space();

        ClearMapDialog();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        LoadAssets();
        CreateCubeGenerator();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawResourceButton();
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        LoadButton();
        SaveButton();
        EditorGUILayout.EndHorizontal();
        
        CheckDirectionWarning();
    }
    bool SetResourceFolder(string path)
    {
        try
        {
            string[] files = Directory.GetFiles(path);

            if (prefabs == null)
                prefabs = new Dictionary<string, GameObject>();

            prefabs.Clear();

            if (files.Length != 0)
            {
                foreach (string file in files)
                {
                    if (file.EndsWith(".prefab"))
                    {
                        int startIndex = file.IndexOf("Assets");
                        string resPath = file.Substring(startIndex);
                        GameObject go  = AssetDatabase.LoadAssetAtPath<GameObject>(resPath);

                        Dictionary<string, string> replaceTextDic = new Dictionary<string, string>();
                        replaceTextDic.Add("\\", "/");
                        replaceTextDic.Add(".prefab", "");

                        string resString = "Resources/";
                        int endIndex = resPath.IndexOf(resString) + resString.Length;
                        resPath      = resPath.Remove(0, endIndex);

                        foreach (KeyValuePair<string, string> text in replaceTextDic)
                            resPath = resPath.Replace(text.Key, text.Value);
                        
                        prefabs.Add(resPath, go);
                    }
                }
                if(prefabs.Count <= 0)
                    return false;

                return true;
            }
        }
        catch (DirectoryNotFoundException e)
        {
            if(_mapGenerator != null)
                _mapGenerator.ResourcePath = "";

            Debug.Log(e);
            return false;
        }
        catch (Exception e)
        {
            if (_mapGenerator != null)
                _mapGenerator.ResourcePath = "";

            Debug.Log(e);
            return false;
        }
        return false;
    }
    void SettingValue()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        
        _isDispalyGrid = EditorGUILayout.Toggle("IsDisplayGrid",_isDispalyGrid);
        _isVertical    = EditorGUILayout.Toggle("IsVertical", _isVertical);
        _offsetMax     = EditorGUILayout.IntField("OffsetMax", _offsetMax);
        _gridWorldSize = EditorGUILayout.Vector2Field("GridWorldSize", _gridWorldSize);
        _nodeRadius    = EditorGUILayout.FloatField("Node Radius", _nodeRadius);

        _mapGenerator.IsDispalyGrid = _isDispalyGrid;
        _mapGenerator.IsVertical    = _isVertical;
        _mapGenerator.OffsetMax     = _offsetMax;
        _mapGenerator.GridWorldSize = _gridWorldSize;
        _mapGenerator.NodeRadius    = _nodeRadius;
        
    }
    void LoadAssets()
    {
        try
        {
            if (GUILayout.Button(new GUIContent("LoadAsset", "Load Map Resource"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
            {
                string path = EditorUtility.OpenFolderPanel("Load Map Resource", Application.dataPath, "");
                _mapGenerator.ResourcePath = path;
                if (path != string.Empty)
                    isAssetLoaded = SetResourceFolder(path);
            }
            if (_mapGenerator.ResourcePath != string.Empty)
                isAssetLoaded = SetResourceFolder(_mapGenerator.ResourcePath);
            else
            {
                if (prefabs == null)
                    isAssetLoaded = false;

                if (prefabs != null)
                {
                    if (prefabs.Count <= 0)
                        isAssetLoaded = false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogAssertion(e);
        }
    }
    void LoadButton()
    {
        if (_mapGenerator.ResourcePath != string.Empty)
        {
            SetResourceFolder(_mapGenerator.ResourcePath);
        }
        try
        {
            if (GUILayout.Button(new GUIContent("LoadData", "Load Map Data"), GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
            {
                string path = EditorUtility.OpenFilePanel("Load MapData", "Assets/", "dat");
                _mapGenerator.FilePath = path;
                
                if (path != string.Empty)
                {
                    if (EditorUtility.DisplayDialog("Warning", "Previous work will be deleted. Do you mind?", "Yes", "No"))
                    {
                        ClearMap();
                        LoadFile(path);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogAssertion(e);
        }

    }
    void SaveButton()
    {
        if (GUILayout.Button(new GUIContent("SaveData", "Save MapData file"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
        {
            string path = EditorUtility.SaveFilePanel("Save MapData", "Assets/","","dat");
            if (path == string.Empty)
            {
                Debug.Log("Incorrect File Path or Name");
            }
            else
            {
                SaveFile(path);
            }
        }
    }
    void ClearMapDialog()
    {
        if (GUILayout.Button(new GUIContent("Clear", "MapObject All Clear"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
        {
            Debug.Log("gg");
            if (EditorUtility.DisplayDialog("Map Clear", "Remove All Map Object!!!", "Yes", "No"))
            {
                Component[] components = _mapGenerator.GetComponentsInChildren<Component>();
                foreach (Component component in components)
                {
                    if (component != null)
                    {
                        MapGenerator mg = component.GetComponent<MapGenerator>();
                        if (mg == null)
                            DestroyImmediate(component.gameObject);
                    }
                }
                if(_mapGenerator.MapObjectDictionary !=null)
                    _mapGenerator.MapObjectDictionary.Clear();
            }
        }
    }
    void ClearMap()
    {
        if (_mapGenerator != null)
        {
            Component[] components = _mapGenerator.GetComponentsInChildren<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                {
                    MapGenerator mg = component.GetComponent<MapGenerator>();
                    if (mg == null)
                        DestroyImmediate(component.gameObject);
                }
            }

            if(_mapGenerator.MapObjectDictionary !=null)
                _mapGenerator.MapObjectDictionary.Clear();
        }
    }

    void CreateCubeGenerator()
    {
        if (_mapGenerator != null)
        {
            if (GUILayout.Button(new GUIContent("CubeGenerator", "Cube Generator"), GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
            {
                CubeGenerator[] cubeGenerators = _mapGenerator.GetComponents<CubeGenerator>();
                if (cubeGenerators != null)
                {
                    for (int i = 0; i < cubeGenerators.Length; i++)
                    {
                        DestroyImmediate(cubeGenerators[i]);
                    }
                }
                _mapGenerator.CreateCubeGenerator();
                GUIUtility.ExitGUI();
            }
        }
    }
    
    void CheckDirectionWarning()
    {
        if (Application.isPlaying)
            return;

        if (_isVertical != _isPrevVertical)
        {
            EditorUtility.DisplayDialog("Warning", "Please, Edit by only one direction", "OK");

            _mapGenerator.ResetGrid();
            _isPrevVertical = _isVertical;
        }
    }
    void DrawResourceButton()
    {
        if (!isAssetLoaded)
        {
            GUILayout.Label("There is No Prefabs, Please Check Load Asset Path");
            return;
        }

        if (prefabs != null)
        {
            int index = 0;
            int elementsInThisRow = 0;
            
            foreach (KeyValuePair<string,GameObject> prefab in prefabs)
            {
                elementsInThisRow++;
                Texture2D prefabTexture = AssetPreview.GetAssetPreview(prefab.Value);
                GUIContent content      = new GUIContent(prefabTexture, prefab.Value.name);

                if (prefabTexture ==null)
                    content = new GUIContent(prefab.Value.name, prefab.Value.name);
                
                GUI.enabled = (selectedPrefabIndex != index);
                if (GUILayout.Button(content, GUILayout.MaxWidth(50), GUILayout.MaxHeight(50)))
                {
                    selectedPrefab = prefab.Value;
                    EditorWindow.FocusWindowIfItsOpen<SceneView>();
                    selectedPrefabPath  = prefab.Key;
                    selectedPrefabIndex = index;
                }
 
                if (elementsInThisRow > Screen.width / 70)
                {
                    elementsInThisRow = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                index++;
            }
        }
        GUI.enabled = true;
    }
    void Spawn(Node node)
    {
        if (selectedPrefab == null )
        {
            Debug.LogError("No prefab selected!");
            return;
        }
        if (node != null)
        {
            GameObject go = Instantiate(selectedPrefab, node.WorldPosition, selectedPrefab.transform.rotation);
            go.name = selectedPrefab.name;
            go.transform.SetParent(_mapGenerator.transform);
            MapObject mapObj = go.AddComponent<MapObject>();
            mapObj.Initialized(selectedPrefabPath, new Node(node.OffsetIndex, node.GridIndex, node.WorldPosition, node.GridX, node.GridY));

            if (!_mapGenerator.MapObjectDictionary.ContainMapObject(mapObj.Data.Node.OffsetIndex, mapObj.Data.Node.GridIndex))
            {
                _mapGenerator.MapObjectDictionary.AddMapObject(mapObj.Data.Node.OffsetIndex, mapObj.Data.Node.GridIndex, mapObj);
            }
            else
            {
                Remove(mapObj.Data.Node);

                _mapGenerator.MapObjectDictionary.AddMapObject(mapObj.Data.Node.OffsetIndex, mapObj.Data.Node.GridIndex, mapObj);
            }
        }
    }
    void Remove(Node node)
    {
        if (node != null)
        {
            List<MapObject> removeList;
            _mapGenerator.MapObjectDictionary.RemoveMapObject(node.OffsetIndex, node.GridIndex,out removeList);
            
            if (removeList.Count >0)
            {
                for(int i=0;i< removeList.Count;i++)
                {
                    if(removeList[i] !=null)
                        DestroyImmediate(removeList[i].gameObject);
                }
            }
        }
    }
    void AddSelection(Node node)
    {
        if (node != null)
            _mapGenerator.AddSelectNode(node);
        else
            _mapGenerator.ClearSelectNode();
    }
    bool SelectionSpawn()
    {
        if (_mapGenerator.SelectNodes != null)
        {
            if (_mapGenerator.SelectNodes.Count > 0)
            {
                for (int i = 0; i < _mapGenerator.SelectNodes.Count; i++)
                {
                    Spawn(_mapGenerator.SelectNodes[i]);
                }
                _mapGenerator.ClearSelectNode();
                return true;
            }

        }
        return false;
    }
    bool SelectionRemove()
    {
        if (_mapGenerator.SelectNodes != null)
        {
            if (_mapGenerator.SelectNodes.Count > 0)
            {
                for (int i = 0; i < _mapGenerator.SelectNodes.Count; i++)
                {
                    Remove(_mapGenerator.SelectNodes[i]);
                }
                _mapGenerator.ClearSelectNode();
                return true;
            }

        }
        return false;
    }

    public bool LoadFile(string filepath)
    {
        
        bool isDone = false;
        if (MapLoader.Load(filepath, _mapGenerator.transform))
        {
            _isVertical         = MapLoader.IsVertical;
            _gridWorldSize.x    = MapLoader.GridWorldSize.x;
            _gridWorldSize.y    = MapLoader.GridWorldSize.y;
            _nodeRadius         = MapLoader.NodeRadius;
            
            _mapGenerator.IsVertical          = _isVertical;
            _mapGenerator.MapObjectDictionary = MapLoader.MapObjectDictionary;

            isDone = true;
        }
        return isDone;
    }
    public bool SaveFile(string filepath)
    {
        try
        {
            var streamWriter = new StreamWriter(filepath);
            using (streamWriter)
            {
                var jsonStr = MapDataSerialize();
                streamWriter.WriteLine(jsonStr);
                streamWriter.Close();
                streamWriter.Dispose();

                Debug.LogFormat("Save file : {0}, {1}", filepath, jsonStr);
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
    
    void ConvertMapList()
    {
        _mapDataList = new MapDataList();
        foreach (var mapObj in _mapGenerator.MapObjectDictionary)
        {
            if (mapObj.Value != null)
            {
                var mapObjList = mapObj.Value;
                for (int i = 0; i < mapObjList.Count; i++)
                {
                    mapObjList[i].Data.Node.WorldPosition = mapObjList[i].Data.Node.WorldPosition - _mapGenerator.transform.position;
                    _mapDataList.Add(mapObjList[i].Data);
                }
            }
            else
            {
                Debug.LogFormat("Deleted MapObject(Index : {0}) in Hierachy", mapObj.Key);
            }
        }
    }

    string MapDataSerialize()
    {
        ConvertMapList();
        if (_mapDataList.Count == 0)
        {
            Debug.Log("No Map Data");
            return "";
        }

        var dataDic = new Dictionary<string, object>();

        //Serialize
        dataDic.Add("Version", EditorDefine.GetVersionStr());

        dataDic.Add("Vertical", _isVertical);
        dataDic.Add("WorldSizeX", _gridWorldSize.x);
        dataDic.Add("WorldSizeY", _gridWorldSize.y);
        dataDic.Add("NodeRadius", _nodeRadius);

        JsonMapData mapStringData = new JsonMapData();
        foreach (MapData data in _mapDataList)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();

            MemoryStream memoryStream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, data.Node);
            string str = System.Convert.ToBase64String(memoryStream.ToArray());
            item.Add("ResourcePath", data.ResPath);
            item.Add("Node", str);

            mapStringData.Add(item);
        }

        dataDic.Add("MapData", mapStringData);

        return JsonFx.Json.JsonWriter.Serialize(dataDic);
    }

    public void GridUpFoward()
    {
        _mapGenerator.GridUpFoward();
    }
    public void GridDownBack()
    {
        _mapGenerator.GridDownBack();
    }

    void OnFocus()
    {
        Debug.Log("focusing..");
        // SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        // SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    void OnDestroy()
    {
        Debug.Log("destroying..");
        // SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }
}
