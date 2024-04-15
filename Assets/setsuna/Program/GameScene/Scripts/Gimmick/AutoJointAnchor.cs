using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AutoJointAnchor : SetsunaSlashScript
{
    [Tooltip("use constraints.freeze when joint not connected")]
    public bool isFreeMove;

    public LayerMask targetLayer;

    [Space()]
    public Joint2D joint;

    bool connected;

    void Start()
    {
        GetComponent<Collider2D>().callbackLayers = targetLayer;
        Disconnect();
    }

    void Update()
    {
        if (!(connected && joint.connectedBody == null)) return;

        Disconnect();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Connect(col);
    }

    void Connect(Collider2D col)
    {
        if (!col.GetComponent<Rigidbody2D>()) return;
        else if (joint.connectedBody != null)
        {
            if (joint.connectedBody.mass > col.attachedRigidbody.mass) return;
        }

        var colRB = col.GetComponent<Rigidbody2D>();

        joint.enabled = true;
        joint.connectedBody = colRB;
        joint.attachedRigidbody.constraints = RigidbodyConstraints2D.None;

        connected = true;
    }

    void Disconnect()
    {
        connected = false;

        joint.enabled = false;

        if (isFreeMove) return;
            
        joint.attachedRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }
}
