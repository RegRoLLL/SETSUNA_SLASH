using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class StagePart : MonoBehaviour
{
    [SerializeField] string partTitle;
    public bool isAnotherRoom;
    public Color backGroundColor;

    public AudioClip bgm;
    public bool bgm_playOnEnter;

    public SavePointManager savePoints;
    public PartClearStatus clearStat = new();

    [HideInInspector] public StageRect rect = new();

    public StageRectMarkers rectMarkers;

    StageManager stageManager;

    void OnEnable()
    {
        SetRect();
    }

    [ContextMenu("set rect")]
    public void SetRect()
    {
        rect.up = rectMarkers.up.position.y;
        rect.down = rectMarkers.down.position.y;
        rect.right = rectMarkers.right.position.x;
        rect.left = rectMarkers.left.position.x;
    }
    [Serializable]
    public struct StageRectMarkers
    {
        public Transform up, down, right, left;
    }
    [Serializable]
    public struct StageRect
    {
        public float up, down, right, left;
    }


    [Serializable]
    public class PartClearStatus
    {
        public int currentPoint, recommendMaxPoint;
    }

    public void SetClearStatus(int currentPoint)
    {
        clearStat.currentPoint = currentPoint;
        clearStat.recommendMaxPoint = savePoints.GetSavePoints()
                                      .Where((point)=>!point.isGoalSave).ToList().Count * 3;
    }

    public void AddPoint(int point)
    {
        clearStat.currentPoint += point;

        if(!stageManager) stageManager = GetComponentInParent<StageManager>();

        stageManager.hub.player.ui.SlashCountUI.ShowPointPopup(clearStat, point);
    }

    public string GetTitle() => partTitle;
}
