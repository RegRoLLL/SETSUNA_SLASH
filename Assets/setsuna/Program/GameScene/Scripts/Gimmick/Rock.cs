using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : SetsunaSlashScript
{
    [SerializeField] bool isConst;
    [SerializeField] float damageSpeed, damageValue;
    [SerializeField] float appearTime, appearRotationCount, vanishTime;
    [SerializeField] List<Sprite> sprites = new();

    SpriteRenderer sr;
    Rigidbody2D rb;
    Collider2D rockCol;
    ParticleSystem vanishParticle;

    
    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rockCol = GetComponent<Collider2D>();
        vanishParticle = GetComponentInChildren<ParticleSystem>();


        if (isConst) return;


        sr.sprite = sprites[Random.Range(0, sprites.Count)];

        sr.enabled = false;
        rb.simulated = false;
    }

    public IEnumerator Appear()
    {
        sr.enabled = true;

        float dt = 0, scale = transform.localScale.x;
        while (dt <= appearTime)
        {
            transform.Rotate(Vector3.forward * appearRotationCount * 360 * Time.deltaTime);
            transform.localScale = Vector2.one * (scale * (dt / appearTime));

            dt += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector2.one * scale;
    }

    public void StartSimulate(Vector2 velocity, bool gravity)
    {
        rb.simulated = true;

        if (!gravity) rb.gravityScale = 0;

        rb.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == playerLayer)
        {
            if (rb.velocity.magnitude < damageSpeed) return;

            col.gameObject.GetComponentInParent<PL_Status>().ConsumeCount();
        }
        else if (col.collider.GetComponent<Rock>())
        {
            StartCoroutine(Vanish());
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == bladeLayer) StartCoroutine(Vanish());
    }

    public IEnumerator Vanish()
    {
        vanishParticle.Play();


        yield return new WaitForSeconds(vanishTime);

        sr.enabled = false;
        rockCol.enabled = false;
        rb.simulated = false;

        while (vanishParticle.isPlaying) yield return null;

        Destroy(gameObject);
    }
}
