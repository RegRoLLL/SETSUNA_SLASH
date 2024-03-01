using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class PL_Attack : SetsunaSlashScript
{
    public Game_HubScript hub;
    [SerializeField] SetsunaPlayerCamera camera_;

    public float slashMP;
    public float interval, interval_dt;

    public float 
        chargeRatio_mouse, 
        chargePower_pad, chargeAdjustRate_pad, chargeMax_pad, chargeMin_pad,
        chargeRatio_touch;
    public float chargeStartTime, charge_dTime;

    public GameObject normalAttackCol;

    [SerializeField] Image stickR, backR;

    [SerializeField] LineRenderer line;
    [SerializeField] GameObject slashEffect;

    [SerializeField] List<AudioClip> attackSounds = new();

    PlayerController_main plCtrler;
    PlayerAnimationCaller animCaller;
    PL_Status stat;

    PlayerInput input;
    InputAction 
        mouseChargeAction, 
        padAttackAction, padChargeAction, padChargeSlash, 
        touchChargeAction, touchChargeDir,
        chargeCancel;
    Vector2 startPos, endPos, direction, lastFrameStickInput;
    bool isDragging;

    void Start()
    {
        charge_dTime = 0;
        input = GetComponent<PlayerInput>();
        chargeCancel = input.actions[hub.action.chargeCancel];
        plCtrler = GetComponent<PlayerController_main>();
        animCaller = GetComponent<PlayerAnimationCaller>();
        stat = GetComponent<PL_Status>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (interval_dt < interval)
        {
            interval_dt += Time.deltaTime;
            return;
        }



        if (CancelMethod()) return;

        bool isGamepad = (config.controllMode == ConfigDatas.ControllMode.gamepad);
        bool isKeyMouse = (config.controllMode == ConfigDatas.ControllMode.keyboard_mouse);


        if (isGamepad)
        {
            GamePadMethod();
        }
        else if(isKeyMouse)
        {
            MouseMethod();
        }
        else
        {
            TouchMethod();
        }

        SetDir();
        SetLine();
    }

    bool CancelMethod()
    {
        if (!isDragging && !plCtrler.isDead) return false;

        var result = false;

        System.Action cancel = () =>
        {
            isDragging = false;
            charge_dTime = 0;

            result = true;
        };

        if (plCtrler.isDead) cancel();

        if (chargeCancel.WasPressedThisFrame()) cancel();

        if (plCtrler.stateP == PlayerController_main.state_pose.crouch) cancel();
        if (plCtrler.stateP == PlayerController_main.state_pose.teleport) cancel();

        camera_.setDistanceFromAverage = true;

        return result;
    }

    void GamePadMethod()
    {
        padAttackAction = input.actions[hub.action.attack];

        if (padAttackAction.WasPressedThisFrame())
        {
            Attack();
            return;
        }



        padChargeAction = input.actions[hub.action.charge_pad];
        padChargeSlash = input.actions[hub.action.chargeSlash_pad];
        Vector2 padInput = padChargeAction.ReadValue<Vector2>();

        if (Mathf.Approximately(1, padInput.magnitude))//入力がある時
        {
            if (lastFrameStickInput.magnitude == 0)//始まった時
            {
                startPos = Vector2.zero;
                isDragging = true;
            }

            if (padChargeSlash.WasPressedThisFrame())
            {
                if (isDragging && (charge_dTime >= chargeStartTime))
                {
                    ChargeSlash(transform.position, (Vector2)transform.position + direction);
                    isDragging = false;
                }
            }

            if (!isDragging) return;



            var adjustInput = input.actions[hub.action.chargeAdjust_pad].ReadValue<float>();
            var isRight = (padInput.x >= 0);

            chargePower_pad += chargeAdjustRate_pad * adjustInput * (isRight ? 1 : -1) * Time.deltaTime;
            chargePower_pad = Mathf.Clamp(chargePower_pad, chargeMin_pad, chargeMax_pad);

            endPos = padInput;
            direction = padInput * chargePower_pad;

            charge_dTime += Time.deltaTime;
        }
        else if(Mathf.Approximately(1, lastFrameStickInput.magnitude))//スティックが戻った時
        {
            charge_dTime = 0;
        }

        lastFrameStickInput = padInput;
    }

    void MouseMethod()
    {
        mouseChargeAction = input.actions[hub.action.charge_mouse];

        Func<Vector2> GetCurSorPos = () => Input.mousePosition;

        if (mouseChargeAction.WasPressedThisFrame())
        {
            startPos = GetCurSorPos();
            isDragging = true;
        }

        if (isDragging)
        {
            charge_dTime += Time.deltaTime;
            endPos = GetCurSorPos();
            direction = (startPos - endPos) * chargeRatio_mouse;
        }

        if (mouseChargeAction.WasReleasedThisFrame())
        {
            isDragging = false;


            if (charge_dTime >= chargeStartTime)
            {
                ChargeSlash(transform.position, (Vector2)transform.position + direction);
            }
            else
            {
                Attack();
            }

            charge_dTime = 0;
        }
    }

    void TouchMethod()
    {
        touchChargeAction = input.actions[hub.action.attack];
        touchChargeDir = input.actions[hub.action.charge_touch];
        var dir = touchChargeDir.ReadValue<Vector2>();

        if (touchChargeAction.WasPressedThisFrame())
        {
            isDragging = true;
            backR.transform.position = stickR.transform.position;
        }

        if (isDragging)
        {
            charge_dTime += Time.deltaTime;
            if(dir != Vector2.zero) direction = -dir * chargeRatio_touch;
            Debug.Log(direction);
        }

        if (touchChargeAction.WasReleasedThisFrame())
        {
            isDragging = false;


            if (charge_dTime >= chargeStartTime)
            {
                ChargeSlash(transform.position, (Vector2)transform.position + direction);
            }
            else
            {
                Attack();
            }

            charge_dTime = 0;
        }
    }

    void SetDir()
    {
        if (charge_dTime >= chargeStartTime)
        {
            camera_.setDistanceFromAverage = false;
            camera_.directionToPL = direction * 0.3f;
        }

        if ((plCtrler.inputVecX > 0) && (direction.x < 0)) direction.x = 0;
        else if((plCtrler.inputVecX < 0) && (direction.x > 0)) direction.x = 0;
        else if (Mathf.Approximately(plCtrler.inputVecX, 0))
        {
            if (direction.x > 0) plCtrler.spritePivot.transform.eulerAngles = Vector2.zero;
            else if (direction.x < 0) plCtrler.spritePivot.transform.eulerAngles = Vector2.up * 180;
        }
    }

    void SetLine()
    {
        line.gameObject.SetActive(charge_dTime >= chargeStartTime);

        if (charge_dTime >= chargeStartTime)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, (Vector2)transform.position + direction);
        }
    }




    void Attack()
    {
        animCaller.Attack();
        normalAttackCol.SetActive(true);

        AudioClip sound = attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)];
        plCtrler.seAS.PlayOneShot(sound);

        interval_dt = 0;
        charge_dTime = 0;
    }

    void ChargeSlash(Vector2 start, Vector2 end)
    {
        //以前はmp不足だと斬撃を放てなかったが、
        //仕様変更によりmp不足の場合にhpで肩代わりすることになった
        //if (!config.easyMode && (stat.mp < slashMP)) return;

        if (!config.easyMode) stat.MP_damage(slashMP);

        var effect = Instantiate(slashEffect);
        effect.GetComponent<SlashEffect>().SetData_charge(start, end);

        plCtrler.seAS.PlayOneShot(audioBind.player.chargeSlash);
        animCaller.Slash();

        interval_dt = 0;
    }
}
