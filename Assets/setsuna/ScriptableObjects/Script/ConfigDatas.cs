using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using RegUtility;
using System.Linq;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObject/ConfigDatas")]
public class ConfigDatas : ScriptableObject
{
    [SerializeField] RDataEncrypter encrypter;

    [Range(0f,1f)]public float masterVolume, bgmVolume, seVolume;
    [SerializeField] ControllMode controllMode_;
    public bool slowWhenSlashCharge;
    public bool easyMode, demoMode, debugMode;

    public SaveData loadedSaveData = new();
    public bool isContinueStart;
    public string saveDataFilePath { get { return Application.persistentDataPath + $"/{saveDataCSV_name}.csv"; } }
    public string saveDataCSV_name;
    public List<string[]> saveDatas = new();

    [SerializeField] bool outPutSaveDataLog;

    private void Awake()
    {
        //Debug.Log("config awaken.");

#if !UNITY_EDITOR
        debugMode=false;
#endif

        InputSystem.onDeviceChange += (device, change) => OnDeviceChange(device, change);

        controllMode = (Application.isMobilePlatform) ? ControllMode.touch : ControllMode.keyboard_mouse;
        LoadSaveDatas();
    }

    void OnEnable()
    {
        if (!File.Exists(saveDataFilePath))
        {
            var f = File.CreateText(saveDataFilePath);
            f.Close();
            Debug.Log("file created.");
        }

        SetDevice();
        LoadSaveDatas();
    }

    void OnValidate()
    {
        SetDevice();
        SetVolumes();
    }


    public enum ControllMode { keyboard_mouse, gamepad, touch }

    public ControllMode controllMode
    {
        get { return controllMode_; }

        set
        {
            controllMode_ = value;
            SetDevice();
        }
    }

    void SetDevice()
    {
        Action<Gamepad> setDevice =
            (controllMode_ == ControllMode.gamepad) ? ((pad) => InputSystem.EnableDevice(pad)) : ((pad) => InputSystem.DisableDevice(pad));

        foreach (var pad in Gamepad.all)
        {
            setDevice(pad);
            //Debug.Log(pad.name + ": " + pad.enabled);
        }
    }

    public void SetDifficulty(bool easy)
    {
        easyMode = easy;
    }

    
    [ContextMenu("setVolumes")]
    public void SetVolumes()
    {
        if (Camera.main == null)
        {
            Debug.Log("SetVolumes failed. Camera.main was null.");
            return;
        }

        SetVolumes(Camera.main.gameObject);
    }
    public void SetVolumes(GameObject obj)
    {
        if (obj == null)
        {
            Debug.Log("SetVolumes failed. object was null.");
            return;
        }

        foreach (var root in obj.scene.GetRootGameObjects())
        {
            foreach (var manager in root.GetComponentsInChildren<AudioVolumeManager>(true))
            {
                manager.SetVolume();
            }
        }
    }
    


    [Serializable]
    public class SaveData
    {
        public string pass;
        public int startPart;
        public int latestPart;
        public string latestPartTitle;
        public int maxJewel, collectedJewel;
        public string jewelsBit;
        public List<(int maxScore, int score)> partScores = new();
    }

    [ContextMenu("loadDatas")]
    void LoadSaveDatas()
    {
        saveDatas = RegIO.ReadCSV(saveDataFilePath)
            .Select(line => line.Where(data => !string.Equals(data, ""))
            .ToArray()).ToList();

        if (!outPutSaveDataLog) return;

        Debug.Log(saveDataFilePath);

        foreach (var save in saveDatas)
        {
            string line = "";

            foreach (var data in save) line += " | " + data;

            line += " | ";

            Debug.Log(line);
        }
    }

    public void Save(SaveData currentPlayData)
    {
        string data = "";
        data += currentPlayData.pass + ",";
        data += encrypter.EncryptInteger(currentPlayData.latestPart) + ",";
        data += currentPlayData.latestPartTitle + ",";
        data += encrypter.EncryptInteger(currentPlayData.maxJewel) + ",";
        data += encrypter.EncryptInteger(currentPlayData.collectedJewel) + ",";
        data += currentPlayData.jewelsBit + ",";
        
        foreach (var (maxScore, score) in currentPlayData.partScores)
        {
            data += encrypter.EncryptInteger(maxScore) + ",";
            data += encrypter.EncryptInteger(score) + ",";
        }

        var sr = new StreamWriter(saveDataFilePath, true);
        sr.WriteLine(data);
        sr.Close();

        LoadSaveDatas();
    }


    public void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        Debug.Log($"[onDeviceChange] {device}, {change}");

        switch (change)
        {
            // 新しいデバイスがシステムに追加された
            case InputDeviceChange.Added: break;

            // 既存のデバイスがシステムから削除された
            case InputDeviceChange.Removed: break;

            // 切断された
            case InputDeviceChange.Disconnected: break;

            // 再接続
            case InputDeviceChange.Reconnected: break;

            // 有効化
            case InputDeviceChange.Enabled: break;

            // 無効化
            case InputDeviceChange.Disabled: break;

            // 使用方法変更
            case InputDeviceChange.UsageChanged: break;

            // 構成変更
            case InputDeviceChange.ConfigurationChanged: break;

            case InputDeviceChange.SoftReset: break;

            case InputDeviceChange.HardReset: break;

            default: throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }
}
