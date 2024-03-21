using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevitationStone : SetsunaSlashScript
{
    [SerializeField] AudioSource seAS;

    [SerializeField] float floatingLimit, floatSPD, floatableMass;
    [SerializeField] bool activated;
    [SerializeField] float activateTime, dTime;
    [SerializeField] Color inactiveColor, activeColor;
    [SerializeField] GameObject activateParticle;
    [SerializeField] ParticleSystem vanishParticle;

    FixedJoint2D joint;
    Rigidbody2D rb;
    Vector2 activatedPoint;
    public float targetPrimeGravityScale;
    SpriteRenderer sr;

    void Start()
    {
        joint = GetComponent<FixedJoint2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.color = inactiveColor;
    }

    void LateUpdate()
    {
        if (!activated) return;

        if (dTime < activateTime)
        {
            dTime += Time.deltaTime;
            sr.color = Color.Lerp(inactiveColor, activeColor, (dTime / activateTime));
            return;
        }
        else if (!activateParticle.activeInHierarchy)
        {
            activateParticle.SetActive(true);
            seAS.PlayOneShot(audioBind.gimmick.levitationStoneAwake);
            sr.color = activeColor;
        }
        
        if (transform.position.y <= activatedPoint.y + floatingLimit)
        {
            rb.gravityScale = -floatSPD;
        }
        else
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            if(joint.connectedBody)joint.connectedBody.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == bladeLayer)
        {
            BreakWithSlash();
            return;
        }

        if (!col.GetComponent<Rigidbody2D>()) return;

        var colRB = col.GetComponent<Rigidbody2D>();
        
        joint.connectedBody = colRB;

        if (activated || colRB.mass <= floatableMass)
        {
            targetPrimeGravityScale = colRB.gravityScale;
            colRB.gravityScale = 0;

            if (activated) return;

            activated = true;
            activatedPoint = transform.position;
            rb.constraints = (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation);
            dTime = 0;
        }
    }


    void BreakWithSlash()
    {
        vanishParticle.transform.parent = null;
        vanishParticle.Play();

        var rb_target = joint.connectedBody;
        rb_target.gravityScale = targetPrimeGravityScale;
        rb_target.constraints = RigidbodyConstraints2D.None;

        Destroy(gameObject);
    }
}
