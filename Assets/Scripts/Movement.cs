using System;
using UnityEngine;

public class Movement : MonoBehaviour
{

    private const float AirMult = .5f;
    private const float Gravity = 8.0f;
    private const float MaxRun = 8.0f;
    private const float MaxFall = -12.0f;
    private const float RunAccel = 10.0f;
    private const float RunReduce = 40.0f;
    private const float HalfGravity = 4.0f;

    public const float JumpSpeed = 6.5f;
    public const float JumpHBoost = 3.0f;
    public const float VarJumpTime = .2f;
    public const float JumpGraceTime = 0.1f;

    private int JumpCount;
    private float FirstJumpVBoost = 6.5f;
    private float FirstJumpHBoost = 4.0f;
    private float SecondJumpVBoost = 8.0f;
    private float SecondJumpHBoost = 6.0f;

    public bool IsJump;
    public bool IsDash;
    public bool IsDuck;
    public bool OnGround;

    public Vector2 Speed;
    public Vector2 Direction;


    private void WalkToRun(Vector2 direction)
    {
        if (OnGround)
        {
            float maxX = MaxRun;
            if (direction.x == Mathf.Sign(Speed.x))
            {
                Speed.x = Mathf.MoveTowards(Speed.x, maxX * direction.x, RunAccel * Time.fixedDeltaTime);
            }
            else
            {
                Speed.x = Mathf.MoveTowards(Speed.x, 0, RunReduce * Time.fixedDeltaTime);
            }
        }
    }

    private void JumpToFall(Vector2 direction)
    {
        if (OnGround)
        {
            if (IsJump)
            {
                JumpCount = 1;
                Speed.y = FirstJumpVBoost;
                Speed.x = FirstJumpHBoost * direction.x;
                Debug.Log("FirstJump");
            }
        }
        else
        {
            if (IsJump)
            {
                JumpCount = 2;
                Speed.y += SecondJumpVBoost;
                Speed.x += SecondJumpHBoost * direction.x;
                Debug.Log("SecondJump");
            }
            float maxX = MaxRun;
            float mult = Mathf.Abs(Speed.y) < HalfGravity ? 0.5f : 1.0f;
            Speed.y = Mathf.MoveTowards(Speed.y, 0, Gravity * mult * Time.fixedDeltaTime);
            Speed.x = Mathf.MoveTowards(Speed.x, maxX * direction.x, RunAccel * AirMult * Time.fixedDeltaTime);
        }
    }

    private void HandleInput()
    {
        Direction.x = Input.GetAxisRaw("Horizontal");
        Direction.y = Input.GetAxisRaw("Vertical");
        IsJump = Input.GetKeyDown(KeyCode.Space);
        IsDuck = Input.GetKeyDown(KeyCode.Q);
        var startPos = transform.position + Vector3.down * 0.16f;
        var endPos = startPos + Vector3.down * 0.04f;
        OnGround = IsOnGround(transform.position, Vector3.down, 0.32f);
        DrawLine(transform.position, Vector3.down * 0.32f, Color.red);
    }

    private bool IsOnGround(Vector3 player, Vector3 direction, float distance)
    {
        return Physics2D.Raycast(player, direction, distance, LayerMask.NameToLayer("Platform"));
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
    }

    private void FixedUpdate()
    {
        WalkToRun(Direction);
        JumpToFall(Direction);
        var deltaMovement = Speed * Time.fixedDeltaTime;
        transform.position += new Vector3(deltaMovement.x, deltaMovement.y, 0);
    }

}
