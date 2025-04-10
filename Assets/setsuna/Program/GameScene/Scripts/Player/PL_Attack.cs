using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using RegUtil;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;

public class PL_Attack : SetsunaSlashScript
{
    [SerializeField] PlayerInputActionName plAction;

    public float interval, interval_dt;
    [SerializeField] SlashControllMode controllMode;
    [SerializeField] float cameraLerpTime;

    [Space(10)]
    public float dragMultiplier;
    public float pointChargeSPD;
    float pointDirRatio;

    [Space(10)]
    public Vector2 dragCamDirMultiplier;
    public Vector2 pointerCamDirMultiplier;
    [SerializeField] Vector2 camMoveCrit_close, camMoveCrit_far;

    [Space(10)]
    public float chargeSlowTimeRatio;
    public float chargeAudioSmallRatio;

    public float chargeStartTime, charge_dTime;
    public GameObject normalAttackCol;

    [SerializeField] LineRenderer line;
    [SerializeField] GameObject slashEffectPrefab;

    [SerializeField] List<AudioClip> attackSounds = new();

    Player pl;

    PlayerInput input;
    InputAction attackAction, chargeCancel;
    Vector2 startPos, endPos, direction;
    bool isCharging;

    Vector2 beforeChargeCameraDirection;

    enum SlashControllMode { drag, pointer }

    void Start()
    {
        pl = GetComponent<Player>();

        charge_dTime = 0;
        input = GetComponent<PlayerInput>();
        attackAction = input.actions[plAction.attack];
        chargeCancel = input.actions[plAction.chargeCancel];
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        //以下、非ポーズ時のみ実行

        if (interval_dt < interval)
        {
            interval_dt += Time.deltaTime;
            return;
        }

        //以下、攻撃インターバル終了後のみ実行

        if (isCharging && config.slowWhenSlashCharge)
        {
            RegTimeKeeper.MultipleTimeScale(ratio: chargeSlowTimeRatio);
            SetsunaAudioKeeper.MultipleAudioScale(ratio: chargeAudioSmallRatio);
        }

        if (WasCanceledThisFlame()) return;

        //以下、このフレームでキャンセルされていない場合のみ実行

        bool isGamepad = (config.controllMode == ConfigDatas.ControllMode.gamepad);

        //新式
        if (isGamepad || controllMode == SlashControllMode.pointer)
        {
            PointerMethod();
        }
        else
        {
            DragMethod();
        }

        SetSpriteAndCameraDir();
        SetLine();
    }

    bool WasCanceledThisFlame()
    {
        if (!isCharging && !pl.GetIsDead()) return false;

        var result = false;

        if (pl.GetIsDead()) result = true;

        if (chargeCancel.WasPressedThisFrame()) result = true;

        if (pl.GetStatePose() == PlayerController_main.state_pose.crouch) result = true;
        if (pl.GetStatePose() == PlayerController_main.state_pose.teleport) result = true;

        if (result)
        {
            isCharging = false;
            charge_dTime = 0;
        }

        pl.SetCameraDistanceSetMode(true);

        return result;
    }

    void DragMethod()
    {
        //開始
        if (attackAction.WasPressedThisFrame())
        {
            startPos = pl.GetCursorPos(PlayerCursor.VectorSpace.screen);
            isCharging = true;
        }

        //長押し中
        if (isCharging)
        {
            charge_dTime += Time.unscaledDeltaTime;
            endPos = pl.GetCursorPos(PlayerCursor.VectorSpace.screen);
            direction = (startPos - endPos) * dragMultiplier;
        }

        //終了
        if (attackAction.WasReleasedThisFrame())
        {
            isCharging = false;

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

    void PointerMethod()
    {
        //開始
        if (attackAction.WasPressedThisFrame())
        {
            isCharging = true;
            pointDirRatio = 0;
        }

        //長押し中
        if (isCharging)
        {
            charge_dTime += Time.unscaledDeltaTime;
            startPos = transform.position;
            endPos = pl.GetCursorPos(PlayerCursor.VectorSpace.world);
            pointDirRatio += pointChargeSPD * Time.unscaledDeltaTime;
            direction = (endPos - startPos);

            if (direction.magnitude > pointDirRatio)
            {
                direction = direction.normalized * pointDirRatio;
            }
        }

        //終了
        if (attackAction.WasReleasedThisFrame())
        {
            isCharging = false;

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

    void SetSpriteAndCameraDir()
    {
        if (charge_dTime >= chargeStartTime)
        {
            SetCameraDirMethod(
                controllMode switch { 
                    SlashControllMode.pointer => GetCameraDirSetPhase(),
                    SlashControllMode.drag => 2,
                    _ => 2
                }
            );

            if ((pl.GetInputX() > 0) && (direction.x < 0)) direction.x = 0;
            else if((pl.GetInputX() < 0) && (direction.x > 0)) direction.x = 0;
            else if (Mathf.Approximately(pl.GetInputX(), 0))
            {
                pl.SetSpriteDirection(direction.x > 0);
            }
        }
        else
        {
            beforeChargeCameraDirection = pl.GetCameraDirection();
        }
    }
    
    /// <returns>0:close 1:mid 2:far</returns>
    int GetCameraDirSetPhase()
    {
        var curPos = pl.GetCursorPos(PlayerCursor.VectorSpace.world);
        var plPos = pl.transform.position;

        if (IsCursorInRect(camMoveCrit_close, curPos)) return 0;
        if (!IsCursorInRect(camMoveCrit_far, curPos)) return 2;
        return 1;
    }
    bool IsCursorInRect(Vector2 crit, Vector2 curPos)
    {
        Vector2 center = pl.cam.transform.position;
        var dir = curPos - center;

        if (Mathf.Abs(dir.x) > crit.x) return false;
        if (Mathf.Abs(dir.y) > crit.y) return false;
        return true;
    }
    /// <param name="phase">0:close 1:mid 2:far</param>
    void SetCameraDirMethod(int phase)
    {
        if (phase == 0)
        {
            //プレイヤーに寄っていく
            pl.SetCameraDistanceSetMode(fromAverage: true);
            pl.cam.AddList(Vector2.zero);
            return;
        }

        pl.SetCameraDistanceSetMode(fromAverage: false);

        //なにもしない
        if (phase == 1)
        {
            charge_dTime = chargeStartTime;
            beforeChargeCameraDirection = pl.GetCameraDirection();
            return;
        }

        //ここからphase2(通常動作)

        Vector2 cameraDir;

        static Vector2 Multiple(Vector2 vec1, Vector2 vec2) => new(vec1.x * vec2.x, vec1.y * vec2.y);

        cameraDir = controllMode switch
        {
            SlashControllMode.pointer => Multiple((endPos - startPos), pointerCamDirMultiplier),
            SlashControllMode.drag => Multiple(direction, dragCamDirMultiplier),
            _ => direction
        };

        var lerp = (charge_dTime - chargeStartTime) / cameraLerpTime;

        //カメラがテレポートしないようにする処理
        if (lerp <= 1f)
        {
            cameraDir = Vector2.Lerp(beforeChargeCameraDirection, cameraDir, lerp);
        }

        pl.SetCameraDirection(cameraDir);
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
        var result = pl.Status.TryConsumeCount();
        if (!result)
        {
            Attack();
            return;
        }

        var effect = Instantiate(slashEffectPrefab);
        effect.GetComponent<SlashEffect>().SetData(start, end);

        pl.PlaySE(audioBind.player.chargeSlash);
        pl.Animator.Slash();

        interval_dt = 0;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLineStrip(Vec2ToRectAngle(camMoveCrit_far), true);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLineStrip(Vec2ToRectAngle(camMoveCrit_close), true);
    }
    Vector3[] Vec2ToRectAngle(Vector2 vec)
    {
        var center = Camera.main.transform.position;

        return new Vector3[]{
            center + Vector3.right * vec.x + Vector3.up * vec.y,
            center + Vector3.right * vec.x + Vector3.down * vec.y,
            center + Vector3.left * vec.x + Vector3.down * vec.y,
            center + Vector3.left * vec.x + Vector3.up * vec.y
        };
    }
}
