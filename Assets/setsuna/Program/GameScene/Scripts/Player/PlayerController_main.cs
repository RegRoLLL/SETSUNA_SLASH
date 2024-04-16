using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PlayerController_main : SetsunaSlashScript
{
    Game_HubScript hub;
    [SerializeField] PlayerInputActionName plAction;
    public AudioSource seAS;
    public PL_Status stat;

    public PlayerCollisionChecker playerCC;
    public Vector2 standColSize, crouchColSize;

    public GameObject spritePivot, soul;
    public float moveSpd, crouchMoveSpd, sprintMoveSpd, jumpPow, wallKickPow, airResistance;
    public float wallKickAngle;
    public float defaultGravityScale, fallGravityScale, kosuriGravityScale;
    public bool jumped, jumpedIntoAir, isWallKosuri, isCrouch, isSprint, isDead;
    public (bool r, bool l) wallKicked;

    public state_pose stateP;
    public state_move stateM;

    public float inputVecX;

    List<Collider2D> contactCols = new();

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
        hub = EventSystem.current.GetComponent<Game_HubScript>();

        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        jumped = false;
        jumpedIntoAir = false;
        isDead = false;

        stateP = state_pose.stand;
        stateM = state_move.idle;

        stat = GetComponent<PL_Status>();
    }

    void Update()
    {
        if (input.actions[plAction.pose].WasPressedThisFrame()) hub.gm.PoseMenu();

        if (Time.timeScale == 0) return;

        if (isDead) return;

        rb.GetContacts(contactCols);
        contactCols = contactCols.Where((c) => !c.isTrigger).ToList();

        if (stateP == state_pose.teleport)
        {
            ColliderSet();
        }
        else
        {
            Move();
        }

        if (jumped && !playerCC.IsGrounded()) jumpedIntoAir = true;

        if (
            (jumped || wallKicked.r || wallKicked.l)
            && ((rb.velocity.y <= 0) || jumpedIntoAir)
            && (playerCC.IsGrounded() && contactCols.Count > 0)
            ) 
        {
            Chakuchi(); 
        }

        spritePivot.SetActive(stateP != state_pose.teleport);
        soul.SetActive(stateP == state_pose.teleport);

        GravitySet();
    }

    void Move()
    {
        inputVecX = input.currentActionMap[plAction.move].ReadValue<float>();

        //Debug.Log(inputVecX);

        var inputJump = input.actions[plAction.jump].WasPressedThisFrame();
        var inputSprint = input.actions[plAction.sprint];

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

        isCrouch = isSprint ? false : input.actions[plAction.crouch].IsPressed();

        float spd = moveSpd;
        if (isSprint) spd = sprintMoveSpd;
        if (isCrouch) spd = crouchMoveSpd;

        Vector2 vec = rb.velocity;

        bool ground = playerCC.IsGrounded(),
             wallR = playerCC.IsWall(true),
             wallL = playerCC.IsWall(false),
             highSlope = playerCC.IsHighSlope();

        //  Debug.Log($"ground:{ground}  wall(R:{wallR} L:{wallL})  highSlope:{highSlope}");

        Vector2 angleVec = playerCC.GetGroundNormal(true);
        Vector2 wallNormal = playerCC.GetWallNormal(inputVecX > 0);
        Vector2 slopeNormal = playerCC.GetHighSlopeNormal();
        Vector2 kickVec;

        if(wallR || wallL)
        {
            kickVec = Quaternion.Euler(0, 0, wallKickAngle * (wallL ? 1f : -1f)) * wallNormal;
        }
        else
        {
            kickVec = Quaternion.Euler(0, 0, (90 - (wallKickAngle / 4)) * ((inputVecX < 0) ? 1f : -1f)) * slopeNormal;
        }


        bool kosuri = false;


        if (ground && !jumped && !wallKicked.r && !wallKicked.l)
        {
            vec = rb.velocity;

            if (inputVecX != 0)
            {
                var vel = angleVec * spd * ((inputVecX > 0) ? 1 : -1);
                var result = vec + vel;

                if ((vel.x > 0) ? !wallR : !wallL)
                {
                    if ((vel.x > 0) ? (result.x <= vel.x) : (result.x >= vel.x))
                    {
                        vec.x = result.x;
                    }
                    else if ((vel.x > 0) ? (vec.x <= vel.x) : (vec.x >= vel.x))
                    {
                        vec.x = vel.x;
                    }
                }

                if ((vel.y > 0) ? (result.y <= vel.y) : (result.y >= vel.y))
                {
                    vec.y = result.y;
                }
            }
            else if(input.currentActionMap[plAction.move].WasReleasedThisFrame())
            {
                vec *= 0;
            }

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

            if ((wallR || wallL || highSlope) && rb.velocity.y < 0)
            {
                //Debug.Log($"wallSide{(wallR ? "_R" : "")}{(wallL ? "_L" : "")}");

                if (inputJump)
                {
                    if ( !highSlope || ((Vector2.SignedAngle(Vector2.up, slopeNormal) > 0) == (inputVecX > 0)))
                    {
                        vec = kickVec * wallKickPow;
                        ((inputVecX > 0) ? ref wallKicked.r : ref wallKicked.l) = true;

                        //Debug.Log($"wallKick!: {kickVec}");
                        Debug.DrawRay(transform.position, kickVec, Color.white, 1);
                    }
                }
                else if(!wallKicked.r && wallR && (inputVecX == 1))
                {
                    //Debug.Log("kosuri_R");
                    wallKicked.l = false;
                    kosuri = true;
                }
                else if (!wallKicked.l && wallL && (inputVecX == -1))
                {
                    //Debug.Log("kosuri_L");
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
        if (inputVecX == 0)
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
        if (input.actions[plAction.crouch].WasPressedThisFrame())
        {
            playerCC.SizeChange(crouchColSize.x, crouchColSize.y);
        }
        else if (input.actions[plAction.crouch].WasReleasedThisFrame())
        {
            playerCC.SizeChange(standColSize.x, standColSize.y);
        }
    }



    public void Chakuchi()
    {
        jumped = false;
        jumpedIntoAir = false;
        wallKicked.r = false;
        wallKicked.l = false;

        rb.velocity *= 0;
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
            else
            {
                //contactCols.ForEach((c) => Debug.Log(c.name));
                //Debug.Log(contactCols.Count);
                if (playerCC.IsGrounded() && contactCols.Count > 0)
                {
                    scale = 0;
                }
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
