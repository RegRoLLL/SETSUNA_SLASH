using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GayserJet : SetsunaSlashScript
{
    [SerializeField] ParticleSystem vape, jet, jet_front;
    [SerializeField] AudioSource seAS;

    public float interval, jetTime, seFadeTime, power_player, power_object, powerCurve;
    public bool isEnable;
    [SerializeField] float dTime;
    HashSet<Collider2D> colliders = new HashSet<Collider2D>();

    BoxCollider2D colbox;
    float length;

    void Start()
    {
        dTime = interval;
        colbox = GetComponent<BoxCollider2D>();

        if (interval <= 0)
        {
            var main = jet.main;
            main.loop = true;
            main = jet_front.main;
            main.loop = true;
        }
    }

    void LateUpdate()
    {
        colliders.RemoveWhere((n) => (n == null));

        if(interval > 0)dTime += Time.deltaTime;

        SetValue();

        if (dTime < interval) return;

        float maxTime = (interval + jetTime + jet.main.startLifetimeMultiplier / 2);


        if (dTime >= maxTime)
        {
            dTime = 0;
            isEnable = false;
            seAS.Stop();
            return;
        }
        else if(!isEnable)
        {
            isEnable = true;
            jet.time = jetTime;
            jet_front.time = jetTime;
            jet.Play();
            jet_front.Play();

            seAS.volume = config.seVolume;
            seAS.PlayOneShot(audioBind.gimmick.gayser);
        }
        else
        {
            if (dTime >= (maxTime - seFadeTime))
            {
                seAS.volume = (float)(config.seVolume * (maxTime - dTime) / seFadeTime);
            }

            foreach (var col in colliders)
            {
                var direction = (col.ClosestPoint(transform.position) - (Vector2)transform.position);
                var ratio = -Mathf.Pow((length - direction.magnitude ) * powerCurve, 2) + powerCurve;
                if (ratio < 0) ratio = 0.1f;

                if(direction.magnitude < length - powerCurve)ratio = 1;

                //Debug.Log(col.gameObject.name +" | " + direction.magnitude + " | " + (power_player * ratio));

                RaycastHit2D result = Physics2D.Raycast(transform.position, direction);
                if ((result.collider == null) || ((result.collider != col) && (result.collider.gameObject.layer == slashableLayer)))
                {
                    //Debug.Log("blocked by " + result.collider.gameObject.name) ;
                    continue;
                }


                if ((col.gameObject.layer == playerLayer) || true)
                {
                    var rb = col.GetComponentInParent<Rigidbody2D>();
                    var vel = rb.velocity;
                    vel.y = (power_player * ratio);
                    rb.velocity = vel;
                    rb.angularVelocity /= 2;
                }
                else
                {
                    var force = transform.rotation * Vector2.up * power_object * ratio;
                    var rb = col.GetComponent<Rigidbody2D>();
                    rb.AddForceAtPosition(force, col.ClosestPoint(transform.position));
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

        Action<ParticleSystem> SetJet = (j) => {
            var main = j.main;
            main.startLifetimeMultiplier = length * 0.3f;
            main.startSpeedMultiplier = length * 2.1f;
            main.gravityModifierMultiplier = length / 5f;

            var emission = j.emission;
            emission.rateOverTimeMultiplier = length * 200f;
        };

        SetJet(jet);
        SetJet(jet_front);
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
