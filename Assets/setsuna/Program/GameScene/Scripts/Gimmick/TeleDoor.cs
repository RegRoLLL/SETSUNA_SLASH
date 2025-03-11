using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InteractGimmick))]
public class TeleDoor : TeleportGimmick
{
    [Space(10)]
    [SerializeField] bool onlyGrounded = true, dontResetAfterTeleport = false;
    [SerializeField] TeleDoorToMode teleportTo = TeleDoorToMode.teleDoor;
    [SerializeField] SpriteRenderer interactIcon;

    Player player;
    bool touchingToPlayer;
    StageManager stage;
    [HideInInspector] public StagePart part;

    enum TeleDoorToMode { teleDoor, savePoint }

    void Start()
    {
        interactIcon.enabled = false;
        touchingToPlayer = false;

        var gimmick = GetComponent<InteractGimmick>();
        gimmick.onInteractEvent.AddListener(InteractDoor);
        interactIcon.sprite = gimmick.interactIconSprite;
    }

    void Update()
    {
        interactIcon.enabled = touchingToPlayer && (!onlyGrounded || player.IsGounded());
    }

    public void InteractDoor(Player pl)
    {
        if (stage == null) stage = GetComponentInParent<StageManager>();

        if (touchingToPlayer == false) return;

        if(onlyGrounded && !pl.IsGounded()) return;

        if (teleportTo == TeleDoorToMode.teleDoor)
        {
            Teleport(pl.transform);
            if (target.TryGetComponent<TeleDoor>(out var door))
            {
                if (door.part != null && door.part.isAnotherRoom)
                {
                    door.part.savePoints.GetSavePoints()[0].Excute(pl);
                }
            }
        }
        else
        {
            if (stage.latestSavePoint is var save and not null)
            {
                pl.transform.position = save.transform.position;
                save.Excute(player);
            }
            else if (stage.anotherPartSave is var anotherSave and not null)
            {
                pl.transform.position = anotherSave.transform.position;
                anotherSave.Excute(player);
            }
            else{
                Teleport(pl.transform);
            }
        }

        if (dontResetAfterTeleport) return;

        StartCoroutine(ResetAfterTeleportCoroutine());
    }

    IEnumerator ResetAfterTeleportCoroutine()
    {
        yield return null;

        var targetDoor = target.gameObject.GetComponent<TeleDoor>();
        //Debug.Log(targetDoor.part.gameObject, targetDoor.part.gameObject);
        stage.SaveNotOverWrite(targetDoor.part.gameObject);
        stage.Load();
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (part == null)
        {
            if (col.TryGetComponent<StagePart>(out var colPart))
            {
                part = colPart;
            }
        }

        if (col.gameObject.layer != playerLayer) return;

        BecomeTeleportable();

        player = col.GetComponentInParent<Player>();
        touchingToPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer != playerLayer) return;

        touchingToPlayer = false;
    }
}
