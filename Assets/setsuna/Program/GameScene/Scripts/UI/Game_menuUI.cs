using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.IO;
using RegUtility;

public class Game_menuUI : SetsunaSlashScript
{
    [SerializeField] Game_HubScript hub;

    [SerializeField] Transform cursor;
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

        if (obj == null) return;

        if (!cursorSetable.Contains(obj)) return;

        var pos = cursor.position;
        pos.y = obj.transform.position.y;
        cursor.position = pos;
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

        deviceToggles.SetActive(config.controllMode != ConfigDatas.ControllMode.touch);
    }

    [Serializable]
    struct SliderBind {
        public Slider slider;
        public TextMeshProUGUI text;
    }




    public void SetCurrentPlayData()
    {
        currentPlayData.hp = hub.playingStage.savedPlayerHP;

        currentPlayData.mp = hub.playingStage.savedPlayerMP;

        currentPlayData.pos = hub.playingStage.savedPlayerPosition;

        currentPlayData.area = hub.playingStage.saveIndex;

        currentPlayData.easyMode = config.easyMode;

        dataDisplay.text = $"HP：{currentPlayData.hp}　　 MP：{currentPlayData.mp}\r\nセーブポイント：({currentPlayData.pos.x}, {currentPlayData.pos.y})\r\nエリア：{Convert.ToInt32(currentPlayData.area) + 1}";
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
}
