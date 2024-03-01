using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationCaller : MonoBehaviour
{
    public Animator anim;
    PlayerController_main plCtrler;

    [SerializeField] bool walk, run, crouch, jump, fall;

    PlayerController_main.state_move stateM_cache;
    PlayerController_main.state_pose stateP_cache;

    void Start()
    {
        plCtrler = GetComponent<PlayerController_main>();
    }

    void Update()
    {
        SetAnim_new();
    }

    void SetAnim_new()
    {
        if (plCtrler.stateM != stateM_cache)
        {
            //Debug.Log("stateM changed.");

            walk = (plCtrler.stateM == PlayerController_main.state_move.walk);
            run = (plCtrler.stateM == PlayerController_main.state_move.run);
            jump = (plCtrler.stateM == PlayerController_main.state_move.jump);
            fall = (plCtrler.stateM == PlayerController_main.state_move.fall);

            if (anim.GetBool(PL_anim.moving))//前フレームで動いてるとき
            {
                if (walk || run)//走り出した or 走るのをやめた
                {
                    anim.SetBool(PL_anim.running, run);
                }
                else//止まった or 跳んだ
                {
                    anim.SetBool(PL_anim.moving, false);
                    if (stateM_cache == PlayerController_main.state_move.run) anim.SetBool(PL_anim.running, false);
                }
            }
            else //前フレームは止まってた時
            {
                if (walk || run)//動き出した
                {
                    anim.SetBool(PL_anim.moving, true);
                    anim.SetBool(PL_anim.running, run);
                }
            }

            if (jump || fall)//跳んだ
            {
                if (stateM_cache == PlayerController_main.state_move.run)
                {
                    anim.SetBool(PL_anim.jump_big, true);
                }
                else
                {
                    anim.SetBool(PL_anim.jump_little, true);
                }
            }
            else
            {
                anim.SetBool(PL_anim.jump_little, false);
                anim.SetBool(PL_anim.jump_big, false);
            }

            stateM_cache = plCtrler.stateM;
        }


        if (plCtrler.stateP != stateP_cache)
        {
            crouch = (plCtrler.stateP == PlayerController_main.state_pose.crouch);

            stateP_cache = plCtrler.stateP;

            anim.SetBool(PL_anim.crouch, crouch);
        }
    }

    void SetAnim_old()
    {
        walk = (plCtrler.stateM == PlayerController_main.state_move.walk);
        run = (plCtrler.stateM == PlayerController_main.state_move.run);
        jump = (plCtrler.stateM == PlayerController_main.state_move.jump);

        anim.SetBool(PL_anim.moving, (walk || run));
        anim.SetBool(PL_anim.running, run);


        crouch = (plCtrler.stateP == PlayerController_main.state_pose.crouch);

        anim.SetBool(PL_anim.crouch, crouch);
    }

    public void Slash()
    {
        anim.SetTrigger(PL_anim.slash);
    }

    public void Attack()
    {
        anim.SetTrigger(PL_anim.attack);
    }

    public void Damage()
    {
        anim.SetTrigger(PL_anim.damage);
    }

    public void Death(bool start)
    {
        anim.SetBool(PL_anim.dead, start);
    }

    class PL_anim
    {
        public const string
            moving = "moving",
            running = "running",
            crouch = "crouch",
            jump_little = "jump_little",
            jump_big = "jump_big",
            damage = "damage",
            slash = "slash",
            attack = "attack",
            dead = "dead";
    }
}
