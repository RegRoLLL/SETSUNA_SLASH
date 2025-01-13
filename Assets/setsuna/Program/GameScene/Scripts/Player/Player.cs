using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SetsunaSlashScript
{
    [SerializeField] PlayerCursor cursor;
    [SerializeField] SetsunaPlayerCamera cam;

    PlayerController_main ctrler;
    PL_Attack attack;
    PlayerCollisionChecker colChecker;
    PL_Status status;
    PlayerAnimationCaller anim;
    PL_Dead dead;

    public Rigidbody2D Rigidbody {  get; private set; }
    public Collider2D Collider { get => colChecker.col; }

    void Start()
    {
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
}
