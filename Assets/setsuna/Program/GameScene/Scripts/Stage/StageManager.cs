using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class StageManager : SetsunaSlashScript
{
    public Game_HubScript hub;

    public string stageName;
    public Color stageColor;

    [Header("Internal Data")]
    public List<GameObject> stageParts = new();
    public List<GameObject> stageClones = new();
    public List<SerializableList<string>> jewelRoomNames = new();
    public int currentIndex, saveIndex;
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

        hub.player.ui.SlashCountUI.jewelCounter.GenerateJewelCells(jewelRoomNames[0].list2.Count);

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

    void SetJewelsContinueData()
    {
        var jewels = config.loadedSaveData.jewelsBit;
        var jewelsList = jewels.ToString().ToList();
        jewelsList.RemoveAt(0);
        jewelsList.Reverse();
        foreach (var (jewel,index) in jewelsList.Select((jewel,index)=>(jewel,index)))
        {
            if (!jewel.Equals('1')) continue;

            CollectJewel(index);
            stageParts.Select(p => p.GetComponent<StagePart>())
                .Where(p => p.isAnotherRoom).ToList()[index]
                .GetComponentInChildren<CollectableJewel>(true).SetCollected();
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


    public void CollectJewel(string partTitle)
    {
        //CollectJewel(jewelRoomNames.FindIndex((title) => title == partTitle));
    }
    void CollectJewel(int index)
    {
        hub.player.CollectJewel(index);
    }
}