using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockStinger : SetsunaSlashScript
{
    [SerializeField] AudioSource seAS;

    public GameObject shield;

    [SerializeField] float power, damage, length, scale;
    [SerializeField] float stayTime, appearTime, vanishTime, dTime;
    [SerializeField] bool appearing, damaged;
    [SerializeField] ParticleSystem chargeParticle, appearParticle, vanishParticle;

    [SerializeField] GameObject stinger;
    [SerializeField] List<GameObject> variations = new();

    Rigidbody2D plRB;
    Vector2 startPos, targetPos;
    Coroutine operation;
    
    void Start()
    {
        appearing = false;
        Operate();
    }

    public void Operate()
    {
        operation = StartCoroutine(Operation());
    }

    IEnumerator Operation()
    {
        if (stinger == null)
        {
            stinger = Instantiate(variations[Random.Range(0, variations.Count)]);
            stinger.transform.parent = this.transform;
            yield return null;
            stinger.transform.localPosition = Vector2.zero - (Vector2.up * length);
            stinger.transform.localEulerAngles = Vector3.zero;
            stinger.transform.localScale = Vector3.one * scale;
        }


        targetPos = stinger.transform.position;
        stinger.transform.position -= transform.rotation * Vector2.up * length;
        startPos = stinger.transform.position;

        chargeParticle.Play();

        yield return new WaitForSeconds(stayTime);

        chargeParticle.gameObject.SetActive(false);
        appearParticle.Play();
        appearParticle.Play();
        appearing = true;

        seAS.PlayOneShot(audioBind.enemy.golem.golem_stinger, 0.2f);

        dTime = 0;
        while (dTime <= appearTime)
        {
            stinger.transform.position = Vector2.Lerp(startPos, targetPos, (dTime / appearTime));

            if (plRB != null) plRB.velocity = (transform.rotation * Vector2.up) * power;

            dTime += Time.deltaTime;
            yield return null;
        }
        stinger.transform.position = targetPos;

        appearing = false;

        if (vanishTime < 0) yield break;
        yield return new WaitForSeconds(vanishTime);

        StartCoroutine(Vanish());
    }

    public void VanishMethod()
    {
        StopCoroutine(operation);
        StartCoroutine(Vanish());
    }

    IEnumerator Vanish()
    {
        foreach (var mrs in GetComponentsInChildren<MeshRenderer>())
        {
            ParticleSystem.ShapeModule shape;
            ParticleSystem particle;

            if (mrs.GetComponent<Rigidbody2D>())
            {
                particle = Instantiate(vanishParticle);
                shape = particle.shape;
            }
            else
            {
                particle = vanishParticle;
                shape = particle.shape;
            }

            shape.meshRenderer = mrs;

            var option = particle.velocityOverLifetime.speedModifier;
            option.constant *= (mrs.transform.eulerAngles.z > 90) ? -1 : 1;

            particle.Play();

            shape.meshRenderer.gameObject.SetActive(false);
        }

        while (vanishParticle.isPlaying) yield return null;

        Destroy(gameObject);
    }

    public void Cold()
    {
        vanishTime = -1;
    }





    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!appearing) return;

        if (col.gameObject == shield)
        {
            VanishMethod();
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (!appearing) return;


        if (col.gameObject.layer != playerLayer) return;

        plRB = col.gameObject.GetComponentInParent<Rigidbody2D>();



        if (damaged) return;

        var state = col.gameObject.GetComponentInParent<PL_Status>();
        state.Damage();
        damaged = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (!appearing) return;

        if (col.gameObject.layer != playerLayer) return;

        plRB = null;
    }
}
