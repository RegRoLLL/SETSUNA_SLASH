using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using RegUtil;

public class PL_Attack : SetsunaSlashScript
{
    [SerializeField] PlayerInputActionName plAction;

    public float slashMP;
    public float interval, interval_dt;

    public float
        chargeRatio_mouse,
        chargePower_pad, chargeAdjustRate_pad, chargeMax_pad, chargeMin_pad,
        chargeRatio_touch;

    [Space(10)]
    public float chargeSlowTimeRatio;
    public float chargeAudioSmallRatio;

    public float chargeStartTime, charge_dTime;
    public GameObject normalAttackCol;

    [SerializeField] Image stickR, backR;

    [SerializeField] LineRenderer line;
    [SerializeField] GameObject slashEffect;

    [SerializeField] List<AudioClip> attackSounds = new();

    Player pl;

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
        pl = GetComponent<Player>();

        charge_dTime = 0;
        input = GetComponent<PlayerInput>();
        chargeCancel = input.actions[plAction.chargeCancel];
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (interval_dt < interval)
        {
            interval_dt += Time.deltaTime;
            return;
        }

        if (isDragging && config.slowWhenSlashCharge)
        {
            RegTimeKeeper.MultipleTimeScale(ratio: chargeSlowTimeRatio);
            SetsunaAudioKeeper.MultipleAudioScale(ratio: chargeAudioSmallRatio);
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
        if (!isDragging && !pl.GetIsDead()) return false;

        var result = false;

        if (pl.GetIsDead()) result = true;

        if (chargeCancel.WasPressedThisFrame()) result = true;

        if (pl.GetStatePose() == PlayerController_main.state_pose.crouch) result = true;
        if (pl.GetStatePose() == PlayerController_main.state_pose.teleport) result = true;

        if (result)
        {
            isDragging = false;
            charge_dTime = 0;
        }

        pl.SetCameraDistanceSetMode(true);

        return result;
    }

    void GamePadMethod()
    {
        padAttackAction = input.actions[plAction.attack];

        if (padAttackAction.WasPressedThisFrame())
        {
            Attack();
            return;
        }



        padChargeAction = input.actions[plAction.charge_pad];
        padChargeSlash = input.actions[plAction.chargeSlash_pad];
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



            var adjustInput = input.actions[plAction.chargeAdjust_pad].ReadValue<float>();
            var isRight = (padInput.x >= 0);

            chargePower_pad += chargeAdjustRate_pad * adjustInput * (isRight ? 1 : -1) * Time.deltaTime;
            chargePower_pad = Mathf.Clamp(chargePower_pad, chargeMin_pad, chargeMax_pad);

            endPos = padInput;
            direction = padInput * chargePower_pad;

            charge_dTime += Time.unscaledDeltaTime;
        }
        else if(Mathf.Approximately(1, lastFrameStickInput.magnitude))//スティックが戻った時
        {
            charge_dTime = 0;
        }

        lastFrameStickInput = padInput;
    }

    void MouseMethod()
    {
        mouseChargeAction = input.actions[plAction.charge_mouse];

        if (mouseChargeAction.WasPressedThisFrame())
        {
            startPos = Input.mousePosition;
            isDragging = true;
        }

        if (isDragging)
        {
            charge_dTime += Time.unscaledDeltaTime;
            endPos = Input.mousePosition;
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
        touchChargeAction = input.actions[plAction.attack];
        touchChargeDir = input.actions[plAction.charge_touch];
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
            pl.SetCameraDistanceSetMode(false);
            pl.SetCameraDirection(direction * 0.3f);
        }

        if ((pl.GetInputX() > 0) && (direction.x < 0)) direction.x = 0;
        else if((pl.GetInputX() < 0) && (direction.x > 0)) direction.x = 0;
        else if (Mathf.Approximately(pl.GetInputX(), 0))
        {
            pl.SetSpriteDirection(direction.x > 0);
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
        pl.Animator.Attack();
        normalAttackCol.SetActive(true);

        AudioClip sound = attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)];
        pl.PlaySE(sound);

        interval_dt = 0;
        charge_dTime = 0;
    }

    void ChargeSlash(Vector2 start, Vector2 end)
    {
        //以前はmp不足だと斬撃を放てなかったが、
        //仕様変更によりmp不足の場合にhpで肩代わりすることになった
        //if (!config.easyMode && (stat.mp < slashMP)) return;

        if (!config.easyMode) pl.Status.MP_damage(slashMP);

        var effect = Instantiate(slashEffect);
        effect.GetComponent<SlashEffect>().SetData_charge(start, end);

        pl.PlaySE(audioBind.player.chargeSlash);
        pl.Animator.Slash();

        interval_dt = 0;
    }
}
