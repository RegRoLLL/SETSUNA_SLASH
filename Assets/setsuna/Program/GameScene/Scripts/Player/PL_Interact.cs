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
            (t) => !t.GetComponent<StagePart>() && t.GetComponentInChildren<InteractGimmick>()
            );

        if (targetTrigger == null) return;

        var targetGimmick = targetTrigger.GetComponentInChildren<InteractGimmick>();

        targetGimmick.Interact(pl);
    }
}
