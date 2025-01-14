using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractGimmick : MonoBehaviour
{
    [HideInInspector] public UnityEvent<Player> onInteractEvent;

    public void Interact(Player player)
    {
        onInteractEvent.Invoke(player);
    }
}
