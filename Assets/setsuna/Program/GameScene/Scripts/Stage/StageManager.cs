using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class StageManager : SetsunaSlashScript
{
    public Game_HubScript hub;

    public string stageName;
    
    [Space()]
    [SerializeField] Transform backGroundsContainer;
    [SerializeField] CanvasGroup showGroup, invisibleGroup;


    [Header("Internal Data")]
    public List<GameObject> stageParts = new();
    public List<GameObject> stageClones = new();
    public List<SerializableList<string>> jewelRoomNames = new();
    public int currentIndex, saveIndex;
    public BackGroundGroup currentBG;
    public Vector3 primaryPlayerPosition, savedPlayerPosition;
    public SavePoint latestSavePoint, anotherPartSave;

    [Serializable]
    public class SerializableList<T> {
        public List<T> list2 = new();
        public SerializableList(List<T> source){
            this.list2 = source;
        }
    }


    void Start()
    {
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            if (root.GetComponent<Game_HubScript>())
            {
                hub = root.GetComponent<Game_HubScript>();
                break;
            }
        }

        InitializeStageManageLists();
        InitializeBackGrounds();

        SaveAll();

        if (config.isContinueStart)
        {
            SetJewelsContinueData();
            return;
        }

        if (!config.debugMode) hub.PL_Ctrler.transform.position = primaryPlayerPosition;

        savedPlayerPosition = hub.PL_Ctrler.transform.position;
    }
    void InitializeStageManageLists()
    {
        stageParts.Clear();
        jewelRoomNames.Clear();
        foreach (Transform tra in transform)
        {
            if (tra.TryGetComponent<StagePart>(out var part))
            {
                part.SetClearStatus(0);
                stageParts.Add(tra.gameObject);

                if (!part.isAnotherRoom)
                {
                    if (part.GetAnotherManager() is var ar_manager and not null)
                    {
                        jewelRoomNames.Add(new(ar_manager.GetNameList()));
                    }
                }
            }
        }
    }
    void InitializeBackGrounds()
    {
        var bgList = backGroundsContainer.GetComponentsInChildren<BackGroundGroup>().ToList();
        foreach (var bgg in bgList)
        {
            bgg.SetParent(invisibleGroup.transform);
            bgg.SetEnable(false);
        }
        Destroy(backGroundsContainer.gameObject);

        currentBG = null;
    }

    void SetJewelsContinueData()
    {
        var jewels = config.loadedSaveData.jewelsBit;
        var jewelsStatList = jewels.ToString().ToList();
        var roomsList = stageParts.Select(p => p.GetComponent<StagePart>())
                                  .Where(p => p.isAnotherRoom)
                                  .ToList();
        foreach (var (stat,index) in jewelsStatList.Select((stat,index)=>(stat,index)))
        {
            if (!stat.Equals('1')) continue;

            roomsList[index].GetComponentInChildren<CollectableJewel>(true).SetCollected();
        }
    }

    public void SetJewelsCountUICollectStat(int roomIndex)
    {
        hub.player.ui.SlashCountUI.jewelCounter.GenerateJewelCells(jewelRoomNames[roomIndex].list2.Count);

        var parts = stageParts.Select(p => p.GetComponent<StagePart>()).ToList();
        
        foreach (var (title, index) in jewelRoomNames[roomIndex].list2.Select((title, index) => (title, index)))
        {
            var part = parts.Find(part => part.GetTitle().Equals(title));

            if (!part.isAnotherRoom) continue;

            if (part.GetComponentInChildren<CollectableJewel>(true) is var jewel and not null)
            {
                if (jewel.IsCollected) CollectJewel(title);
            }
        }
    }


    public void SaveAll()
    {
        if(stageClones.Count > 0)
            foreach (var obj in stageClones)
            {
                Destroy(obj);
            }

        stageClones.Clear();

        foreach (var part in stageParts)
        {
            var p = Instantiate(part);
            p.SetActive(false);
            p.transform.parent = part.transform.parent;
            stageClones.Add(p);
        }
    }

    
    public void SaveOverWrite(GameObject stage)
    {
        SaveOverWrite(stageParts.FindIndex((n) => (n == stage)));
    }
    public void SaveOverWrite(int index)
    {
        var part = Instantiate(stageParts[index]);

        part.SetActive(false);
        part.transform.parent = stageClones[index].transform.parent;
        Destroy(stageClones[index]);
        stageClones[index] = part;
    }

    public void SaveNotOverWrite(GameObject stage)
    {
        SaveNotOverWrite(stageParts.FindIndex((n) => (n == stage)));
    }
    public void SaveNotOverWrite(int index)
    {
        saveIndex = index;
    }


    public void Load()
    {
        Load(saveIndex);
        if(currentIndex < stageParts.Count-1)Load(currentIndex+1);
        if(currentIndex > 0)Load(currentIndex-1);
    }
    void Load(int index)
    {
        var newPart = Instantiate(stageClones[index]);
        var oldPart = stageParts[index];

        newPart.SetActive(true);
        newPart.transform.parent = oldPart.transform.parent;
        var newPartComponent = newPart.GetComponent<StagePart>();
        var oldPartComponent = oldPart.GetComponent<StagePart>();

        var data = oldPartComponent.savePoints.GetSavePointsData();
        newPartComponent.savePoints.SetSavePointsData(data);
        newPartComponent.savePoints.ConnectSavePoints_InRuntime();

        newPartComponent.SetClearStatus(oldPartComponent.clearStat.currentPoint);

        if (newPartComponent.isAnotherRoom)
        {
            var oldJewel = oldPartComponent.GetComponentInChildren<CollectableJewel>(true);

            if (oldJewel.IsCollected)
            {
                newPartComponent.GetComponentInChildren<CollectableJewel>(true).SetCollected();
            }
        }

        var oldSavePoints = oldPartComponent.savePoints.GetSavePoints();
        var newSavePoints = newPartComponent.savePoints.GetSavePoints();
        if (anotherPartSave != null)
        {
            var pointIndex = oldSavePoints.IndexOf(anotherPartSave);
            if (pointIndex != -1) anotherPartSave = newSavePoints[pointIndex];
        }
        else
        {
            var pointIndex = oldSavePoints.IndexOf(latestSavePoint);
            if (pointIndex != -1) latestSavePoint = newSavePoints[pointIndex];
        }

        foreach (var (point,_index) in newSavePoints.Select((point,_index)=>(point,_index)))
        {
            point.SetHints(oldSavePoints[_index].GetHints());
        }

        Destroy(oldPart);
        stageParts[index] = newPart;
    }

    public void SetCurrentPart(int partIndex)
    {
        currentIndex = partIndex;
        var part = stageParts[partIndex].GetComponent<StagePart>();

        if (part.isAnotherRoom)
        {
            var roomIndex = jewelRoomNames.FindIndex(element => element.list2.Contains(part.GetTitle()));
            SetJewelsCountUICollectStat(roomIndex);
        }

        SetBackGround(next: part);
    }


    public void SetBackGround(StagePart next)
    {
        if ((currentBG != null) && (currentBG == next.backGroundGroup)) return;

        Camera.main.GetComponent<Camera>().backgroundColor = next.backGroundColor;

        if (currentBG != null)
        {
            //Debug.Log($"disable:{currentBG}");
            currentBG.SetParent(invisibleGroup.transform);
            currentBG.SetEnable(false);
        }
        currentBG = next.backGroundGroup;

        if (currentBG == null) return;

        //Debug.Log($"enable:{next.backGroundGroup}");
        currentBG.SetParent(showGroup.transform);
        currentBG.SetEnable(true);
    }



    public void CollectJewel(string partTitle)
    {int a, b;
        a = jewelRoomNames.FindIndex(element => element.list2.Contains(partTitle));

        if (a < 0) return;

        b = jewelRoomNames[a].list2.IndexOf(partTitle);

        if (b < 0) return;

        CollectJewel(b);
    }
    void CollectJewel(int index)
    {
        hub.player.CollectJewel(index);
    }

    /// <summary>
    /// ïÛêŒÇÃälìæèÛãµ
    /// </summary>
    /// <returns>01ÇÃÉfÅ[É^óÒÇ≈ï‘Ç∑</returns>
    public string GetJewelsCollectingBits()
    {
        string result = "";
        foreach (var part in stageParts.Select(obj=>obj.GetComponent<StagePart>()))
        {
            if (!part.isAnotherRoom) continue;

            if (part.GetComponentInChildren<CollectableJewel>(true) is var jewel and not null)
            {
                if (jewel.IsCollected) result += "1";
                else result += "0";
            }
        }
        return result;
    }
}