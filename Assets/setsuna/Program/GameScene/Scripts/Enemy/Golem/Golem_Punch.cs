using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem_Punch : SetsunaSlashScript
{
    public ParticleSystem particle;

    public float atk;

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != playerLayer) return;

        var status = col.GetComponentInParent<PL_Status>();

        status.ConsumeCount();
    }
}
