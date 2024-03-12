using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using RegUtility;

public class TitleManager : SetsunaSlashScript
{
    ConfigDatas.ControllMode lastControllMode;


    [SerializeField] GameObject continueWindow;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] TextMeshProUGUI passNotFoundCaution;
    [SerializeField] string passNotFoundMessage;
    int passNotFoundCount = 0;

    [SerializeField] GameObject continueCheckWindow;
    [SerializeField] TextMeshProUGUI saveDataText;
    [SerializeField] Button continueCheckPrimeSelect;

    [SerializeField] string[] selectedSaveData;

    void Start()
    {
        continueWindow.SetActive(false);
        continueCheckWindow.SetActive(false);
    }

    public void CheckSaveData()
    {
        CheckSaveData(passwordField.text);
    }
    public void CheckSaveData(string password)
    {
        var data = config.saveDatas.Find((n) => n[0] == password);
        bool found = (data != null);
        passNotFoundCaution.gameObject.SetActive(!found);
        passNotFoundCaution.text = $"{passNotFoundMessage}({passNotFoundCount})";

        if (!found) passNotFoundCount++;
        else ContinueCheck(data);

        //Debug.Log(password + " | " + found);
    }

    public void ContinueCheck(string[] data)
    {
        selectedSaveData = data;

        continueCheckWindow.SetActive(true);

        string easy = (data[6] == "0") ? "ノーマル" : "イージー";
        saveDataText.text = $"パスワード：{data[0]}\r\nHP：{data[1]}　　 MP：{data[2]}\r\nセーブポイント：({data[3]}, {data[4]})\r\nエリア：{Convert.ToInt32(data[5])+1}　　{easy}";

        continueCheckPrimeSelect.Select();
    }

    public void ContinueDecide()
    {
        config.loadedSaveData.pass = selectedSaveData[0];
        config.loadedSaveData.hp = (float)Convert.ToDouble(selectedSaveData[1]);
        config.loadedSaveData.mp = (float)Convert.ToDouble(selectedSaveData[2]);
        config.loadedSaveData.pos.x = (float)Convert.ToDouble(selectedSaveData[3]);
        config.loadedSaveData.pos.y = (float)Convert.ToDouble(selectedSaveData[4]);
        config.loadedSaveData.area = Convert.ToInt32(selectedSaveData[5]);
        config.loadedSaveData.easyMode = Convert.ToBoolean(Convert.ToInt32(selectedSaveData[6]));
        config.isContinueStart = true;

        SceneManager.LoadScene(gameScene);
    }
}
