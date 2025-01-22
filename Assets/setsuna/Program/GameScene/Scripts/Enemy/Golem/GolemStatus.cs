using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemStatus : Status
{
    [SerializeField] Golem golem;
    [SerializeField] Game_HubScript hub;
    public bool invincible;
    public float stingerDamageSpeed;
    public float rockStingerDamageValue, slashDamageValue;
    bool isdead = false;

    void Start()
    {
        HP_heal(HP_max);
    }

    void Update()
    {
        base.Regene();

        if (HP > 0) return;

        if (!isdead)
        {
            golem.StartCoroutine(golem.Death());
            isdead = true;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (invincible)
        {
            if (col.gameObject.layer == bladeLayer)
            {
                HP_damage(1);
            }
        }
        else
        {
            if (col.gameObject.layer == bladeLayer)
            {
                HP_damage(slashDamageValue);
            }
            else if ((col.gameObject.layer == slashableLayer) && col.GetComponentInParent<RockStinger>())
            {
                HP_damage(rockStingerDamageValue);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != slashableLayer) return;

        if (!col.gameObject.GetComponent<Rigidbody2D>()) return;

        //Debug.Log("stinger collision: " + col.relativeVelocity.magnitude);

        if (col.relativeVelocity.magnitude < stingerDamageSpeed) return;

        HP_damage(rockStingerDamageValue);
        golem.StunMethod();
    }
}
