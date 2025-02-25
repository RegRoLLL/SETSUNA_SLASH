using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, RequireComponent(typeof(InteractGimmick))]
public class SavePoint : SetsunaSlashScript
{
    [SerializeField] SavePointStatus status = new();
    public bool isGoalSave, isAnotherPart;
    [SerializeField] List<HintUI.Hint> hints = new();

    [Space]
    [SerializeField] AudioSource seAS;
    [SerializeField] SpriteRenderer sprite, interactIcon;
    public ParticleSystem saveEffect_front, saveEffect_back;
    [SerializeField] Sprite inactive, active;

    public float floatingCycle, floatHight;
    float dTime;

    StageManager manager;
    StagePart part;
    PlayerDetectArea area;

    Game_HubScript hub;
    Player player;

    bool initialized;

    private void Start()
    {
        area = GetComponent<PlayerDetectArea>();
        GetComponent<InteractGimmick>().onInteractEvent.AddListener(InteractSavePoint);
    }

    void Initialize()
    {
        part = GetComponentInParent<StagePart>();
        manager = GetComponentInParent<StageManager>();

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

        interactIcon.enabled = hints.Count>=1 && area.detected;
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

    public void Excute(Player pl)
    {
        if (!initialized) Initialize();
        player = pl;

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
        }
        else
        {
            player.Status.SetRecommendCount(status.recommendSlashCount);
        }

        if (isGoalSave){
            player.ui.SlashCountUI.SetSubtitleLabel("", "");
        }
        else if(isAnotherPart){
            player.ui.SlashCountUI.SetSubtitleLabel(part.GetTitle(), "");
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
            if (manager.anotherPartSave != null) manager.anotherPartSave.Inactive();
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

    public void Inactive()
    {
        status.isActivated = false;
        sprite.sprite = inactive;
    }

    public void InteractSavePoint(Player pl)
    {
        if (hints.Count <= 0) return;
        pl.ui.OpenHintUI(hints, pl);
    }

    public StagePart GetPart() => part;

    public List<HintUI.Hint> GetHints() => hints;
    public void SetHints(List<HintUI.Hint> hintsData) => hints = hintsData;

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
