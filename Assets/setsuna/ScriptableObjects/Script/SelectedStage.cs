using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SelectedStageData",menuName = "ScriptableObject/SelectedStage")]
public class SelectedStage : ScriptableObject
{
    public GameObject stageObject { get; private set; }
    public StageManager manager { get; private set; }
    public Vector2 startPos { get; private set; }

    [SerializeField]public GameObject stage
    {
        set
        {
            stageObject = value;
            manager = value.GetComponent<StageManager>();
            startPos = manager.primaryPlayerPosition;
        }
    }
}
