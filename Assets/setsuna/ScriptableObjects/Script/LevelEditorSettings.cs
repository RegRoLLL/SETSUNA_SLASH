using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LE_Settings",menuName = "ScriptableObject/LevelEditorSettings")]
public class LevelEditorSettings : ScriptableObject
{
    public string savePath;
}
