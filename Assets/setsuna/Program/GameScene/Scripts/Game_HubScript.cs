using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;

public class Game_HubScript : SetsunaSlashScript
{
    public int pl_layer;
    public Player pl;
    [HideInInspector] public GameManager gm;
    [HideInInspector] public PlayerController_main player;
    [HideInInspector] public SetsunaPlayerCamera camera_;

    public StageManager playingStage;
    public StageStat currentPart;
    public AudioSource bgmAS;

    void Start()
    {
        player = pl.GetComponent<PlayerController_main>();
        camera_ = pl.cam;
        gm = gameObject.GetComponent<GameManager>();

        StartCoroutine(DelayOneFrame(() =>
        {
            //Debug.Log($"partsCount:{playingStage.stageParts.Count}", playingStage);
            SetPart(playingStage.stageParts[0].GetComponent<StageStat>());
        }));
    }

    IEnumerator DelayOneFrame(Action action)
    {
        yield return null;
        action?.Invoke();
    }

    public void SetPart(StageStat stage)
    {
        playingStage.currentIndex = playingStage.stageParts.FindIndex((n) => (n == stage.gameObject));

        playingStage.SetBackGround(next: stage);

        if (bgmAS.clip != stage.bgm)
        {
            bgmAS.clip = stage.bgm;

            if (stage.bgm_playOnEnter) bgmAS.Play();
            else bgmAS.Stop();
        }

        currentPart = stage;
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
