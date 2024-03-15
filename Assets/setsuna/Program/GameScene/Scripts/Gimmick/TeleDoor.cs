using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleDoor : TeleportGimmick
{
    [SerializeField] GameObject interactIcon;

    GameObject player;
    PlayerInput input;

    private void Start()
    {
        interactIcon.SetActive(false);
    }


    private void Update()
    {
        if (interactIcon.activeInHierarchy == false) return;

        if (input.actions[plAction.interact].WasPressedThisFrame() == false) return;

        Teleport(player.transform);
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
