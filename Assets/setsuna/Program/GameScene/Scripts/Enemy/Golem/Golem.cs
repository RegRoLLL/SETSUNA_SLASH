using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.U2D.Animation;

public class Golem : SetsunaSlashScript
{
    [SerializeField] AudioSource seAS;

    [SerializeField] GolemStatus stat;
    [SerializeField] GameObject hpBar;
    [SerializeField] Image hpBarFill;
    [SerializeField] ParticleSystem invincibleParticle;

    [SerializeField] PlayerDetectArea area, attackRange, closestRange;
    [SerializeField] GameObject wall;
    [SerializeField] Vector2 leftMarkerFixPos;
    [SerializeField] Animator animator;

    [SerializeField] bool excuting;
    [SerializeField] float inactivateTime, dTime;

    [SerializeField] float moveSPD, backStepSPD;
    [SerializeField] bool backStepping;

    [SerializeField] CoolTime punchCT, throwCT, stingCT;
    [SerializeField] Golem_Punch punch;

    [SerializeField] Rock rock;
    [SerializeField] int throwAttackCount;
    [SerializeField] Vector2 rockAppearPosLocal, rockSize;
    [SerializeField] float throwSpeed;

    public GameObject rockstinger, shield;
    [SerializeField] int stingAttackCount;
    [SerializeField] float upPosY, downPosY, xRange_large, xRange_small, stingerWidth;
    [SerializeField] List<RockStinger> rockStingers_up;

    [SerializeField] bool isStunning;
    [SerializeField] float stunTime;

    public List<GameObject> onBrokenDestroyObjects = new();

    StageManager stage;
    StageStat stagePart; Vector2 previewLeftMarker;
    Game_HubScript hub;
    Rigidbody2D rb;

    void Start()
    {
        stage = GetComponentInParent<StageManager>();
        stagePart = GetComponentInParent<StageStat>();
        hub = stage.hub;
        rb = GetComponent<Rigidbody2D>();
        wall.SetActive(false);
        shield.SetActive(false);
        stat.invincible = true;

        if (config.easyMode) { 
            stat.hp_max *= 0.5f;
            Debug.Log("golem nerfed.");
        }

        isStunning = false;
        dTime = 0;
    }

    void Update()
    {
        if(hub==null) hub = stage.hub;

        SetBattleStage();

        if (area.detected && !isStunning)
        {
            dTime = 0;
            animator.SetBool(GolemAnim.activated, true);

            if (!hub.bgmAS.isPlaying) hub.bgmAS.Play();
        }
        else if(!area.detected)
        {
            dTime += Time.deltaTime;

            if (dTime >= inactivateTime)
            {
                animator.SetBool(GolemAnim.activated, false);
                dTime = 0;
            }
        }

        invincibleParticle.gameObject.SetActive(stat.invincible && animator.GetBool(GolemAnim.activated));

        if (!area.detected || isStunning) return;

        if (animator.IsInTransition(0)) return;



        if (!backStepping)
        {
            if (attackRange.detected)
            {
                CloseRange();
            }
            else if (!closestRange.detected)
            {
                OutRange();
            }

            if (!closestRange.detected) AnyRange();
        }


        if (excuting) rb.velocity = Vector2.zero;
        else Move();
    }

    void LateUpdate()
    {
        hpBar.SetActive(animator.GetBool(GolemAnim.activated) || isStunning);
        hpBarFill.fillAmount = stat.hp / stat.hp_max;
    }


    void CloseRange()
    {
        if (excuting) return;

        punchCT.dt += Time.deltaTime;
        if (punchCT.dt >= punchCT.ct)
        {
            StartCoroutine(Punch());
            punchCT.dt = 0;
        }
    }

    void OutRange()
    {
        if (excuting) return;

        throwCT.dt += Time.deltaTime;
        if (throwCT.dt >= throwCT.ct)
        {
            StartCoroutine(Throw());
            throwCT.dt = 0;
        }
    }

    void AnyRange()
    {
        if (excuting) return;

        stingCT.dt += Time.deltaTime;
        if (stingCT.dt >= stingCT.ct)
        {
            StartCoroutine(RockStinger());
            stingCT.dt = 0;
        }
    }

    void Move()
    {
        float dir = (transform.position.x < hub.player.transform.position.x) ? 1 : -1;
        transform.rotation = Quaternion.Euler(0, 90 + (90 * dir), 0);

        if (closestRange.detected || (attackRange.detected && backStepping))
        {
            backStepping = true;

            var vel = rb.velocity;
            vel.x = backStepSPD * dir * -1f;
            rb.velocity = vel;
        }
        else if (!attackRange.detected)
        {
            backStepping = false;

            var vel = rb.velocity;
            vel.x = moveSPD * dir;
            rb.velocity = vel;
        }

        animator.SetBool(GolemAnim.moving, !attackRange.detected);
    }



    IEnumerator Punch()
    {
        excuting = true;

        animator.SetTrigger(GolemAnim.punch);

        yield return new WaitForSeconds(punchCT.untilAttackTime);

        punch.gameObject.SetActive(true);

        yield return new WaitForSeconds(punchCT.untilHitTime - punchCT.untilAttackTime);

        seAS.PlayOneShot(audioBind.enemy.golem.golem_punch);
        punch.particle.Play();
        punch.gameObject.SetActive(false);

        yield return new WaitForSeconds(punchCT.motionTime - punchCT.untilHitTime - punchCT.untilAttackTime);

        excuting = false;
    }

    IEnumerator Throw()
    {
        excuting = true;

        for (int i = 0; i < throwAttackCount; i++)
        {
            animator.SetTrigger(GolemAnim.throwing);

            yield return new WaitForSeconds(throwCT.untilAttackTime);

            var r = Instantiate(rock.gameObject).GetComponent<Rock>();
            r.transform.parent = this.transform.parent;
            r.transform.position = (Vector2)transform.position + rockAppearPosLocal;
            r.transform.localScale = rockSize;
            r.StartCoroutine(r.Appear());
            seAS.PlayOneShot(audioBind.enemy.golem.golem_throwGenerate);

            yield return new WaitForSeconds(throwCT.untilHitTime - throwCT.untilAttackTime);

            r.StartSimulate((hub.player.transform.position - r.transform.position).normalized * throwSpeed, true);
            seAS.PlayOneShot(audioBind.enemy.golem.golem_throw);

            yield return new WaitForSeconds(throwCT.motionTime - throwCT.untilHitTime - throwCT.untilAttackTime);

        }

        excuting = false;

        yield break;
    }

    IEnumerator RockStinger()
    {
        excuting = true;

        if (rockStingers_up.Count > 0)
        {
            foreach (var rs in rockStingers_up) rs.VanishMethod();
            rockStingers_up.Clear();
        }

        shield.SetActive(true);

        seAS.loop = true;
        seAS.PlayOneShot(audioBind.enemy.golem.golem_stingerAttack);

        for (int i = 0; i < stingAttackCount; i++)
        {
            animator.SetTrigger(GolemAnim.rockStinger);

            Sting(false);

            if (i == stingAttackCount - 1)//最後の１発
            {
                rockStingers_up = Sting(true);

                foreach (var rs in rockStingers_up) rs.Cold();
            }
            else Sting(true);

            yield return new WaitForSeconds(stingCT.untilAttackTime);

            yield return new WaitForSeconds(stingCT.untilHitTime - stingCT.untilAttackTime);

            yield return new WaitForSeconds(stingCT.motionTime - stingCT.untilHitTime - stingCT.untilAttackTime);
        }

        seAS.loop = false;
        seAS.Stop();

        shield.SetActive(false);

        excuting = false;

        yield break;
    }

    List<RockStinger> Sting(bool up)
    {
        var stingers = new List<RockStinger>();

        float   x,
                minPointX = (area.GetComponent<BoxCollider2D>().bounds.min.x + stingerWidth),
                maxPointX = (area.GetComponent<BoxCollider2D>().bounds.max.x - stingerWidth);

        Func<float,RockStinger> setStinger = (posXworld) =>
        {
            var s = Instantiate(rockstinger);
            s.transform.parent = transform.parent.parent;

            s.GetComponent<RockStinger>().shield = shield;

            var pos = s.transform.localPosition;
            pos.y = up ? upPosY : downPosY;
            pos.x = transform.parent.parent.InverseTransformPoint(Vector2.right * posXworld).x;
            s.transform.localPosition = pos;

            s.transform.localEulerAngles = Vector3.forward * (up ? 180 : 0);

            s.transform.localScale = Vector3.one;

            s.transform.parent = transform.parent;

            var rs = s.GetComponent<RockStinger>();
            stingers.Add(rs);

            return rs;
        };

        Func<float, bool> isSetable = (posXworld) =>
         {
             bool result = true;

             if ((posXworld < minPointX) || (maxPointX < posXworld)) return false;

             foreach (var s in stingers)
             {
                 if ((posXworld <= s.transform.position.x + stingerWidth) && (posXworld >= s.transform.position.x - stingerWidth))
                 {
                     result = false;
                     break;
                 }
             }

             return result;
         };

        var plPosWorld = hub.player.transform.position;

        

        if (up)
        {
            //プレイヤー位置(ズレあり)に１本
            var rs = setStinger(plPosWorld.x + UnityEngine.Random.Range(-xRange_small, xRange_small));

            //右に１本
            x = rs.transform.position.x + xRange_large;
            if (isSetable(x)) setStinger(x);

            //左に１本
            x = rs.transform.position.x - xRange_large;
            if (isSetable(x))
            {
                setStinger(x);

                if (stingers.Count == 2)//右に生やせなかった場合
                {
                    x -= xRange_large;
                    if (isSetable(x)) setStinger(x);
                }
            }
            else
            {
                x = rs.transform.position.x + (xRange_large * 2);
                if (isSetable(x)) setStinger(x);
            }
        }
        else
        {
            //プレイヤー位置に１本
            setStinger(plPosWorld.x);

            //重ならないようにランダムで2本
            for (int i = 0; i < 2; i++)
            {
                int n = 0;

                do
                {
                    x = UnityEngine.Random.Range(minPointX, maxPointX);

                    if (n++ >= 1000)
                    {
                        Debug.Log("infinity loop entered.");
                        break;
                    }
                } while (!isSetable(x));

                setStinger(x);
            }
        }

        return stingers;
    }


    public void StunMethod()
    {
        StartCoroutine(Stun());
    }
    IEnumerator Stun()
    {
        isStunning = true;
        stat.invincible = false;
        animator.SetBool(GolemAnim.activated, false);

        if (rockStingers_up.Count > 0)
        {
            foreach (var rs in rockStingers_up) rs.VanishMethod();
            rockStingers_up.Clear();
        }

        yield return new WaitForSeconds(stunTime);

        stat.invincible = true;
        animator.SetBool(GolemAnim.activated, true);
        isStunning = false;
    }



    void SetBattleStage()
    {
        if (wall.activeInHierarchy == area.detected) return;

        if (area.detected)
        {
            previewLeftMarker = stagePart.rectMarkers.left.localPosition;
            stagePart.rectMarkers.left.localPosition = leftMarkerFixPos;
            wall.SetActive(area.detected);
            stagePart.SetRect();
        }
        else
        {
            //stagePart.rectMarkers.left.localPosition = previewLeftMarker;
            //wall.SetActive(area.detected);
            //stagePart.SetRect();
        }

    }

    public IEnumerator Death()
    {
        var SRs = this.GetComponentsInChildren<SpriteRenderer>();
        var stat = GetComponentInParent<StageStat>().transform;

        foreach (var sr in SRs)
        {
            if (sr.gameObject == shield) continue;

            sr.transform.parent = stat;
            sr.GetComponent<SpriteSkin>().enabled = false;
            sr.gameObject.AddComponent<PolygonCollider2D>();
            sr.gameObject.AddComponent<Rigidbody2D>();
        }

        foreach (var obj in onBrokenDestroyObjects)
        {
            Destroy(obj);
            yield return null;
        }

        GameObject laststinger = null;

        if (rockStingers_up.Count > 0)
        {
            foreach (var rs in rockStingers_up)
            {
                rs.VanishMethod();
                laststinger = rs.gameObject;
            }
            rockStingers_up.Clear();
        }

        while (laststinger != null) yield return null;

        stagePart.rectMarkers.left.localPosition = previewLeftMarker;
        stagePart.SetRect();

        yield return null;

        Destroy(wall);

        this.transform.parent.parent = null;

        hub.bgmAS.Stop();

        seAS.transform.parent = null;
        seAS.loop = false;
        seAS.PlayOneShot(audioBind.enemy.golem.golem_death);

        hub.playingStage.SaveOverWrite(hub.playingStage.currentIndex);

        Destroy(this.transform.parent.gameObject);

        //Debug.Log("deathMethod finished.");
    }

    class GolemAnim
    {
        public const string
            activated = "activated",
            moving = "moving",
            throwing = "throwing",
            rockStinger = "rockStinger",
            punch = "punch";
    };

    [Serializable]
    struct CoolTime
    {
        public float ct, dt, motionTime, untilAttackTime, untilHitTime;
    }
}
