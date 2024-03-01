using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetsunaSlashScript : MonoBehaviour
{
    public const int
        playerLayer = 3,
        slashableLayer = 6,
        not_slashableLayer = 7,
        bladeLayer = 8,
        savePointLayer = 9,
        slashGimmickLayer = 10,
        deathFallAreaLayer = 13;

    public ConfigDatas config;
    public AudioBinding audioBind;

    public const string
        titleScene = "TitleScene",
        gameScene = "GameScene",
        levelEditScene = "LevelEditScene";
}
