using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : SetsunaSlashScript
{
    [SerializeField] StageManager manager;
    [SerializeField] StageStat part;
    [SerializeField] AudioSource seAS;
    [SerializeField] PlayerDetectArea area;

    public bool isArcSavePoint;
    public float healPerSec;

    public SpriteRenderer sprite;
    public ParticleSystem particle, saveEffect_front, saveEffect_back;
    [SerializeField] Sprite inactive, active;

    public float floatingCycle, floatHight;
    [SerializeField] float dTime;

    Game_HubScript hub;


    void Start()
    {
        sprite.sprite = inactive;
        part = GetComponentInParent<StageStat>();
        manager = GetComponentInParent<StageManager>();
    }


    void Update()
    {
        if (hub == null) hub = manager.hub;

        sprite.transform.localPosition = Vector2.up * (Mathf.Sin(dTime / floatingCycle) * floatHight);

        dTime += Time.deltaTime;

        if (isArcSavePoint && area.detected)
        {
            Debug.LogWarning("mpシステム改築中、セーブポイントの挙動が未実装です");
            //hub.PL_Ctrler.stat.HP_heal(healPerSec*Time.deltaTime);
            //hub.PL_Ctrler.stat.MP_heal(healPerSec * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != manager.hub.pl_layer) return;

        if (manager.hub.gm.isSaving) return;

        SavePointExcute();
    }

    void SavePointExcute()
    {
        Debug.LogWarning("mpシステム改築中、セーブポイントの挙動が未実装です");
        manager.savedPlayerPosition = transform.position;
        //manager.savedPlayerHP = manager.hub.PL_Ctrler.GetComponent<PL_Status>().HP;
        //manager.savedPlayerMP = manager.hub.PL_Ctrler.GetComponent<PL_Status>().MP;
        manager.hub.playingStage.SaveNotOverWrite(part.gameObject);

        sprite.sprite = active;

        seAS.PlayOneShot(audioBind.gimmick.savePoint);
        saveEffect_front.Play();
        saveEffect_back.Play();
    }
}
