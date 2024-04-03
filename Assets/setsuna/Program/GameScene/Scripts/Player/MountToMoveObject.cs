using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MountToMoveObject : MonoBehaviour
{
    [SerializeField] float angle;

    [SerializeField, Tooltip("x,yÇªÇÍÇºÇÍÇ…èàóùÇìKópÇ∑ÇÈÇ©ÇÃê›íËÅB\r\ndefault: x = true, y = false")] 
    bool x = true, y = false;

    [Header("Internal property")]
    [SerializeField] bool readyToSetTarget;

    Rigidbody2D myRB, targetRB;
    Transform target;
    Vector2 targetLastFramePos;

    void Start()
    {
        Initialize();
    }

    private void FixedUpdate()
    {
        Function();
    }

    void Initialize()
    {
        readyToSetTarget = true;
        myRB = GetComponent<Rigidbody2D>();
    }

    void Function()
    {
        NullCheck();
        FixVelocity();
    }

    void NullCheck()
    {
        if (target != null || readyToSetTarget) return;

        readyToSetTarget = true;
    }

    void FixVelocity()
    {
        if (readyToSetTarget) return;

        var diff = ((Vector2)target.position - targetLastFramePos) / Time.fixedDeltaTime;
        targetLastFramePos = target.position;

        var vel = targetRB.velocity;

        //Debug.Log($"diff:{diff}  vel:{vel}");

        if(Mathf.Abs(vel.x) > Mathf.Abs(diff.x)) vel.x = diff.x;
        if(Mathf.Abs(vel.y) > Mathf.Abs(diff.y)) vel.y = diff.y;

        if (!x) vel.x = 0;
        if (!y) vel.y = 0;

        //Debug.Log($"vel:{vel}");

        myRB.velocity += vel;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (readyToSetTarget == false) return;

        if (col.transform.GetComponent<Rigidbody2D>() == false) return;
        if (target == col.transform) return;

        float collisionAngle;

        foreach (var contact in col.contacts)
        {
            collisionAngle = Vector2.Angle(Vector2.up, contact.normal);

            if (collisionAngle > angle / 2) continue;

            target = col.transform;
            targetLastFramePos = target.position;
            targetRB = target.GetComponent<Rigidbody2D>();
            readyToSetTarget = false;
            break;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (target != col.transform) return;

        target = null;
        targetRB = null;
        readyToSetTarget = true;
    }
}
