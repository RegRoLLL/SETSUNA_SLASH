using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using RegUtil;

public class GameManager : SetsunaSlashScript
{
    [HideInInspector] public Game_HubScript hub;
    public float loadTeleportTime;
    public bool isSaving, isPausing;

    Game_SetsunaUI playerUI;

    void Start()
    {
        hub = GetComponent<Game_HubScript>();
        playerUI = hub.pl.ui;

        isSaving = false;

        if (config.isContinueStart)
        {
            SetContinueData();
        }
        else
        {
            StartCoroutine(TitleCall());
        }
    }

    void Update()
    {
        if (isPausing) RegTimeKeeper.Pause();


        switch (config.controllMode)
        {
            case ConfigDatas.ControllMode.keyboard_mouse:
                playerUI.ToggleTouchController(false);
                hub.player.input.SwitchCurrentControlScheme("KeyBoard & Mouse", Keyboard.current, Mouse.current);
                break;

            case ConfigDatas.ControllMode.gamepad:
                playerUI.ToggleTouchController(false);
                if(Gamepad.current==null) hub.player.input.SwitchCurrentControlScheme("GamePad");
                else hub.player.input.SwitchCurrentControlScheme("GamePad", Gamepad.current);
                break;

            case ConfigDatas.ControllMode.touch:
                playerUI.ToggleTouchController(true);
                if(Gamepad.current==null) hub.player.input.SwitchCurrentControlScheme("TouchPanel");
                else hub.player.input.SwitchCurrentControlScheme("TouchPanel", Gamepad.current);
                break;

            default: break;
        }
    }

    void SetContinueData()
    {
        hub.player.stat.SetHP(config.loadedSaveData.hp);
        hub.playingStage.savedPlayerHP = config.loadedSaveData.hp;

        hub.player.stat.SetMP(config.loadedSaveData.mp);
        hub.playingStage.savedPlayerMP = config.loadedSaveData.mp;

        hub.player.transform.position = config.loadedSaveData.pos;
        hub.playingStage.savedPlayerPosition = config.loadedSaveData.pos;

        hub.playingStage.saveIndex = config.loadedSaveData.area;
        hub.playingStage.currentIndex = config.loadedSaveData.area;

        config.easyMode = config.loadedSaveData.easyMode;

        config.isContinueStart = false;
    }

    IEnumerator TitleCall()
    {
        hub.player.stateP = PlayerController_main.state_pose.teleport;

        yield return StartCoroutine(playerUI.TitleCall(hub.playingStage.stageName));

        hub.player.stateP = PlayerController_main.state_pose.stand;

        yield return StartCoroutine(Flash());
    }

    
    public void LoadSave()
    {
        StartCoroutine(hub.pl.ReturnPlayerPos());
    }


    IEnumerator Flash()
    {
        yield return StartCoroutine(playerUI.Flash(() =>
        {
            var stat = hub.player.GetComponent<PL_Status>();
            stat.HP_heal(stat.hp_max);
            stat.HP_damage(stat.hp_max - hub.playingStage.savedPlayerHP);
            stat.MP_heal(stat.mp_max);
            stat.MP_damage(stat.mp_max - hub.playingStage.savedPlayerMP);
        }));
    }

    [ContextMenu("setVolumes")]
    public void SetAudioVolumes()
    {
        gameObject.SendMessage(nameof(AudioVolumeManager.SetVolume), SendMessageOptions.DontRequireReceiver);
    }


    public void TogglePause()
    {
        playerUI.TogglePauseMenu(!isPausing);
    }

    public void Back2Title()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(titleScene);
    }
}
