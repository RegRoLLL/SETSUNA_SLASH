using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : SetsunaSlashScript
{
    [SerializeField] PlayerCursor cursor;
    public SetsunaPlayerCamera cam;
    public Game_SetsunaUI ui;

    PlayerController_main ctrler;
    PL_Attack attack;
    PlayerCollisionChecker colChecker;
    PL_Status status;
    PlayerAnimationCaller anim;
    PL_Dead dead;

    public Rigidbody2D Rigidbody {  get; private set; }
    public Collider2D Collider { get => colChecker.col; }

    GameManager gm;

    void Start()
    {
        gm = EventSystem.current.GetComponent<GameManager>();

        ctrler = GetComponent<PlayerController_main>();
        attack = GetComponent<PL_Attack>();
        colChecker = GetComponentInChildren<PlayerCollisionChecker>();
        status = GetComponent<PL_Status>();
        anim = GetComponent<PlayerAnimationCaller>();
        dead = GetComponent<PL_Dead>();

        Rigidbody = GetComponent<Rigidbody2D>();
    }

    //cursor
    public Vector2 GetCursorPos(PlayerCursor.VectorSpace space)
    {
        return cursor.GetCursorPos(space);
    }

    //ctrler
    public bool GetIsDead() => ctrler.isDead;
    public PlayerController_main.state_move GetStateMove() => ctrler.stateM;
    public PlayerController_main.state_pose GetStatePose() => ctrler.stateP;
    public void SetSpriteDirection(bool right)
    {
        ctrler.spritePivot.transform.eulerAngles = right ? Vector2.zero : Vector2.up * 180;
    }
    public float GetInputX() => ctrler.inputVecX;
    public void PlaySE(AudioClip clip)
    {
        ctrler.seAS.PlayOneShot(clip);
    }

    //cam
    public void SetCameraDistanceSetMode(bool fromAverage)
    {
        cam.setDistanceFromAverage = fromAverage;
    }
    public void SetCameraDirection(Vector2 direction)
    {
        cam.directionToPL = direction;  
    }
    public Vector2 GetCameraDirection() => cam.directionToPL;

    //anim
    public PlayerAnimationCaller Animator{ get=>anim; }

    //status
    public PL_Status Status { get => status; }

    //colChecker
    public List<Collision2D> GetCollisions() => colChecker.GetCollisions();
    public List<Collider2D> GetTriggers() => colChecker.GetTriggers();
    public bool IsGounded() => colChecker.IsGrounded();


    public IEnumerator ReturnPlayerPos()
    {
        gm.isSaving = true;

        ctrler.stateP = PlayerController_main.state_pose.teleport;

        float dTime = 0, ratio;
        Vector3 beforeTeleportPos = ctrler.transform.position;
        while (dTime <= gm.loadTeleportTime)
        {
            ratio = dTime / gm.loadTeleportTime;

            ctrler.transform.position = Vector3.Lerp(beforeTeleportPos, gm.hub.playingStage.savedPlayerPosition, ratio);

            dTime += Time.deltaTime;

            yield return null;
        }
        ctrler.transform.position = gm.hub.playingStage.savedPlayerPosition;
        ctrler.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        cam.ResetList();

        ctrler.stateP = PlayerController_main.state_pose.stand;

        gm.hub.playingStage.Load();

        yield return StartCoroutine(ui.Flash());

        gm.isSaving = false;
    }
}
