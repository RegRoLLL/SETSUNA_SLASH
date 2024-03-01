using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController_main : SetsunaSlashScript
{
    public Game_HubScript hub;
    public AudioSource seAS;
    public PL_Status stat;

    public PlayerCollisionChecker playerCC;
    public Vector2 standColSize, crouchColSize;

    public GameObject spritePivot, soul;
    public float moveSpd, crouchMoveSpd, sprintMoveSpd, jumpPow, wallKickPow, airResistance;
    public float wallKickAngle;
    public float defaultGravityScale, fallGravityScale, kosuriGravityScale;
    public bool jumped, isWallKosuri, isCrouch, isSprint, isDead;
    public (bool r, bool l) wallKicked;

    public state_pose stateP;
    public state_move stateM;

    public float inputVecX;

    [SerializeField] Image stickL, backL;

    [HideInInspector]public Rigidbody2D rb;

    [HideInInspector]public PlayerInput input;

    public enum state_pose
    {
        stand, crouch, teleport
    }

    public enum state_move
    {
        idle, walk, run, jump, fall
    }

    void Start()
    {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        jumped = false;
        isDead = false;

        stateP = state_pose.stand;
        stateM = state_move.idle;

        stat = GetComponent<PL_Status>();
    }

    void Update()
    {
        if (input.actions[hub.action.pose].WasPressedThisFrame()) hub.gm.PoseMenu();

        if (Time.timeScale == 0) return;

        if (isDead) return;

        if (stateP == state_pose.teleport)
        {
            ColliderSet();
        }
        else
        {
            Move();
        }

        if ((jumped || wallKicked.r || wallKicked.l) && rb.velocity.y <= 0 && playerCC.IsGrounded()) Chakuchi();

        spritePivot.SetActive(stateP != state_pose.teleport);
        soul.SetActive(stateP == state_pose.teleport);

        GravitySet();
    }

    void Move()
    {
        inputVecX = input.currentActionMap[hub.action.move].ReadValue<float>();

        //Debug.Log(inputVecX);

        var inputJump = input.actions[hub.action.jump].WasPressedThisFrame();
        var inputSprint = input.actions[hub.action.sprint];

        if (inputSprint.WasPressedThisFrame())
        {
            isSprint = true;
        }
        else if (inputSprint.WasReleasedThisFrame() && inputVecX==0)
        {
            isSprint = false;
        }
        else if(isSprint && !inputSprint.IsPressed() && inputVecX==0)
        {
            isSprint = false;
        }

        isCrouch = isSprint ? false : input.actions[hub.action.crouch].IsPressed();

        float spd = moveSpd;
        if (isSprint) spd = sprintMoveSpd;
        if (isCrouch) spd = crouchMoveSpd;

        Vector2 vec = rb.velocity;

        bool ground = playerCC.IsGrounded(),
             wallR = playerCC.IsWall(true),
             wallL = playerCC.IsWall(false);

        Vector2 angleVec = playerCC.GetGroundNormal(true);
        Vector2 kickVec;

        if(wallR || wallL)
        {
            kickVec = Quaternion.Euler(0, 0, wallKickAngle * (wallL ? 1f : -1f)) * playerCC.GetWallNormal(inputVecX > 0);
        }
        else
        {
            kickVec = Quaternion.Euler(0, 0, (90 - (wallKickAngle/3)) * (wallL ? 1f : -1f)) * playerCC.GetWallNormal(inputVecX > 0);
        }


        bool kosuri = false;


        if (ground && !jumped && !wallKicked.r && !wallKicked.l)
        {
            vec = rb.velocity;
            vec.x = 0;
            if (!(rb.velocity.x == 0 && rb.velocity.y != 0))
            {
                if (ground)
                {
                    vec.y = 0;
                    //Debug.Log("yVelocity reset");
                }
            }

            if ((inputVecX > 0) && !wallR) vec += angleVec * spd;
            if ((inputVecX < 0) && !wallL) vec -= angleVec * spd;

            if (inputJump)
            {
                vec.y = jumpPow;

                jumped = true;
            }
        }
        else if(!ground) //‹ó’†
        {
            if (vec.x != 0) vec.x -= vec.normalized.x * airResistance * Time.deltaTime;

            float delta = (spd - airResistance) * 2 * Time.deltaTime;

            if ((inputVecX > 0) && !wallR && (vec.x < spd + delta)) vec.x += delta;
            if ((inputVecX < 0) && !wallL && (vec.x > -spd - delta)) vec.x -= delta;

            if ((wallR || wallL) && rb.velocity.y < 0)
            {
                //Debug.Log($"wallSide{(wallR ? "_R" : "")}{(wallL ? "_L" : "")}");

                if (inputJump)
                {
                    vec = kickVec * wallKickPow;
                    (wallR ? ref wallKicked.r : ref wallKicked.l) = true;

                    //Debug.Log($"wallKick!: {kickVec}");
                    Debug.DrawRay(transform.position, kickVec, Color.white, 1);
                }
                else if(!wallKicked.r && wallR && (inputVecX == 1))
                {
                    Debug.Log("kosuri_R");
                    wallKicked.l = false;
                    kosuri = true;
                }
                else if (!wallKicked.l && wallL && (inputVecX == -1))
                {
                    Debug.Log("kosuri_L");
                    wallKicked.r = false;
                    kosuri = true;
                }
            }
        }

        isWallKosuri = kosuri;

        rb.velocity = vec;

        if (inputVecX > 0) spritePivot.transform.eulerAngles = Vector2.zero;
        else if (inputVecX < 0) spritePivot.transform.eulerAngles = Vector2.up * 180;

        StateSet(vec);
    }

    void StateSet(Vector2 inputVec)
    {
        if (inputVec.x == 0)
        {
            stateM = state_move.idle;
        }
        else
        {
            stateM = isSprint ? state_move.run : state_move.walk;
        }

        if (jumped) stateM = state_move.jump;
        else if (!playerCC.IsGrounded()) stateM = state_move.fall;

        stateP = (playerCC.IsGrounded() && isCrouch) ? state_pose.crouch : state_pose.stand;

        ColliderSet();
    }

    void ColliderSet()
    {
        if (input.actions[hub.action.crouch].WasPressedThisFrame())
        {
            playerCC.SizeChange(crouchColSize.x, crouchColSize.y);
        }
        else if (input.actions[hub.action.crouch].WasReleasedThisFrame())
        {
            playerCC.SizeChange(standColSize.x, standColSize.y);
        }
    }



    public void Chakuchi()
    {
        jumped = false;
        wallKicked.r = false;
        wallKicked.l = false;
    }

    void GravitySet()
    {
        if (stateP != state_pose.teleport)
        {
            bool falling = (rb.velocity.y < 0);
            float scale = defaultGravityScale;

            if (falling) scale = fallGravityScale;

            if (isWallKosuri)
            {
                var vec = rb.velocity;
                vec.y = -kosuriGravityScale;
                rb.velocity = vec;
            }

            rb.gravityScale = scale;
        }
        else
        {
            rb.gravityScale = 0;
        }
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == deathFallAreaLayer)hub.gm.LoadSave();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<StageStat>())
        {
            var stage = other.gameObject.GetComponent<StageStat>();

            if (hub.currentPart != stage) hub.SetPart(stage);
        }
    }

    public void OnReset(InputValue value)
    {
        Debug.Log("callback called.");
        hub.gm.LoadSave();
    }

    public void SetStickBack()
    {
        backL.transform.position = stickL.transform.position;
    }
}
