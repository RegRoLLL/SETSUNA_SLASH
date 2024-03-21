using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// <br>==============================</br>
/// <br>必ずシーン内のオブジェクトにアタッチすること！！！</br>
/// <br>==============================</br>
/// </summary>
[DefaultExecutionOrder(100)]
public class SetsunaAudioKeeper : MonoBehaviour
{
    private static float audioScale, lastFrameScale;
    private static bool forceSetMode;

    [SerializeField] List<AudioVolumeManager> speakers = new();
    static List<AudioVolumeManager> speakers_static = new();

    [SerializeField] List<AudioVolumeManager> speakers_ignore = new();

    private void Awake()
    {
        speakers.Clear();
        foreach (var root in this.gameObject.scene.GetRootGameObjects())
        {
            speakers.AddRange(root.GetComponentsInChildren<AudioVolumeManager>(true));
        }
        speakers_static = speakers;
        speakers_static = speakers_static.Where((AVM) => (!speakers_ignore.Contains(AVM))).ToList();

        Initialize();
    }

    void Update()
    {
        UpdateAudioScale();

        lastFrameScale = audioScale;

        Initialize();
    }

    static void UpdateAudioScale()
    {
        if (lastFrameScale == audioScale) return;

        foreach (var AVM in speakers_static)
        {
            AVM.volumeScale = audioScale;
            AVM.SetVolume();
        }
    }

    void Initialize()
    {
        audioScale = 1f;
        forceSetMode = false;
    }

    static public float MultipleAudioScale(float ratio)
    {
        if (forceSetMode) return audioScale;

        audioScale *= ratio;

        return audioScale;
    }

    static public void Mute()
    {
        audioScale = 0f;
    }

    static public void ForceSetAudioScale(float scale)
    {
        if (audioScale == 0) return;

        audioScale = scale;
        forceSetMode = true;
    }
}
