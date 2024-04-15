using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WindBlower : SetsunaSlashScript
{
    [SerializeField] List<ParticleSystem> blowParticles = new();
    [SerializeField] AudioSource seAS;

    [Header("TIME")]
    public float interval;
    public float jetTime, seFadeTime;

    [Header("POWER")]
    public float power_player;
    public float power_object, powerCurve;

    [Tooltip("距離減衰を利用するか。\r\ndefault = true")]
    public bool usePowerCurve;

    [Header("RAY_DATA")]
    public LayerMask rayMethodMask;
    public LayerMask blockMask;
    public int rayDensity;

    [Header("Appearance")]
    public SpriteRenderer line;
    public SpriteRenderer sign;
    public float dark_line, dark_sign, bright_line, bright_sign;

    [Header("INTERNAL_DATA")]
    public bool isEnable;
    [SerializeField] float dTime;
    HashSet<Collider2D> colliders = new HashSet<Collider2D>();

    BoxCollider2D colbox;
    float length;

    void Start()
    {
        dTime = interval;
        colbox = GetComponent<BoxCollider2D>();

        foreach (var p in blowParticles)
        {
            var main = p.main;
            main.loop = true;
        }


        Color color;

        color = line.color;
        color.a = dark_line;
        line.color = color;

        color = sign.color;
        color.a = dark_sign;
        sign.color = color;
    }

    void LateUpdate()
    {
        colliders.RemoveWhere((n) => (n == null));

        if(interval > 0)dTime += Time.deltaTime;

        SetValue();

        if (dTime < interval) return;//休止中

        float maxTime = (interval + (jetTime));


        if (dTime >= maxTime)//停止
        {
            DeActivate();
            return;
        }
        else if(!isEnable)//起動
        {
            Activate();
        }
        else//作動中
        {
            seAS.GetComponent<AudioVolumeManager>().SetVolume();

            if (dTime >= (maxTime - seFadeTime))//音量フェードアウト
            {
                seAS.volume *= ((maxTime - dTime) / seFadeTime);
            }

            var rayResult = RayCastMethod();
            var cols = colliders.Where((c) => !rayResult.Contains(c)).ToList();

            foreach (var col in cols)
            {
                //Debug.Log(col);
                TriggerMethod(col);
            }
        }
    }

    void SetValue()
    {
        float lastLength = length;
        length = colbox.size.y;

        if (Mathf.Approximately(length, lastLength)) return;

        //Debug.Log($"length: {length}");

        foreach (var p in blowParticles)
        {
            var main = p.main;
            main.startLifetimeMultiplier = (length / 5);
        }
    }

    public void Activate()
    {
        isEnable = true;

        foreach (var p in blowParticles)
        {
            p.Play();
        }

        Color color;

        color = line.color;
        color.a = bright_line;
        line.color = color;

        color = sign.color;
        color.a = bright_sign;
        sign.color = color;

        seAS.volume = config.seVolume;
        seAS.PlayOneShot(audioBind.gimmick.gayser);
    }

    public void DeActivate()
    {
        dTime = 0;
        isEnable = false;
        seAS.Stop();

        foreach (var p in blowParticles)
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Color color;

        color = line.color;
        color.a = dark_line;
        line.color = color;

        color = sign.color;
        color.a = dark_sign;
        sign.color = color;
    }

    /// <summary>
    /// 新設したraycastによる処理
    /// </summary>
    /// <returns>検出したコライダーのリスト</returns>
    List<Collider2D> RayCastMethod()
    {
        var result = new List<Collider2D>();

        float width = GetComponent<BoxCollider2D>().size.x * transform.lossyScale.x;
        int rayCount = (int)width * rayDensity;

        Vector2 center = transform.position,
                  step = transform.right * width / (rayCount - 1);
        Vector2 pos = (rayCount <= 1) ? (center) : (center - (Vector2)(transform.right * width / 2));
        Vector2 forceDir = transform.up;

        for (int i = 0; i < rayCount; i++)
        {
            var hit = Physics2D.Raycast(pos, forceDir, length, rayMethodMask);
            Collider2D col = hit.collider;

            Debug.DrawRay(pos, (forceDir * (col ? hit.distance : length)), (col ? Color.cyan : Color.magenta));

            pos += step;

            if (col == null) continue;

            result.Add(col);

            var ratio = GetRatio(hit.distance, true);

            //プレイヤーに対して
            if (col.gameObject.layer == playerLayer)
            {
                ToPlayer(col, ratio);
            }
            //非プレイヤーオブジェクトに対して
            else
            {
                ToObject(col, hit.point, ratio);
            }
        }

        return result;
    }

    /// <summary>
    /// 従来のTriggerなBoxColliderによる処理
    /// </summary>
    void TriggerMethod(Collider2D col)
    {
        var colPos = col.ClosestPoint(transform.position);
        var posUnderCol = GetPosUnderCol(col);
        var direction = (colPos - posUnderCol);
        var ratio = GetRatio(direction.magnitude, false);

        //Debug.Log(col +" | " + direction.magnitude + " | " + (ratio), col.gameObject);


        //重なっている場合は一番下の対象のみに影響を与える
        RaycastHit2D result = Physics2D.Raycast(posUnderCol, direction.normalized, length, blockMask);
        //Debug.DrawRay(posUnderCol, direction.normalized * length, Color.red);
        //Debug.Log($"{result.collider.gameObject} | {col.gameObject}");
        if ((result.collider == null) || ((result.collider != col) && (result.collider.gameObject.layer is slashableLayer or not_slashableLayer)))
        {
            //Debug.Log("blocked by " + result.collider, result.collider.gameObject);
            return;
        }


        //プレイヤーに対して
        if (col.gameObject.layer == playerLayer)
        {
            ToPlayer(col, ratio);
        }
        //非プレイヤーオブジェクトに対して
        else
        {
            ToObject(col, colPos, ratio);
        }
    }


    Vector2 GetPosUnderCol(Collider2D col)
    {
        var right = (Vector2)transform.right;
        var pos = (Vector2)transform.position;
        var colPos = col.ClosestPoint(pos);

        return pos + Vector2.Dot(colPos - pos, right) * right;
    }

    float GetRatio(float distance, bool linear)
    {
        if (!usePowerCurve) return 1;

        float ratio;

        if (linear) ratio = (1 / powerCurve) * (length - distance);
        else ratio = -Mathf.Pow(((length - powerCurve) - distance), 2) / Mathf.Pow(powerCurve, 2) + 1.1f;

        if (distance < length - powerCurve) ratio = 1;

        return ratio;
    }


    void ToPlayer(Collider2D col, float ratio)
    {
        //Debug.Log($"player | ratio:{ratio}");

        Vector2 forceDir = transform.up;
        var rb = col.GetComponentInParent<Rigidbody2D>();
        var vel = rb.velocity;
        var force = power_player * forceDir * ratio;

        //速度の力と反対方向の成分
        float dot = Vector2.Dot(vel, -forceDir);

        //力と反対方向の速度がある場合、打ち消す
        if (dot > 0)
        {
            vel += forceDir * dot;
        }
        //加速しすぎている場合の打ち消し
        else if(-dot > force.magnitude)
        {
            Debug.Log("clamp_player");
            vel -= force.normalized * (MathF.Abs(dot) - force.magnitude);
        }

        vel += force * Time.deltaTime;
        rb.velocity = vel;

        //Debug.Log($"{col.name} | vel:{rb.velocity} | ratio:{ratio} | deltaTime:{Time.deltaTime}");
    }

    void ToObject(Collider2D col, Vector2 colPos, float ratio)
    {
        Vector2 forceDir = transform.up;

        var rb = col.GetComponent<Rigidbody2D>();

        if (rb == null) return;

        var vel = rb.velocity;
        var force = power_object * ratio * forceDir;

        //速度の力と反対方向の成分
        float dot = Vector2.Dot(vel, -forceDir);

        //Debug.Log($"dot_obj: {dot}");

        //力と反対方向の速度がある場合、打ち消す
        if (dot > 0)
        {
            force += forceDir * dot * Time.deltaTime;
        }
        //加速しすぎている場合の打ち消し
        else if(-dot > force.magnitude)
        {
            //Debug.Log("clamp_obj");
            force *= 0;
            force = -forceDir * (MathF.Abs(dot) - force.magnitude) * Time.deltaTime;
        }

        rb.AddForceAtPosition(force * 100 * Time.deltaTime, colPos);

        //Debug.Log($"{col.name} | dir:{direction} | ratio:{ratio} | deltaTime:{Time.deltaTime}");
    }



    private void OnTriggerStay2D(Collider2D col)
    {
        //Debug.Log("trigger enter");

        if (col.gameObject.layer == bladeLayer) return;

        if (col.gameObject.layer == playerLayer)
        {
            colliders.Add(col);
        }
        else if (col.GetComponent<Rigidbody2D>())
        {
            colliders.Add(col);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        colliders.Remove(col);
    }
}
