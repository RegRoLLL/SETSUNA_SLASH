using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(1)]
public class Player : SetsunaSlashScript
{
    [SerializeField] PlayerCursor cursor;
    public SetsunaPlayerCamera cam;
    public Game_SetsunaUI ui;

    [HideInInspector] public PlayerController_main ctrler;
    PL_Attack attack;
    PlayerCollisionChecker colChecker;
    PL_Status status;
    PlayerAnimationCaller anim;
    PL_Dead dead;

    public Rigidbody2D Rigidbody {  get; private set; }
    public Collider2D Collider { get => colChecker.col; }

    GameManager gm;

    void Awake()
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
    public bool SetIsDead(bool dead) => ctrler.isDead = dead;
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

    //dead
    public void Death() => dead.Death();

    //ui
    public void CollectJewel(int index)
    {
        ui.SlashCountUI.jewelCounter.GetJewel(index);
    }

    public void ToggleStopPlayerControll(bool stop)
    {
        ctrler.enabled = !stop;
        attack.enabled = !stop;
    }

    public IEnumerator ReturnPlayerPos()
    {
        gm.isSaving = true;

        ctrler.stateP = PlayerController_main.state_pose.teleport;

        float dTime = 0, ratio;
        Vector3 fromPos = ctrler.transform.position;
        Vector3 toPos;
        if(gm.hub.playingStage.anotherPartSave is var anotherSave and not null){
            toPos = anotherSave.transform.position;
        }
        else if (gm.hub.playingStage.latestSavePoint is var lastSave and not null){
            toPos = lastSave.transform.position;
        }
        else{
            toPos = transform.position;
        }
        while (dTime <= gm.loadTeleportTime)
        {
            ratio = dTime / gm.loadTeleportTime;

            ctrler.transform.position = Vector3.Lerp(fromPos, toPos, ratio);

            dTime += Time.deltaTime;

            yield return null;
        }
        ctrler.transform.position = toPos;
        ctrler.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        cam.ResetList();

        ctrler.stateP = PlayerController_main.state_pose.stand;

        gm.hub.playingStage.Load();

        yield return StartCoroutine(ui.Flash(status.ResetCount));

        gm.isSaving = false;
    }
}
