using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : MonoBehaviour
{

    public const float WalkSpeed = 6.4f;
    public const float MaxRun = 9.0f;
    public const float RunAccel = 10f;
    private const float RunReduce = 4f;
    public const float MaxFall = 160f;
    private const float Gravity = 900f;
    private const float HalfGravThreshold = 40f;

    public int Facing;
    public Vector2 Speed;
    public bool OnGround;
    public bool IsJump;
    private float jumpGraceTimer;
    public bool AutoJump;
    public float AutoJumpTimer;
    private float varJumpSpeed;
    private float varJumpTimer;

    public bool DummyMoving = false;
    public bool DummyGravity = true;
    public bool DummyFriction = true;
    public bool DummyMaxspeed = true;

    private void LaunchUpdate()
    {
        // Decceleration
        if (Speed.y < 0)
        {
            Speed.y = Mathf.MoveTowards(Speed.y, MaxFall, Gravity * 0.5f * Time.deltaTime);
        }
        else
        {
            Speed.y = Mathf.MoveTowards(Speed.x, 0, RunAccel * 0.2f * Time.deltaTime);
        }
        Speed.x = Mathf.MoveTowards(Speed.x, 0, RunAccel * 0.2f * Time.deltaTime);
    }

    private void DummyUpdate()
    {
        // gravity
        if (!OnGround)
        {
            float mult = Mathf.Abs(Speed.y) < HalfGravThreshold && IsJump ? 0.5f : 1f;
            Speed.y = Mathf.MoveTowards(Speed.y, MaxFall, Gravity * mult * Time.deltaTime);
        }
        Speed.x = Mathf.MoveTowards(Speed.x, 0, RunAccel * Time.deltaTime);
    }

    private void DummyWalkTo(float x, bool walkBackwards = false, float speedMultiplier = 1f, bool keepWalkingIntoWalls = false)
    {
        Speed.x = Mathf.MoveTowards(Speed.x, Math.Sign(x) * WalkSpeed * speedMultiplier, RunAccel * Time.deltaTime);
    }

    private void DummyRunTo(float x, bool fastAnim = false)
    {
        Speed.x = Mathf.MoveTowards(Speed.x, Mathf.Sign(x) * MaxRun, RunAccel * Time.deltaTime);
    }


}
