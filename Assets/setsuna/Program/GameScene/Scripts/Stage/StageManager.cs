using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : SetsunaSlashScript
{
    public Game_HubScript hub;

    public string stageName;

    public List<GameObject> stageParts = new List<GameObject>();
    public List<GameObject> stageClones = new List<GameObject>();
    public int currentIndex, saveIndex;
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

        SaveAll();

        if (config.isContinueStart) return;

        if (!config.debugMode) hub.player.transform.position = primaryPlayerPosition;

        savedPlayerPosition = hub.player.transform.position;
        savedPlayerHP = hub.player.GetComponent<PL_Status>().hp;
        savedPlayerMP = hub.player.GetComponent<PL_Status>().mp;
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
}
