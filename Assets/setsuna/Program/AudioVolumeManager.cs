using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVolumeManager : MonoBehaviour
{
    public float volumeScale;

    [SerializeField] ConfigDatas config;
    [SerializeField] AudioSourceType type;


    void Start()
    {
        SetVolume();
    }

    public void SetVolume()
    {
        //Debug.Log("volume set.");

        var master = config.masterVolume;
        float volume = 0;

        switch (type)
        {
            case AudioSourceType.bgm:
                volume = config.bgmVolume;
                break;

            case AudioSourceType.soundEffect:
                volume = config.seVolume;
                break;

            default:
                break;
        }

        var AS = this.GetComponent<AudioSource>();

        AS.volume = master * volume * volumeScale;
    }

    enum AudioSourceType { bgm, soundEffect }
}
