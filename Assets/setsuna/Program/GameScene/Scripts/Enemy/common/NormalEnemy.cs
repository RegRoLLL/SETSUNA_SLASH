using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : SetsunaSlashScript
{
    [SerializeField] GameObject soul;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != bladeLayer) return;

        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        soul.SetActive(true);
        soul.transform.parent = this.transform.parent;

        yield return null;

        Destroy(gameObject);
    }
}
