using System;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public const float MaxFall = -16.0f;
    public const float Gravity = 9.0f;
    public const float HalfGravThreshold = 4.0f;

    public const float FastMaxFall = -24.0f;
    public const float FastMaxAccel = 30.0f;

    public const float MaxRun = 9.0f;
    public const float RunAccel = 10.0f;
    public const float RunReduce = 4.0f;
    public const float GroundMult = 1.0f;
    public const float AirMult = .65f;  //空中横向移动系数

    public const float HoldingMaxRun = 7.0f;
    public const float HoldMinTime = .35f;

    public const float BounceAutoJumpTime = .1f;

    public const float DuckFriction = 5.0f;
    public const int DuckCorrectCheck = 4;
    public const float DuckCorrectSlide = 5.0f;

    public const float DodgeSlideSpeedMult = 1.2f;
    public const float DuckSuperJumpXMult = 1.25f;
    public const float DuckSuperJumpYMult = .5f;

    public const float JumpGraceTime = 0.1f;
    public const float JumpSpeed = 10.5f;
    public const float JumpHBoost = 4.0f;
    public const float VarJumpTime = .2f;
    public const float CeilingVarJumpGrace = .05f;

    public const float DashSpeed = 24.0f;
    public const float EndDashSpeed = 16.0f;
    public const float EndDashUpMult = .75f;
    public const float DashTime = .15f;

    public const float BoostMoveSpeed = 8.0f;
    public const float BoostTime = .25f;

    public const float DuckWindMult = 0f;
    public const int WindWallDistance = 3;

    public const float ReflectBoundSpeed = 22.0f;

    public const float LaunchSpeed = 28.0f;
    public const float LaunchCancelThreshold = 22.0f;

    public const float MaxWalk = 6.4f;


    private const float LiftYCap = -13.0f;
    private const float LiftXCap = 25.0f;
    private Vector2 LiftSpeed = new Vector2(30.0f, -15.0f);

    private const float SwimYSpeedMult = .5f;
    private const float SwimMaxRise = -6.0f;
    private const float SwimVDeccel = 60.0f;
    private const float SwimMax = 8.0f;
    private const float SwimUnderwaterMax = 6.0f;
    private const float SwimAccel = 60.0f;
    private const float SwimReduce = 40.0f;
    private const float SwimDashSpeedMult = .75f;

    private bool isJump;
    private bool isDash;
    private bool onGround;
    private bool onMoving;
    private bool isDucking;
    private bool isRunning;
    private bool isUnderwater;
    private int facing;
    private Vector2 Speed;
    private Vector2 Direction;
    private float jumpGraceTimer;
    private float dashAttackTimer;
    private float wallSlideTimer;
    private float wallBoostTimer;
    public float WallSlideTime { get; private set; }

    public Vector2 LiftBoost
    {
        get
        {
            Vector2 val = LiftSpeed;

            if (Math.Abs(val.x) > LiftXCap)
                val.x = LiftXCap * Math.Sign(val.x);

            if (val.y > 0)
                val.y = 0;
            else if (val.y < LiftYCap)
                val.y = LiftYCap;

            return val;
        }
    }


    private void WalkToRun(Vector2 move)
    {
        float maxX = isUnderwater ? SwimUnderwaterMax : SwimMax;
        float maxY = SwimMax;
        if (Mathf.Abs(Speed.x) > SwimMax && Mathf.Sign(Speed.x) == Mathf.Sign(move.x))
        {
            Speed.x = Mathf.MoveTowards(Speed.x, maxX * move.x, SwimReduce * Time.deltaTime);
        }
        else
        {
            Speed.x = Mathf.MoveTowards(Speed.x, maxX * move.x, SwimAccel * Time.deltaTime);
        }

        if (move.y == 0 && !onGround)
        {
            Speed.y = Mathf.MoveTowards(Speed.y, SwimMaxRise, SwimAccel * Time.deltaTime);
        }
        //else if (move.y >= 0 && onGround)
        //{

        //    float mult = (Mathf.Abs(Speed.y) < HalfGravThreshold) ? 0.5f : 1.0f;
        //    Speed.y = Mathf.MoveTowards(Speed.y, 0, Gravity * mult * Time.deltaTime);
        //Speed.x = Mathf.MoveTowards(Speed.x, SwimMax * .6f * move.x, SwimAccel * .5f * AirMult * Time.deltaTime);

        //if (Mathf.Abs(Speed.y) > SwimMax && Mathf.Sign(Speed.y) == Mathf.Sign(move.y))
        //{
        //    Speed.y = Mathf.MoveTowards(Speed.y, maxY * move.y, RunReduce * Time.deltaTime);
        //}
        //else
        //{
        //    Speed.y = Mathf.MoveTowards(Speed.y, maxY * move.y, RunAccel * Time.deltaTime);
        //}
        //}
        //if (onGround)
        //{
        //    jumpGraceTimer = JumpGraceTime;
        //}
        //else if (jumpGraceTimer > 0)
        //    jumpGraceTimer -= Time.deltaTime;

        //if (jumpGraceTimer > 0)
        //{
        //    if (isJump)
        //    {
        //        Jump(move);
        //    }
        //}
    }

    public void Jump(Vector2 move)
    {
        jumpGraceTimer = 0;
        Speed.x += JumpHBoost * move.x;
        Speed.y = JumpSpeed;

    }

    private void JumpToFall(Vector2 direction)
    {
        if (onGround)
        {
            if (isJump)
            {
                Speed.y = JumpSpeed;
                Speed.x = JumpHBoost * direction.x;
            }
        }
        else
        {
            float mult = (Mathf.Abs(Speed.y) < HalfGravThreshold) ? 0.5f : 1.0f;
            Speed.y = Mathf.MoveTowards(Speed.y, 0, Gravity * mult * Time.deltaTime);
            Speed.x = Mathf.MoveTowards(Speed.x, SwimMax * .6f * direction.x, Gravity * .5f * AirMult * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (!onGround)
        {
            float mult = (Mathf.Abs(Speed.y) < HalfGravThreshold && isJump) ? 0.5f : 1.0f;
            Speed.y = Mathf.MoveTowards(Speed.y, MaxFall, Gravity * mult * Time.deltaTime);
        }
        else
        {
            Speed.y = 0;
        }
    }

    private void HandleInput()
    {
        Direction.x = Input.GetAxisRaw("Horizontal");
        Direction.y = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);
        isJump = Input.GetKeyDown(KeyCode.Space);
        isDucking = Input.GetKeyDown(KeyCode.Q);
        var startPos = transform.position + Vector3.down * 0.45f;
        var endPos = startPos + Vector3.down * 0.07f;
        onMoving = Direction != Vector2.zero;
        onGround = Physics2D.Raycast(startPos, Vector2.down, 0.07f, LayerMask.NameToLayer("Player"));
        Debug.DrawLine(startPos, endPos, Color.blue);
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        WalkToRun(Direction);
        JumpToFall(Direction);
        //ApplyGravity();
        transform.position += new Vector3(Speed.x, Speed.y, 0) * Time.deltaTime;
    }

}
