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
using Unity.VisualScripting;

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
        if(!ssUI) ssUI = GetComponentInParent<Game_SetsunaUI>();

        var parts = Hub.playingStage.stageParts.Select((p)=>p.GetComponent<StagePart>()).ToList();
        var latestSavePart = parts.FindIndex(p => (p == Hub.playingStage.latestSavePoint.GetPart())) + 1;

        var load = config.loadedSaveData;
        if (config.isContinueStart && latestSavePart < load.latestPart)
        {
            //コンティニュースタート時の初期代入
            currentPlayData.latestPart = load.latestPart;
            currentPlayData.latestPartTitle = load.latestPartTitle;
        }
        else if(currentPlayData.latestPart < latestSavePart)
        {
            //最高到達partの更新
            currentPlayData.latestPart = latestSavePart;
            currentPlayData.latestPartTitle = parts[latestSavePart - 1].GetTitle();
        }

        var jewelBit = Hub.playingStage.GetJewelsCollectingBits();
        currentPlayData.maxJewel = jewelBit.Count();
        currentPlayData.collectedJewel = jewelBit.Where(c => char.Equals(c,'1')).Count();
        currentPlayData.jewelsBit = jewelBit;

        var mainParts = parts.Where((p)=>!p.isAnotherRoom).ToList();
        List<(int maxScore, int score)> copiedPlayData = new(config.isContinueStart ? config.loadedSaveData.partScores : currentPlayData.partScores);
        currentPlayData.partScores.Clear();
        void Add(StagePart.PartClearStatus stat) => currentPlayData.partScores.Add((stat.recommendMaxPoint, stat.currentPoint));
        if (config.isContinueStart)
        {
            //コンティニュー時(比較して更新していれば上書き)
            foreach (var (stat,index) in mainParts.Select((p,i) => (p.clearStat,i)))
            {
                if (copiedPlayData.Count - 1 < index){
                    //アップデートなどでpart数が増えた場合の増分
                    //Debug.Log($"part{index + 1}:上書き(増分)");
                    Add(stat);
                    continue;
                }

                //比較
                var (maxScore,score) = copiedPlayData[index];
                if (maxScore != stat.recommendMaxPoint){
                    //アップデートなどでpart内容が変わった場合、上書き
                    //Debug.Log($"part{index + 1}:上書き(内容変更)");
                    Add(stat);
                }
                else if(score < stat.currentPoint){
                    //通常のスコア更新
                    //Debug.Log($"part{index + 1}:上書き(更新)");
                    Add(stat);
                }
                else{
                    //スコアが最高スコアを下回った場合、現状維持
                    //Debug.Log($"part{index + 1}:現状維持");
                    currentPlayData.partScores.Add((maxScore, score));
                }
            }
        }
        else
        {
            //非コンティニュー時(現状のデータそのまま代入)
            foreach (var stat in mainParts.Select((p) => p.clearStat))
            {
                Add(stat);
            }
        }

        var text = "";
        text += $"宝石：{currentPlayData.collectedJewel}/{currentPlayData.maxJewel}\r\n";
        text += $"最新エリア：{currentPlayData.latestPartTitle}(part{currentPlayData.latestPart})\r\n";
        text += $"スコア：\r\n";

        int part = 1;
        foreach (var (maxScore, score) in currentPlayData.partScores)
        {
            if (part > currentPlayData.latestPart) break;
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
