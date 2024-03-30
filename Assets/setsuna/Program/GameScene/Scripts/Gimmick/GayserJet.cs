using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GayserJet : SetsunaSlashScript
{
    [SerializeField] List<ParticleSystem> blowParticles = new();
    [SerializeField] AudioSource seAS;

    public float interval, jetTime, seFadeTime, power_player, power_object, powerCurve;
    public LayerMask rayMask;
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
            if (dTime >= (maxTime - seFadeTime))//音量フェードアウト
            {
                seAS.volume = (float)(config.seVolume * seAS.GetComponent<AudioVolumeManager>().volumeScale * (maxTime - dTime) / seFadeTime);
            }


            //力を加える方向の単位ベクトル
            Vector2 forceDir = transform.up;


            foreach (var col in colliders)//範囲内のコライダーすべてに対してそれぞれ
            {
                var right = (Vector2)transform.right;
                var pos = (Vector2)transform.position;
                var colPos = col.ClosestPoint(pos);
                var posUnderCol = pos + Vector2.Dot(colPos - pos, right) * right;
                var direction = (colPos - posUnderCol);

                var ratio = -Mathf.Pow(((length - powerCurve) - direction.magnitude), 2) / Mathf.Pow(powerCurve, 2) + 1;
                if (ratio <= 0) ratio = 0.1f;
                if (direction.magnitude < length - powerCurve) ratio = 1;

                //Debug.Log(col.gameObject.name +" | " + direction.magnitude + " | " + (ratio), col.gameObject);


                //重なっている場合は一番下の対象のみに影響を与える
                RaycastHit2D result = Physics2D.Raycast(posUnderCol, direction.normalized, length,rayMask);
                //Debug.DrawRay(posUnderCol, direction.normalized * length, Color.red);
                //Debug.Log($"{result.collider.gameObject} | {col.gameObject}");
                if ((result.collider == null) || (result.collider.gameObject != col.gameObject))
                {
                    //Debug.Log("blocked by " + result.collider.gameObject.name, result.collider.gameObject);
                    continue;
                }


                if (col.gameObject.layer == playerLayer)//プレイヤーに対して
                {
                    var rb = col.GetComponentInParent<Rigidbody2D>();
                    var vel = rb.velocity;
                    var force = power_player * forceDir * ratio * Time.deltaTime;

                    //力と反対方向の速度がある場合、打ち消す
                    if (Vector2.Dot(vel, -forceDir) is float dot and > 0)
                    {
                        vel += forceDir * dot + force;
                    }

                    vel += force;
                    rb.velocity = vel;

                    //Debug.Log($"{col.name} | vel:{rb.velocity} | ratio:{ratio} | deltaTime:{Time.deltaTime}");
                }
                else//非プレイヤーオブジェクトに対して
                {
                    var rb = col.GetComponent<Rigidbody2D>();
                    var vel = rb.velocity;
                    var force = power_object * ratio * forceDir * Time.deltaTime;

                    //力と反対方向の速度がある場合、打ち消す
                    if (Vector2.Dot(vel, -forceDir) is float dot and > 0)
                    {
                        rb.velocity += forceDir * dot;
                    }

                    rb.AddForceAtPosition(force*100, colPos);

                    //Debug.Log($"{col.name} | dir:{direction} | ratio:{ratio} | deltaTime:{Time.deltaTime}");
                }
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
