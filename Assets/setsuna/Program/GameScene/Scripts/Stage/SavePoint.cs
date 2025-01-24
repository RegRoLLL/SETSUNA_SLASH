using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavePoint : SetsunaSlashScript
{
    [SerializeField] SavePointStatus status = new();
    public bool isGoalSave, isAnotherPart;

    [SerializeField] AudioSource seAS;
    public ParticleSystem saveEffect_front, saveEffect_back;
    [SerializeField] Sprite inactive, active;

    public float floatingCycle, floatHight;
    float dTime;

    StageManager manager;
    StagePart part;
    SpriteRenderer sprite;

    Game_HubScript hub;
    Player player;

    bool initialized;


    void Initialize()
    {
        part = GetComponentInParent<StagePart>();
        manager = GetComponentInParent<StageManager>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        sprite.sprite = inactive;

        if(isGoalSave) status.recommendSlashCount = -1;

        initialized = true;
    }


    void Update()
    {
        if (!initialized) Initialize();

        if (hub == null) hub = manager.hub;

        sprite.transform.localPosition = Vector2.up * (Mathf.Sin(dTime / floatingCycle) * floatHight);

        dTime += Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (status.isActivated) return;

        if (!initialized) Initialize();

        if (col.gameObject.layer != manager.hub.pl_layer) return;

        if (player == null) player = col.GetComponentInParent<Player>();

        if (manager.hub.gm.isSaving) return;

        SavePointExcute();
    }

    void SavePointExcute()
    {
        if (manager.latestSavePoint != null){
            if (manager.latestSavePoint.status.nextSave is var lastNext and not null){
                //区間終わりのセーブポイント
                if (lastNext == this) AreaClear();
            }
        }

        AreaStart();

        Save();
        Activate();
    }
    void AreaStart()
    {
        if (isAnotherPart)
        {
            player.Status.SetAnotherPart();
            return;
        }

        player.Status.SetRecommendCount(status.recommendSlashCount);

        if (isGoalSave){
            player.ui.SlashCountUI.SetSubtitleLabel("", "");
        }
        else{
            player.ui.SlashCountUI.SetSubtitleLabel(part.GetTitle(), this.gameObject.name);
        }
    }
    void AreaClear()
    {
        part.AddPoint(player.Status.CalcScore());
    }
    void Save()
    {
        manager.savedPlayerPosition = transform.position;

        if (!isAnotherPart)
        {
            manager.latestSavePoint = this;
        }
        manager.anotherPartSave = isAnotherPart ? this : null;
        
        manager.hub.playingStage.SaveNotOverWrite(part.gameObject);
    }
    void Activate()
    {
        status.isActivated = true;
        sprite.sprite = active;
        seAS.PlayOneShot(audioBind.gimmick.savePoint);
        saveEffect_front.Play();
        saveEffect_back.Play();
    }



    [Serializable]
    public class SavePointStatus
    {
        public SavePointStatus() { }
        public SavePointStatus(SavePointStatus stat)
        {
            isActivated = stat.isActivated;
            recommendSlashCount = stat.recommendSlashCount;
            isAreaCleared = stat.isAreaCleared;
            nextSave = stat.nextSave;
        }

        public bool isActivated;
        public int recommendSlashCount;
        public bool isAreaCleared;
        [SerializeField] public SavePoint nextSave;
    }

    public void SetData(SavePointStatus data)
    {
        if (!initialized) Initialize();

        status = data;
        if(data.isActivated) sprite.sprite = active;
    }

    public SavePointStatus GetData() => status;

    public void SetNext(SavePoint point) => status.nextSave = point;
}
