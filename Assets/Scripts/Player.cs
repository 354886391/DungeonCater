﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public const float MaxFall = -16.0f;
    public const float Gravity = 90.0f;
    public const float HalfGravThreshold = 4.0f;

    public const float FastMaxFall = -24.0f;
    public const float FastMaxAccel = 30.0f;

    public const float MaxRun = 9.0f;
    public const float RunAccel = 100.0f;
    public const float RunReduce = 40.0f;
    public const float AirMult = .65f;

    public const float HoldingMaxRun = 7.0f;
    public const float HoldMinTime = .35f;

    public const float BounceAutoJumpTime = .1f;

    public const float DuckFriction = 50.0f;
    public const int DuckCorrectCheck = 4;
    public const float DuckCorrectSlide = 5.0f;

    public const float DodgeSlideSpeedMult = 1.2f;
    public const float DuckSuperJumpXMult = 1.25f;
    public const float DuckSuperJumpYMult = .5f;

    public const float JumpGraceTime = 0.1f;
    public const float JumpSpeed = -10.5f;
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

    public const float WalkSpeed = 6.4f;



    public bool IsJump;
    public bool IsDash;
    public bool Ducking;
    public bool OnGround;
    public int Facing;
    public Vector2 Speed;
    public Vector2 Direction;

    private void DummyWalkTo(float x, float speedMultiplier = 1f)
    {
        Speed.x = Mathf.MoveTowards(Speed.x, Facing * WalkSpeed * speedMultiplier, RunAccel * Time.deltaTime);
    }

    private void DummyRunTo(float moveX)
    {
        //Running and Friction
        if (Ducking && OnGround)
            Speed.x = Mathf.MoveTowards(Speed.x, 0, DuckFriction * Time.deltaTime);
        else
        {
            float mult = OnGround ? 1 : AirMult;
            if (Math.Abs(Speed.x) > MaxRun && Facing == moveX)
                Speed.x = Mathf.MoveTowards(Speed.x, MaxRun * moveX, RunReduce * mult * Time.deltaTime);  //Reduce back from beyond the max speed
            else
                Speed.x = Mathf.MoveTowards(Speed.x, MaxRun * moveX, RunAccel * mult * Time.deltaTime);   //Approach the max speed
        }
    }

    private void ApplyGravity()
    {
        // 减速
        if (Speed.y < 0)
            Speed.y = Mathf.MoveTowards(Speed.y, MaxFall, Gravity * .5f * Time.deltaTime);
        else
            Speed.y = Mathf.MoveTowards(Speed.y, MaxFall, Gravity * .25f * Time.deltaTime);
        Speed.x = Mathf.MoveTowards(Speed.x, 0, RunAccel * .2f * Time.deltaTime);

        if (!OnGround)
        {
            float mult = (Mathf.Abs(Speed.y) < HalfGravThreshold && IsJump) ? 0.5f : 1.0f;
            Speed.y = Mathf.MoveTowards(Speed.y, MaxFall, Gravity * mult * Time.deltaTime);
        }
    }

    public Rigidbody2D rigid;

    private void HandleInput()
    {
        Direction.x = Input.GetAxisRaw("Horizontal");
        Direction.y = Input.GetAxisRaw("Vertical");
        OnGround = Physics2D.Raycast(transform.position + Vector3.down * 0.55f, Vector2.down, 0.1f);
        Debug.DrawLine(transform.position + Vector3.down * 0.55f, transform.position + Vector3.down * 0.51f, Color.blue);
    }

    private void Update()
    {
        HandleInput();
        DummyRunTo(Direction.x);
        ApplyGravity();
        //rigid.velocity = Speed;
        transform.position += new Vector3(Speed.x, Speed.y, 0) * Time.deltaTime;
    }
}