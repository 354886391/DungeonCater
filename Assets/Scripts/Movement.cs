using System;
using UnityEngine;

public class Movement : MonoBehaviour
{

    private const float AirMult = .5f;
    private const float Gravity = 8.0f;
    private const float MaxRun = 8.0f;
    private const float MaxFall = -12.0f;
    private const float RunAccel = 10.0f;
    private const float RunReduce = 20.0f;
    private const float HalfGravity = 4.0f;

    public const float JumpSpeed = 6.5f;
    public const float JumpHBoost = 3.0f;
    public const float VarJumpTime = .2f;
    public const float JumpGraceTime = 0.1f;

    private int jumpCount;
    private float OnceJumpVBoost = 6.5f;
    private float OnceJumpHBoost = 3.0f;
    private float DoubleJumpVBoost = 8.0f;
    private float DoubleJumpHBoost = 5.0f;

    private bool isJump;
    private bool isDash;
    private bool onGround;
    private bool isDucking;
    public Vector2 Speed;
    public Vector2 Direction;


    private void WalkToRun(Vector2 move)
    {
        if (onGround)
        {
            float maxX = MaxRun;
            if (move.x == Mathf.Sign(Speed.x))
            {
                Speed.x = Mathf.MoveTowards(Speed.x, maxX * move.x, RunAccel * Time.deltaTime);
            }
            else
            {
                Speed.x = Mathf.MoveTowards(Speed.x, maxX * move.x, RunReduce * Time.deltaTime);
            }
        }
    }

    private void JumpToFall(Vector2 move)
    {

        if (isJump)
        {
            if (onGround)
            {
                jumpCount = 0;
                Speed.y = OnceJumpVBoost;
                Speed.x = OnceJumpHBoost * move.x;
                Debug.Log("OnceJump");
            }
            else if (jumpCount == 0)
            {
                jumpCount += 1;
                Speed.y += DoubleJumpVBoost;
                Speed.x += DoubleJumpHBoost * move.x;
                Debug.Log("DoubleJump");
            }
        }
        else
        {
            float maxX = MaxRun;
            float mult = Mathf.Abs(Speed.y) < HalfGravity ? 0.5f : 1.0f;
            Speed.y = Mathf.MoveTowards(Speed.y, 0, Gravity * mult * Time.deltaTime);
            Speed.x = Mathf.MoveTowards(Speed.x, maxX * move.x, RunAccel * AirMult * Time.deltaTime);
        }
    }

    private void HandleInput()
    {
        Direction.x = Input.GetAxisRaw("Horizontal");
        Direction.y = Input.GetAxisRaw("Vertical");
        isJump = Input.GetKeyDown(KeyCode.Space);
        isDucking = Input.GetKeyDown(KeyCode.Q);
        var startPos = transform.position + Vector3.down * 0.16f;
        var endPos = startPos + Vector3.down * 0.04f;
        onGround = Physics2D.Raycast(startPos, Vector2.down, 0.07f, LayerMask.NameToLayer("Player"));
        Debug.DrawLine(startPos, endPos, Color.blue);
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
        transform.position += new Vector3(Speed.x, Speed.y, 0) * Time.deltaTime;
    }

}
