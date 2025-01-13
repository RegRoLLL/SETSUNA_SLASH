using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractGimmick : SetsunaSlashScript
{
    public UnityEvent<Player> onInteractEvent;

    public void Interact(Player player)
    {
        Debug.Log("Gimmick interacted", gameObject);
        onInteractEvent.Invoke(player);
    }
}
