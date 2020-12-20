using System;
using System.Collections;
using System.Collections.Generic;
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

    public bool IsJump;
    public bool IsDash;
    public bool Ducking;
    public bool OnGround;
    public bool OnMoving;
    public bool IsRunning;
    public int Facing;
    public float JumpGraceTimer;
    public Vector2 Speed;
    public Vector2 Direction;

    private void WalkToRun(float directionX)
    {
        if (directionX != 0 && directionX == Facing)    //Todo
        {
            float mult = OnGround ? GroundMult : AirMult;
            Speed.x = Mathf.MoveTowards(Speed.x, (IsRunning ? MaxRun : MaxWalk) * directionX, RunAccel * mult * Time.deltaTime);   //Approach the max speed
        }
        else
        {
            Speed.x = Mathf.MoveTowards(Speed.x, 0, RunAccel * 2.5f * Time.deltaTime);
        }
    }

    private void JumpToFall(Vector2 direction)
    {
        if (OnGround)
            JumpGraceTimer = JumpGraceTime;
        else if (JumpGraceTimer > 0)
            JumpGraceTimer -= Time.deltaTime;
        if (JumpGraceTimer > 0)
        {
            if (IsJump)
            {
                Speed.x = JumpHBoost * direction.x;
                Speed.y = JumpSpeed;
            }
            else
                JumpGraceTimer = 0;
        }
        else
        {
            float mult = (Mathf.Abs(Speed.y) < HalfGravThreshold) ? 0.5f : 1.0f;
            Speed.y = Mathf.MoveTowards(Speed.y, 0, Gravity * mult * Time.deltaTime);
        }
        if (!OnGround)
        {
            Speed.x = Mathf.MoveTowards(Speed.x, MaxRun * .6f * direction.x, RunAccel * .5f * AirMult * Time.deltaTime);
            //Speed.y = Mathf.MoveTowards(Speed.y, MaxFall * 2, Gravity * 0.25f * Time.deltaTime);
        }

    }

    private void ApplyGravity()
    {
        if (!OnGround)
        {
            float mult = (Mathf.Abs(Speed.y) < HalfGravThreshold && IsJump) ? 0.5f : 1.0f;
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
        IsRunning = Input.GetKey(KeyCode.LeftShift);
        IsJump = Input.GetKeyDown(KeyCode.Space);
        var startPos = transform.position + Vector3.down * 0.45f;
        var endPos = startPos + Vector3.down * 0.07f;
        OnMoving = Direction != Vector2.zero;
        OnGround = Physics2D.Raycast(startPos, Vector2.down, 0.07f, LayerMask.NameToLayer("Player"));
        Debug.DrawLine(startPos, endPos, Color.blue);
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        WalkToRun(Direction.x);
        JumpToFall(Direction);
        //ApplyGravity();
        //rigid.velocity = Speed;
        transform.position += new Vector3(Speed.x, Speed.y, 0) * Time.deltaTime;
    }

}
