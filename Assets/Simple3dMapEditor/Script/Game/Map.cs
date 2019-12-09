using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    readonly string STAGE  = "Stage";

    public Text StageUI;
    public Text StarUI;
    public Text GameEnd;
    
    [SerializeField]
    string _mapFolderPath = "Simple3dMapEditor/Resources/Mapdata/Stage";
    [SerializeField]
    int _startStage   = 1;
    int _currentStage = 1;
    int _starMax      = 5;
    void Start ()
    {
        GameOver(false);
        _currentStage = _startStage;
        ChangeStage(_startStage);
    }
    void OnChangeStar(int starCount)
    {
        if (starCount >= _starMax)
        {
            _currentStage++;
            ChangeStage(_currentStage);
        }
        else
        {
            if (StarUI != null)
                StarUI.text = string.Format("Star : {0}", _starMax - starCount);
        }
    }

    void ChangeStage(int stageNumber)
    {
        ClearMap();
        string filePath = string.Format("{0}/{1}/{2}{3}.dat", Application.dataPath, _mapFolderPath, STAGE, stageNumber);
        if (MapLoader.Load(filePath, transform))
        {
            _starMax = 0;
            foreach (KeyValuePair<int, List<MapObject>> mapObjDic in MapLoader.MapObjectDictionary)
            {
                for (int i = 0; i < mapObjDic.Value.Count;i++)
                {
                    MapObject mapObj = mapObjDic.Value[i];
                    if (mapObj.tag == "Star")
                        _starMax++;

                    if (mapObj.tag == "PlayerSpawn")
                    {
                        Vector2 min = Vector2.zero;
                        Vector2 max = Vector2.zero;

                        min.x = transform.position.x - MapLoader.GridWorldSize.x / 2;
                        min.y = transform.position.y - MapLoader.GridWorldSize.y / 2;
                        max.x = transform.position.x + MapLoader.GridWorldSize.x / 2;
                        max.y = transform.position.y + MapLoader.GridWorldSize.y / 2;

                        Player player = FindObjectOfType<Player>();
                        if (player == null)
                        {
                            GameObject prefab = Resources.Load("Game/Player") as GameObject;
                            GameObject playerObj = Instantiate(prefab);
                            player = playerObj.GetComponent<Player>();
                        }
                        player.Initailized(mapObj.transform.position, min, max, OnChangeStar);
                    }
                }
            }

            if (StarUI != null)
                StarUI.text = string.Format("Star : {0}", _starMax);

            if (StageUI != null)
                StageUI.text = string.Format("Stage : {0}", _currentStage);
        }
        else
        {
            GameOver(true);
            Debug.LogFormat("Can't find {0}", filePath);
        }
    }
    void ClearMap()
    {
        Component[] components = GetComponentsInChildren<Component>();
        foreach (Component component in components)
        {
            if (component != null)
            {
                Map map = component.GetComponent<Map>();
                if (map == null)
                    Destroy(component.gameObject);
            }
        }
    }
    void GameOver(bool isOver)
    {
        if (GameEnd != null)
        {
            GameEnd.gameObject.SetActive(isOver);
        }
    }
}
