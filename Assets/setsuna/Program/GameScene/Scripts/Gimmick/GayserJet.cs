using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GayserJet : SetsunaSlashScript
{
    [SerializeField] List<ParticleSystem> blowParticles = new();
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
            foreach (var p in blowParticles)
            {
                var main = p.main;
                main.loop = true;
            }
        }
    }

    void LateUpdate()
    {
        colliders.RemoveWhere((n) => (n == null));

        if(interval > 0)dTime += Time.deltaTime;

        SetValue();

        if (dTime < interval) return;//�x�~��

        float maxTime = (interval + (jetTime));


        if (dTime >= maxTime)//��~
        {
            dTime = 0;
            isEnable = false;
            seAS.Stop();
            return;
        }
        else if(!isEnable)//�N��
        {
            isEnable = true;

            foreach (var p in blowParticles)
            {
                var main = p.main;
                main.duration = jetTime * main.simulationSpeed;
                p.Play();
            }

            seAS.volume = config.seVolume;
            seAS.PlayOneShot(audioBind.gimmick.gayser);
        }
        else//�쓮��
        {
            if (dTime >= (maxTime - seFadeTime))//���ʃt�F�[�h�A�E�g
            {
                seAS.volume = (float)(config.seVolume * (maxTime - dTime) / seFadeTime);
            }

            foreach (var col in colliders)//�͈͓��̃R���C�_�[���ׂĂɑ΂��Ă��ꂼ��
            {
                var colPos = col.ClosestPoint(transform.position);
                var underColPos = new Vector2(colPos.x, transform.position.y);
                var direction = (colPos.y - transform.position.y);
                var ratio = -Mathf.Pow((length - direction) * powerCurve, 2) + powerCurve;
                if (ratio < 0) ratio = 0.1f;

                if(direction < length - powerCurve)ratio = 1;

                //Debug.Log(col.gameObject.name +" | " + direction.magnitude + " | " + (power_player * ratio));


                //�d�Ȃ��Ă���ꍇ�͈�ԉ��̑Ώۂ݂̂ɉe����^����
                RaycastHit2D result = Physics2D.Raycast(underColPos, Vector2.up * direction);
                if ((result.collider == null) || ((result.collider != col) && (result.collider.gameObject.layer == slashableLayer)))
                {
                    //Debug.Log("blocked by " + result.collider.gameObject.name) ;
                    continue;
                }


                if ((col.gameObject.layer == playerLayer) || true)//�v���C���[�ɑ΂���
                {
                    var rb = col.GetComponentInParent<Rigidbody2D>();
                    var vel = rb.velocity;
                    vel.y = (power_player * ratio);
                    rb.velocity = vel;
                    rb.angularVelocity /= 2;
                }
                else//��v���C���[�I�u�W�F�N�g�ɑ΂���
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

        foreach (var p in blowParticles)
        {
            var main = p.main;
            main.startLifetimeMultiplier = (length / 5);
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
