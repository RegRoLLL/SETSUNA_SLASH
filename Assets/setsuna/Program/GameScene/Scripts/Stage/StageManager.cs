using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StageManager : SetsunaSlashScript
{
    public Game_HubScript hub;

    public string stageName;

    [Space()]
    [SerializeField] Transform backGroundsContainer;
    [SerializeField] CanvasGroup showGroup, fadeGroup, invisibleGroup;

    [Header("Internal Data")]
    public List<GameObject> stageParts = new List<GameObject>();
    public List<GameObject> stageClones = new List<GameObject>();
    public int currentIndex, saveIndex;
    public BackGroundGroup currentBG;
    public Vector3 primaryPlayerPosition, savedPlayerPosition;
    public float savedPlayerHP, savedPlayerMP;


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

        stageParts.Clear();
        foreach (Transform tra in transform)
        {
            if (tra.GetComponent<StageStat>())
                stageParts.Add(tra.gameObject);
        }

        var bgList = backGroundsContainer.GetComponentsInChildren<BackGroundGroup>().ToList();
        foreach (var bgg in bgList)
        {
            bgg.SetParent(invisibleGroup.transform);
            bgg.SetEnable(false);
        }
        Destroy(backGroundsContainer.gameObject);

        currentBG = null;

        SaveAll();

        if (config.isContinueStart) return;

        if (!config.debugMode) hub.PL_Ctrler.transform.position = primaryPlayerPosition;

        savedPlayerPosition = hub.PL_Ctrler.transform.position;
        savedPlayerHP = hub.PL_Ctrler.GetComponent<PL_Status>().hp;
        savedPlayerMP = hub.PL_Ctrler.GetComponent<PL_Status>().mp;
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
        var part = Instantiate(stageClones[index]);

        part.SetActive(true);
        part.transform.parent = stageParts[index].transform.parent;
        Destroy(stageParts[index]);
        stageParts[index] = part;
    }


    public void SetBackGround(StageStat next)
    {
        if ((currentBG != null) && (currentBG == next.backGroundGroup)) return;

        Camera.main.GetComponent<Camera>().backgroundColor = next.backGroundColor;

        if (currentBG != null) { 
            //Debug.Log($"disable:{currentBG}");
            currentBG.SetParent(invisibleGroup.transform);
            currentBG.SetEnable(false);
        }

        //Debug.Log($"enable:{next.backGroundGroup}");
        next.backGroundGroup.SetParent(showGroup.transform);
        next.backGroundGroup.SetEnable(true);

        currentBG = next.backGroundGroup;
    }
}
