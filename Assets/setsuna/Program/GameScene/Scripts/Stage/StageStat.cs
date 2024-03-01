using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StageStat : MonoBehaviour
{
    public Color backGroundColor;

    public AudioClip bgm;
    public bool bgm_playOnEnter;

    public StageRect rect = new StageRect();

    public StageRectMarkers rectMarkers;

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
}
