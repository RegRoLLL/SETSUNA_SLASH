using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavePoint : SetsunaSlashScript
{
    [SerializeField] SavePointStatus status = new();
    public bool isArcSavePoint;

    [SerializeField] AudioSource seAS;
    public ParticleSystem saveEffect_front, saveEffect_back;
    [SerializeField] Sprite inactive, active;

    public float floatingCycle, floatHight;
    float dTime;

    StageManager manager;
    StagePart part;
    PlayerDetectArea area;
    SpriteRenderer sprite;

    Game_HubScript hub;
    Player player;

    bool initialized;


    void Initialize()
    {
        part = GetComponentInParent<StagePart>();
        manager = GetComponentInParent<StageManager>();
        area = GetComponentInChildren<PlayerDetectArea>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        sprite.sprite = inactive;

        if(isArcSavePoint) status.recommendSlashCount = -1;

        initialized = true;
    }


    void Update()
    {
        if (!initialized) Initialize();

        if (hub == null) hub = manager.hub;

        sprite.transform.localPosition = Vector2.up * (Mathf.Sin(dTime / floatingCycle) * floatHight);

        dTime += Time.deltaTime;

        if (isArcSavePoint && area.detected)
        {
            Debug.LogWarning("mpシステム改築中、アークセーブポイントの固有挙動は未実装です");
        }
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
            if (manager.latestSavePoint.status.nextSave == this)
            {
                part.AddPoint(player.Status.CalcScore());
            }
        }
        

        manager.savedPlayerPosition = transform.position;
        manager.latestSavePoint = this;
        status.isActivated = true;
        manager.hub.playingStage.SaveNotOverWrite(part.gameObject);

        sprite.sprite = active;

        player.Status.SetRecommendCount(status.recommendSlashCount);

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
