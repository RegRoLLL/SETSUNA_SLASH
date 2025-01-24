using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class TeleportGimmick : SetsunaSlashScript
{
    public TeleportGimmick target;
    bool teleported;

    void Start()
    {
        teleported = false;
    }

    public void Teleport(Transform playerT)
    {
        if (teleported) return;

        var distance = playerT.position - this.transform.position;
        playerT.position = target.transform.position + distance;
        target.TeleportRecieve();
    }

    public void TeleportRecieve()
    {
        teleported = true;
    }

    public void BecomeTeleportable()
    {
        teleported = false;
    }
}
