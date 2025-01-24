using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableJewel : SetsunaSlashScript
{
    [SerializeField] Color collectedColor;

    public bool IsCollected { get; private set; }

    public void SetCollected()
    {
        IsCollected = true;

        GetComponent<SpriteRenderer>().color = collectedColor;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        GetDecision(col.gameObject);
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        GetDecision(col.gameObject);
    }

    void GetDecision(GameObject obj)
    {
        if (obj.layer != playerLayer) return;

        if (!IsCollected)
        {
            var part = this.GetComponentInParent<StagePart>();
            this.GetComponentInParent<StageManager>().CollectJewel(part.GetTitle());
            SetCollected();
        }

        gameObject.SetActive(false);
    }
}
