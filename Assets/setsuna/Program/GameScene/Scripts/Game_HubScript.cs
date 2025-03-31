using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;

public class Game_HubScript : SetsunaSlashScript
{
    public int pl_layer;
    public Player player;
    [HideInInspector] public GameManager gm;
    public PlayerController_main PL_Ctrler { get => player.ctrler; }
    [HideInInspector] public SetsunaPlayerCamera camera_;

    public StageManager playingStage;
    public StagePart currentPart;
    public AudioSource bgmAS;

    void Start()
    {
        camera_ = player.cam;
        gm = gameObject.GetComponent<GameManager>();

        StartCoroutine(DelayOneFrame(() =>
        {
            //Debug.Log($"partsCount:{playingStage.stageParts.Count}", playingStage);
            SetPart(playingStage.stageParts[0].GetComponent<StagePart>());
        }));
    }

    IEnumerator DelayOneFrame(Action action)
    {
        yield return null;
        action?.Invoke();
    }

    public void SetPart(StagePart stage)
    {
        currentPart = stage;
        camera_.SetPos();

        playingStage.SetCurrentPart(playingStage.stageParts.FindIndex((n) => (n == stage.gameObject)));

        if (bgmAS.clip != stage.bgm)
        {
            bgmAS.clip = stage.bgm;

            if (stage.bgm_playOnEnter) bgmAS.Play();
            else bgmAS.Stop();
        }
        Camera.main.backgroundColor = stage.backGroundColor;
    }

    public Vector2 CameraPosClamp(Vector2 pos, Camera camera)
    {
        var yHalf = camera.ViewportToWorldPoint(Vector2.one).y - camera.transform.position.y;
        var xHalf = camera.ViewportToWorldPoint(Vector2.one).x - camera.transform.position.x;

        pos.x = Mathf.Clamp(pos.x, currentPart.rect.left + xHalf, currentPart.rect.right - xHalf);
        pos.y = Mathf.Clamp(pos.y, currentPart.rect.down + yHalf, currentPart.rect.up - yHalf);

        return pos;
    }
}
