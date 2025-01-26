using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.IO;
using RegUtility;
using Cysharp.Threading.Tasks.Triggers;
using System.Linq;
using RegUtil;

public class Game_menuUI : SetsunaSlashScript
{
    Game_SetsunaUI ssUI;
    Game_HubScript Hub { get => ssUI.gm.hub; }

    [SerializeField] Transform cursor;
    [SerializeField] Selectable onDeSelectPointer;
    [SerializeField] List<GameObject> cursorSetable = new();

    [SerializeField] GameObject settingsPanel;
    [SerializeField]  List<SliderBind> sliders = new();
    bool sliderHasBeenSet = false;

    [SerializeField] GameObject deviceToggles;
    [SerializeField] List<Toggle> toggles = new();



    [SerializeField] GameObject savePanel, saveSucceedPanel;
    [SerializeField] TextMeshProUGUI dataDisplay, passFoundCaution;
    [SerializeField] string passFoundMessage;
    int passFoundCount = 0;
    [SerializeField] ConfigDatas.SaveData currentPlayData;



    void Start()
    {
        ssUI = GetComponentInParent<Game_SetsunaUI>();

        SetContentsOnSettings();

        settingsPanel.SetActive(false);
        savePanel.SetActive(false);
        saveSucceedPanel.SetActive(false);
    }

    void OnEnable()
    {
        SetContentsOnSettings();
    }

    void Update()
    {
        var obj = EventSystem.current.currentSelectedGameObject;

        if (obj == null) EventSystem.current.SetSelectedGameObject(onDeSelectPointer.gameObject);

        if (this.gameObject.activeInHierarchy) RegTimeKeeper.Pause();

        if (!cursorSetable.Contains(obj)) return;

        var pos = cursor.position;
        pos.y = obj.transform.position.y;
        cursor.position = pos;
    }

    public void SetDeselectPointer(Selectable target)
    {
        onDeSelectPointer = target;
    }


    [ExecuteAlways]
    public void OnSliderValidate(int sliderNum)
    {
        if (!sliderHasBeenSet) return;

        var bind = sliders[sliderNum];
        bind.text.text = (GetSliderValue01(bind.slider) * 100).ToString() + "%";

        config.masterVolume = GetSliderValue01(sliders[0].slider);
        config.bgmVolume = GetSliderValue01(sliders[1].slider);
        config.seVolume = GetSliderValue01(sliders[2].slider);
        config.SetVolumes();
    }

    float GetSliderValue01(Slider slider)
    {
        return slider.value / slider.maxValue;
    }

    void SetSliderValueFrom01(Slider slider, float value)
    {
        slider.value = value * slider.maxValue;
    }

    public void OnDeviceToggleChanged(int num)
    {
        config.controllMode = (ConfigDatas.ControllMode)num;
        SetContentsOnSettings();
    }

    void SetContentsOnSettings()
    {
        SetSliderValueFrom01(sliders[0].slider, config.masterVolume);
        SetSliderValueFrom01(sliders[1].slider, config.bgmVolume);
        SetSliderValueFrom01(sliders[2].slider, config.seVolume);

        sliderHasBeenSet = true;

        OnSliderValidate(0);
        OnSliderValidate(1);
        OnSliderValidate(2);

        toggles[0].isOn = (config.controllMode == ConfigDatas.ControllMode.keyboard_mouse);
        toggles[1].isOn = (config.controllMode == ConfigDatas.ControllMode.gamepad);

        //一時的にマウキーのみに変更
        //deviceToggles.SetActive(config.controllMode != ConfigDatas.ControllMode.touch);
    }

    [Serializable]
    struct SliderBind {
        public Slider slider;
        public TextMeshProUGUI text;
    }




    public void SetCurrentPlayData()
    {
        var parts = Hub.playingStage.stageParts.Select((p)=>p.GetComponent<StagePart>()).ToList();
        var currentPartSave = parts.FindIndex(p => (p == Hub.playingStage.latestSavePoint.GetPart()));
        currentPlayData.currentPart
            = (currentPartSave == -1) ? 1 : currentPartSave+1;

        var jewelBit = ssUI.SlashCountUI.jewelCounter.GetJewelsCollecting();
        currentPlayData.maxJewel = jewelBit.Count() - 1;
        currentPlayData.collectedJewel = jewelBit.Where(c => char.Equals(c,'1')).Count() - 1;
        currentPlayData.jewelsBit = jewelBit;

        var mainParts = parts.Where((p)=>!p.isAnotherRoom).ToList();
        currentPlayData.partScores.Clear();
        foreach (var stat in mainParts.Select((p) => p.clearStat))
        {
            currentPlayData.partScores.Add((stat.recommendMaxPoint, stat.currentPoint));
        }

        var text = "";
        text += $"宝石：{currentPlayData.collectedJewel}/{currentPlayData.maxJewel} | {currentPlayData.jewelsBit}\r\n";
        text += $"最新エリア：{Convert.ToInt32(currentPlayData.currentPart) + 1}\r\n";
        text += $"スコア：\r\n";

        int part = 1;
        foreach (var (maxScore, score) in currentPlayData.partScores)
        {
            text += $"part{part++} {score}/{maxScore}  ";
        }

        dataDisplay.text = text ;
    }

    public void CheckPassword(TMP_InputField field)
    {
        CheckPassword(field.text);
    }
    public void CheckPassword(string password)
    {
        var data = config.saveDatas.Find((n) => n[0] == password);

        bool found = (data != null) || (password == "");
        passFoundCaution.gameObject.SetActive(found);
        passFoundCaution.text = $"{passFoundMessage}({passFoundCount})";

        if (found) passFoundCount++;
        else Save(password);
    }

    void Save(string pass)
    {
        currentPlayData.pass = pass;

        config.Save(currentPlayData);

        saveSucceedPanel.SetActive(true);
    }


    public void ClosePauseMenu()
    {
        ssUI.TogglePauseMenu(false);
    }

    public void ResetToSavePoint()
    {
        ssUI.gm.LoadSave();
        ClosePauseMenu();
    }

    public void BackTitle()
    {
        ssUI.gm.Back2Title();
    }
}
