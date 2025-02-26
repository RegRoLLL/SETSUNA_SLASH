using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RC2D = UnityEngine.RigidbodyConstraints2D;

public class LevitationStone : SetsunaSlashScript
{
    [SerializeField] float targetDensityFixValue = 500;

    [SerializeField] float floatingLimit, floatSPD, floatableMass;
    [SerializeField] bool activeOnStart;
    [SerializeField] float activateTime, dTime;
    [SerializeField] Color inactiveColor, activeColor;

    [SerializeField] AudioSource seAS;
    [SerializeField] GameObject activateParticle;
    [SerializeField] ParticleSystem vanishParticle;
    [SerializeField] GameObject slashEffectPrefab;

    SlashEffect slashEffectInstance;

    bool activated,floatStarted,limitReached;

    FixedJoint2D joint;
    Rigidbody2D targetRB;
    Collider2D targetCol;
    Vector2 activatedPoint;
    float targetPrimeGravityScale, targetPrimeDensity;
    SpriteRenderer sr;

    readonly RC2D ySlideC = (RC2D.FreezePositionX | RC2D.FreezeRotation);
    readonly RC2D freezeC = RC2D.FreezeAll;
    readonly RC2D freeC = RC2D.None;

    void Start()
    {
        joint = GetComponent<FixedJoint2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.color = inactiveColor;

        slashEffectInstance = slashEffectPrefab.GetComponent<SlashEffect>();

        if(activeOnStart) Activate();
    }

    void LateUpdate()
    {
        if (!activated) return;

        if(!floatStarted)
        {
            if (dTime < activateTime)
            {
                dTime += Time.deltaTime;
                sr.color = Color.Lerp(inactiveColor, activeColor, (dTime / activateTime));
                return;
            }
            else 
            {
                floatStarted = true;

                activateParticle.SetActive(true);
                seAS.PlayOneShot(audioBind.gimmick.levitationStoneAwake);
                sr.color = activeColor;
            }
        }

        //ここから先は浮遊開始後のみ動作

        if (!targetRB) return;
        
        if ( !limitReached)
        {
            targetRB.velocity = floatSPD * Vector2.up;
            targetRB.constraints = ySlideC;

            if(transform.position.y >= activatedPoint.y + floatingLimit) limitReached = true;
        }
        else
        {
            targetRB.velocity = Vector2.zero;
            targetRB.constraints = freezeC;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == bladeLayer)
        {
            BreakWithSlash();
            return;
        }

        if (targetRB != null) return;

        if (!col.TryGetComponent<Rigidbody2D>(out var colRB)) return;

        targetRB = colRB;
        targetCol = col;
        joint.connectedBody = targetRB;

        var slashedDensity = slashEffectInstance.density;
        var targetCalcMass = targetRB.mass / targetCol.density * slashedDensity;

        if (activated || targetCalcMass <= floatableMass)
        {
            if (!activated) Activate();

            targetPrimeGravityScale = targetRB.gravityScale;
            targetPrimeDensity = targetCol.density;
            targetCol.density = targetDensityFixValue;
            targetRB.gravityScale = 0;
            targetRB.constraints = floatStarted ? ySlideC : freezeC;
        }
    }

    void Activate()
    {
        activated = true;
        activatedPoint = transform.position;
        dTime = 0;
    }


    void BreakWithSlash()
    {
        vanishParticle.transform.parent = null;
        vanishParticle.Play();

        if (targetRB)
        {
            targetRB.gravityScale = targetPrimeGravityScale;
            targetRB.constraints = freeC;
            targetCol.density = targetPrimeDensity;
        }

        Destroy(gameObject);
    }
}
