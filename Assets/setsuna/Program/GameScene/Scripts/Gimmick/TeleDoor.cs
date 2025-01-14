using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InteractGimmick))]
public class TeleDoor : TeleportGimmick
{
    [Space(10)]
    [SerializeField] bool onlyGrounded = true;
    [SerializeField] TeleDoorToMode teleportTo = TeleDoorToMode.teleDoor;
    [SerializeField] SavePoint targetSavePoint;
    [SerializeField] SpriteRenderer interactIcon;

    Player player;
    bool touchingToPlayer;

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
        if (touchingToPlayer == false) return;

        if(onlyGrounded && !pl.IsGounded()) return;

        if (teleportTo == TeleDoorToMode.teleDoor)
        {
            Teleport(pl.transform);
        }
        else
        {
            pl.transform.position = targetSavePoint.transform.position;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
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
