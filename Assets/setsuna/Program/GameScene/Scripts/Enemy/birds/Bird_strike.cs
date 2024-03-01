using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird_strike : SetsunaSlashScript
{
    Game_HubScript hub;

    [SerializeField] float moveSpeed, damage;

    [SerializeField] PlayerDetectArea area;
    [SerializeField] PolygonCollider2D birdCollider;
    [SerializeField] FixedJoint2D joint;

    [SerializeField] bool isMoving, wasThroughedPlayer, hasDamaged, wasStinged;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        joint.enabled = false;
    }

    void Update()
    {
        if (area.detected) isMoving = true;

        if (!isMoving || wasStinged) return;

        if (hub == null)
            hub = GetComponentInParent<StageManager>().hub;


        rb.velocity = -transform.up * moveSpeed;

        if (wasThroughedPlayer) return;
        if (hub.player.transform.position.y > transform.position.y) wasThroughedPlayer = true;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!isMoving || wasStinged) return;

        if (col.gameObject.layer == playerLayer && !hasDamaged)
        {
            hasDamaged = true;
            hub.player.stat.HP_damage(damage);
        }
        else if(col.gameObject.layer is slashableLayer or not_slashableLayer)
        {
            wasStinged = true;
            birdCollider.isTrigger = false;

            if (col.GetComponent<Rigidbody2D>() is var colrb)
            {
                joint.connectedBody = colrb;
                joint.enabled = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (joint.connectedBody != null) return;
        
        //Debug.Log(col.gameObject.name);

        if (col.collider.GetComponent<Rigidbody2D>() is var colrb)
        {
            joint.connectedBody = colrb;
            joint.enabled = true;
        }
    }

    private void OnBecameVisible()
    {
        if (isMoving) return;
        isMoving = true;
    }

    private void OnBecameInvisible()
    {
        if (wasThroughedPlayer && !wasStinged)
        {
            Destroy(gameObject);
        }
    }
}
