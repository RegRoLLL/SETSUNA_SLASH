using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikeiKun : SetsunaSlashScript
{
    [SerializeField] StageManager stage;
    [SerializeField] PlayerDetectArea area;
    [SerializeField] SlashEffect slashEffect;
    [SerializeField] ParticleSystem vanishParticle;

    public Vector2 primePos, target;
    public float speed, minDirectionX;

    Rigidbody2D rb;

    void Start()
    {
        stage = GetComponentInParent<StageManager>();
        primePos = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        target = (area.detected ? stage.hub.player.transform.position : primePos);

        var directionX = (target - (Vector2)transform.position).x;
        var direction = (directionX > 0) ? 1 : -1;

        if (Mathf.Abs(directionX) <= minDirectionX) direction = 0;

        rb.velocity = transform.right * speed * direction;
    }



    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.parent != this.transform) return;

        if (col.gameObject.layer == slashableLayer)
        {
            if (col.GetComponent<Rigidbody2D>())
            { 
                Vanish(); 
            }
        }
    }


    void Vanish()
    {
        StartCoroutine(VanishCoroutine());
    }

    IEnumerator VanishCoroutine()
    {
        while (transform.childCount > 0)
        {
            var t = transform.GetChild(0);
            t.parent = transform.parent;

            if (t.gameObject.layer == slashableLayer)
            {
                yield return null;
                continue;
            }

            if (!t.GetComponent<Rigidbody2D>() && (t.gameObject != vanishParticle.gameObject))
            {
                var rb = t.gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.useAutoMass = true;

                t.GetComponent<PolygonCollider2D>().density = slashEffect.density;
                t.GetComponent<PolygonCollider2D>().sharedMaterial = slashEffect.physicsMaterial;
            }
            yield return null;
        }

        vanishParticle.Play();
        Destroy(gameObject);
    }
}
