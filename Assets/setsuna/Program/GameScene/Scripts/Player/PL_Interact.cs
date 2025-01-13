using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PL_Interact : MonoBehaviour
{
    Player pl;

    void Start()
    {
        pl = GetComponent<Player>();
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        var targetTrigger = pl.GetTriggers().Find(
            (t) => t.GetComponent<Collider2D>() && !t.GetComponent<StageStat>()
            );

        var targetGimmick = targetTrigger.GetComponentInChildren<InteractGimmick>();

        Debug.Log("onInteract", targetGimmick.gameObject);

        targetGimmick.Interact(pl);
    }
}
