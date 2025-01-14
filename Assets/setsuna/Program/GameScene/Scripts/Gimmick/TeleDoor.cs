using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InteractGimmick))]
public class TeleDoor : TeleportGimmick
{
    [SerializeField] TeleDoorToMode teleportTo = TeleDoorToMode.teleDoor;
    [SerializeField] SavePoint targetSavePoint;
    [SerializeField] SpriteRenderer interactIcon;

    GameObject player;
    PlayerInput input;

    enum TeleDoorToMode { teleDoor, savePoint }

    void Start()
    {
        interactIcon.enabled = false;
        GetComponent<InteractGimmick>().onInteractEvent.AddListener(InteractDoor);
    }

    public void InteractDoor(Player pl)
    {
        if (interactIcon.enabled == false) return;

        if (teleportTo == TeleDoorToMode.teleDoor)
        {
            Teleport(player.transform);
        }
        else
        {
            player.transform.position = targetSavePoint.transform.position;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != playerLayer) return;

        interactIcon.enabled = true;
        BecomeTeleportable();

        if (input != null) return;

        input = col.GetComponentInParent<PlayerInput>();
        player = col.GetComponentInParent<PlayerController_main>().gameObject;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer != playerLayer) return;

        interactIcon.enabled = false;
    }
}
