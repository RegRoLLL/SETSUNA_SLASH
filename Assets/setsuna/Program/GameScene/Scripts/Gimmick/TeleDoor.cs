using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleDoor : TeleportGimmick
{
    [SerializeField] TeleDoorToMode teleportTo = TeleDoorToMode.teleDoor;
    [SerializeField] SavePoint targetSavePoint;
    [SerializeField] GameObject interactIcon;

    GameObject player;
    PlayerInput input;

    enum TeleDoorToMode { teleDoor, savePoint }

    private void Start()
    {
        interactIcon.SetActive(false);
    }


    private void Update()
    {
        if (interactIcon.activeInHierarchy == false) return;

        if (input.actions[plAction.interact].WasPressedThisFrame() == false) return;

        if (teleportTo == TeleDoorToMode.teleDoor){ 
            Teleport(player.transform);
        }
        else { 
            player.transform.position = targetSavePoint.transform.position;
        }
    }



    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != playerLayer) return;

        interactIcon.SetActive(true);
        BecomeTeleportable();

        if (input != null) return;

        input = col.GetComponentInParent<PlayerInput>();
        player = col.GetComponentInParent<PlayerController_main>().gameObject;
    }


    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer != playerLayer) return;

        interactIcon.SetActive(false);
    }
}
