using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : SetsunaSlashScript
{
    public Game_HubScript hub;
    [SerializeField] GameObject poseMenu, touchController;
    public float loadTeleportTime, flashTime;
    public Image whiteOut;
    public bool isSaving;

    public float titleOpenTime, titleStayTime, titleCloseTime;
    public Image titlePanel;
    public TextMeshProUGUI titleTMP;

    void Start()
    {
        whiteOut.gameObject.SetActive(false);
        isSaving = false;
        poseMenu.SetActive(false);

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
        switch (config.controllMode)
        {
            case ConfigDatas.ControllMode.keyboard_mouse:
                touchController.SetActive(false);
                hub.player.input.SwitchCurrentControlScheme("KeyBoard & Mouse", Keyboard.current, Mouse.current);
                break;

            case ConfigDatas.ControllMode.gamepad:
                touchController.SetActive(false);
                if(Gamepad.current==null) hub.player.input.SwitchCurrentControlScheme("GamePad");
                else hub.player.input.SwitchCurrentControlScheme("GamePad", Gamepad.current);
                break;

            case ConfigDatas.ControllMode.touch:
                touchController.SetActive(true);
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

        titlePanel.gameObject.SetActive(true);


        titleTMP.text = hub.playingStage.stageName;

        float dTime = 0;

        titlePanel.fillOrigin = 0;
        while (dTime <= titleOpenTime)
        {
            float ratio = dTime / titleOpenTime;
            float value = Mathf.Sin(ratio * 90 * Mathf.Deg2Rad);

            titlePanel.fillAmount = value;

            dTime += Time.unscaledDeltaTime;
            yield return null;
        }
        titlePanel.fillAmount = 1;



        yield return new WaitForSecondsRealtime(titleStayTime);



        dTime = 0;
        titlePanel.fillOrigin = 1;
        while (dTime <= titleOpenTime)
        {
            float ratio = dTime / titleOpenTime;
            float value = -Mathf.Pow(ratio,4) + 1;

            titlePanel.fillAmount = value;

            dTime += Time.unscaledDeltaTime;
            yield return null;
        }
        titlePanel.fillAmount = 0;


        titlePanel.gameObject.SetActive(false);


        hub.player.stateP = PlayerController_main.state_pose.stand;
        yield return StartCoroutine(Flash());
    }

    
    public void LoadSave()
    {
        StartCoroutine(ReturnPlayerPos());
    }

    public IEnumerator ReturnPlayerPos()
    {
        isSaving = true;

        hub.player.stateP = PlayerController_main.state_pose.teleport;

        float dTime = 0, ratio;
        Vector3 beforeTeleportPos = hub.player.transform.position;
        while (dTime <= loadTeleportTime)
        {
            ratio = dTime / loadTeleportTime;

            hub.player.transform.position = Vector3.Lerp(beforeTeleportPos, hub.playingStage.savedPlayerPosition, ratio);

            dTime += Time.deltaTime;

            yield return null;
        }
        hub.player.transform.position = hub.playingStage.savedPlayerPosition;
        hub.player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        hub.camera_.ResetList();

        hub.player.stateP = PlayerController_main.state_pose.stand;

        hub.playingStage.Load();

        yield return StartCoroutine(Flash());

        isSaving = false;
    }

    IEnumerator Flash()
    {
        whiteOut.gameObject.SetActive(true);

        whiteOut.color = Color.white;
        Color color = whiteOut.color;

        var stat = hub.player.GetComponent<PL_Status>();
        stat.HP_heal(stat.hp_max);
        stat.HP_damage(stat.hp_max - hub.playingStage.savedPlayerHP);
        stat.MP_heal(stat.mp_max);
        stat.MP_damage(stat.mp_max - hub.playingStage.savedPlayerMP);

        float dTime = 0;
        while (dTime <= flashTime)
        {
            color.a = 1 - (dTime / flashTime);

            whiteOut.color = color;

            dTime += Time.unscaledDeltaTime;
            yield return null;
        }

        whiteOut.color = Color.clear;

        whiteOut.gameObject.SetActive(false);
    }

    [ContextMenu("setVolumes")]
    public void SetAudioVolumes()
    {
        gameObject.SendMessage(nameof(AudioVolumeManager.SetVolume), SendMessageOptions.DontRequireReceiver);
    }


    public void PoseMenu()
    {
        poseMenu.SetActive(!poseMenu.activeInHierarchy);
        Time.timeScale = poseMenu.activeInHierarchy ? 0 : 1;
    }

    public void Back2Title()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(titleScene);
    }
}
