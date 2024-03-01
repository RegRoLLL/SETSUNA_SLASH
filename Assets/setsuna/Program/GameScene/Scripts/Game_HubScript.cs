using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;

public class Game_HubScript : SetsunaSlashScript
{
    public GameManager gm;
    public InputActionName action = new InputActionName();
    public int pl_layer;
    public PlayerController_main player;
    public SetsunaPlayerCamera camera_;
    public VideoPlayer moviePlayer;

    public StageManager playingStage;
    public StageStat currentPart;
    public AudioSource bgmAS;

    void Start()
    {
        SetPart(playingStage.stageParts[0].GetComponent<StageStat>());
    }

    public void SetPart(StageStat stage)
    {
        currentPart = stage;
        playingStage.currentIndex = playingStage.stageParts.FindIndex((n) => (n == stage.gameObject));

        camera_.GetComponent<Camera>().backgroundColor = stage.backGroundColor;

        if (bgmAS.clip != stage.bgm)
        {
            bgmAS.clip = stage.bgm;

            if (stage.bgm_playOnEnter) bgmAS.Play();
            else bgmAS.Stop();
        }
    }

    public Vector2 CameraPosClamp(Vector2 pos, Camera camera)
    {
        var yHalf = camera.ViewportToWorldPoint(Vector2.one).y - camera.transform.position.y;
        var xHalf = camera.ViewportToWorldPoint(Vector2.one).x - camera.transform.position.x;

        pos.x = Mathf.Clamp(pos.x, currentPart.rect.left + xHalf, currentPart.rect.right - xHalf);
        pos.y = Mathf.Clamp(pos.y, currentPart.rect.down + yHalf, currentPart.rect.up - yHalf);

        return pos;
    }


    [Serializable]
    public class InputActionName
    {
        public string 
            move, crouch, sprint, jump, attack, 
            charge_mouse, charge_pad, chargeAdjust_pad, chargeSlash_pad, chargeCancel,
            charge_touch,
            reset, pose;
    }
}
