using System;
using UnityEngine;

public class Movement : MonoBehaviour
{

    private const float AirMult = .65f;
    private const float Gravity = 90.0f;
    private const float MaxRun = 9.0f;
    private const float MaxFall = -16.0f;
    private const float RunAccel = 100.0f;
    private const float RunReduce = 40.0f;
    private const float HalfGravity = 4.0f;

    private const float HoldingMaxRun = 7.0f;

    public const float JumpSpeed = 10.5f;
    public const float JumpHBoost = 4.0f;
    public const float VarJumpTime = .2f;
    public const float JumpGraceTime = 0.1f;

    private const float LiftYCap = -13.0f;
    private const float LiftXCap = 25.0f;


    public bool IsJump;
    public bool IsDash;
    public bool OnGround;

    public bool IsDucking;
    public bool IsHolding;

    public float moveX;
    public Vector2 Speed;
    public Vector2 Direction;
    private Vector2 LiftSpeed;


    private Vector2 LiftBoost
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

    private void CRunning()
    {
        if (IsDucking && OnGround)
        {
        }
        else
        {
            float mult = OnGround ? 1f : AirMult;
            float max = IsHolding ? 6f : MaxRun;
            if (Mathf.Abs(Speed.x) > max && Mathf.Sign(Speed.x) == moveX)
                Speed.x = Mathf.MoveTowards(Speed.x, max * moveX, RunReduce * mult * Time.deltaTime);
            else
                Speed.x = Mathf.MoveTowards(Speed.x, max * moveX, RunAccel * mult * Time.deltaTime);
            if (Mathf.Abs(Speed.x) > 0.0001f)
            {
                Debug.Log("speedX " + Speed.x);
            }
        }

    }

    private void CGravity()
    {
        if (!OnGround)
        {
            float max = MaxFall;
            //Wall Slide
            float mult = (Mathf.Abs(Speed.y) < HalfGravity) ? 0.5f : 1f;
            Speed.y = Mathf.MoveTowards(Speed.y, 0, Gravity * mult * Time.deltaTime);
        }
    }

    private void CJumping()
    {
        LiftSpeed = Vector2.up * Math.Max(Speed.y, 8.0f);
        if (IsJump)
        {
            Speed.x += JumpHBoost * moveX;
            Speed.y = JumpSpeed;
            Speed += LiftBoost;
        }
    }

    private void HandleInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        Direction.y = Input.GetAxisRaw("Vertical");
        IsJump = Input.GetKeyDown(KeyCode.Space);
        IsDucking = Input.GetKeyDown(KeyCode.Q);
        OnGround = IsOnGround(transform.position, Vector3.down, 0.25f);
        DrawLine(transform.position, Vector3.down * 0.25f, Color.red);
    }

    private bool IsOnGround(Vector3 player, Vector3 direction, float distance)
    {
        return Physics2D.Raycast(player, direction, distance, LayerMask.GetMask("Platform"));
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        Debug.DrawRay(start, end, color);
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        HandleInput();
        CRunning();
        CGravity();
        CJumping();
        transform.Translate(Speed * Time.deltaTime);
    }

    private void FixedUpdate()
    {

        //JumpToFall(Direction);
        //var deltaMovement = Speed * Time.deltaTime;

        //transform.position += new Vector3(deltaMovement.x, deltaMovement.y, 0);
    }

}
