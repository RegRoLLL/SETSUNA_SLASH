using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoul : SetsunaSlashScript
{
    [SerializeField] float speed;
    [SerializeField] float mpValue;

    [SerializeField]StageManager sm;
    [SerializeField]Game_HubScript hub;
    Rigidbody2D rb;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(sm == null) sm = GetComponentInParent<StageManager>();
        if (hub == null) hub = sm.hub;
        rb.velocity = ((Vector2)hub.PL_Ctrler.transform.position - (Vector2)transform.position).normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.LogWarning("mpシステム変更によりEnemySoulは廃止されました");

        //if (col.gameObject.layer == playerLayer)
        //{
        //    hub.PL_Ctrler.stat.MP_heal(mpValue);
        //    Destroy(gameObject);
        //}
    }
}
