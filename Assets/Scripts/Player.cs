using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Tracked]
    public class Player : Actor
    {
        #region Constants

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

        public const float ClimbMaxStamina = 110;
        private const float ClimbUpCost = 100 / 2.2f;
        private const float ClimbStillCost = 100 / 10f;
        private const float ClimbJumpCost = 110 / 4f;
        private const int ClimbCheckDist = 2;
        private const int ClimbUpCheckDist = 2;
        private const float ClimbNoMoveTime = .1f;
        public const float ClimbTiredThreshold = 20f;
        private const float ClimbUpSpeed = -45f;
        private const float ClimbDownSpeed = 80f;
        private const float ClimbSlipSpeed = 30f;
        private const float ClimbAccel = 900f;
        private const float ClimbGrabYMult = .2f;
        private const float ClimbHopY = -120f;
        private const float ClimbHopX = 100f;
        private const float ClimbHopForceTime = .2f;
        private const float ClimbJumpBoostTime = .2f;
        private const float ClimbHopNoWindTime = .3f;

        private const float LaunchSpeed = 280f;
        private const float LaunchCancelThreshold = 220f;

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
                    // climbing (holds)
                    else if ((anim.Equals(PlayerSprite.ClimbUp) && (frame == 5)) ||
                        (anim.Equals(PlayerSprite.ClimbDown) && (frame == 5)))
                    {
                        var holding = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Center + Vector2.UnitX * (int)Facing, temp));
                        if (holding != null)
                            Play(Sfxs.char_mad_handhold, SurfaceIndex.Param, holding.GetWallSoundIndex(this, (int)Facing));
                    }
                    else if (anim.Equals("wakeUp") && frame == 19)
                        Play(Sfxs.char_mad_campfire_stand);
                    else if (anim.Equals("sitDown") && frame == 12)
                        Play(Sfxs.char_mad_summit_sit);
                    else if (anim.Equals("push") && (frame == 8 || frame == 15))
                        Dust.BurstFG(Position + new Vector2(-(int)Facing * 5, -1), new Vector2(-(int)Facing, -0.5f).Angle(), 1, 0);
                }
            };

    private void DummyRunTo(float moveX)
                {
                    data.Pusher.Collidable = false;
                    return;
                }

                var was = Position;
                Position = data.TargetPosition;
                if (!CollideCheck<Solid>())
                {
                    data.Pusher.Collidable = false;
                    return;
                }

                Position = was;
                data.Pusher.Collidable = false;
            }

            if (!TrySquishWiggle(data))
                Die(Vector2.Zero);
            else if (ducked && CanUnDuck)
                Ducking = false;
        }

        #endregion

        #region Normal State

        private void NormalBegin()
        {
            maxFall = MaxFall;
        }

        private void NormalEnd()
        {
            wallBoostTimer = 0;
            wallSpeedRetentionTimer = 0;
            hopWaitX = 0;
        }

        //Climb攀爬
        public bool ClimbBoundsCheck(int dir)
        {
            return Left + dir * ClimbCheckDist >= level.Bounds.Left && Right + dir * ClimbCheckDist < level.Bounds.Right;
        }

        public bool ClimbCheck(int dir, int yAdd = 0)
        {
            return ClimbBoundsCheck(dir) && !ClimbBlocker.Check(Scene, this, Position + Vector2.UnitY * yAdd + Vector2.UnitX * ClimbCheckDist * (int)Facing) && CollideCheck<Solid>(Position + new Vector2(dir * ClimbCheckDist, yAdd));
        }

        private const float SpacePhysicsMult = .6f;

        private int NormalUpdate()
        {
            //Use Lift Boost if walked off platform
            if (LiftBoost.Y < 0 && wasOnGround && !onGround && Speed.Y >= 0)
                Speed.Y = LiftBoost.Y;

            if (Holding == null)
            {
                if (Input.Grab.Check && !IsTired && !Ducking)
                {
                    //Grabbing Holdables
                    foreach (Holdable hold in Scene.Tracker.GetComponents<Holdable>())
                        if (hold.Check(this) && Pickup(hold))
                            return StPickup;

                    //Climbing
                    if (Speed.Y >= 0 && Math.Sign(Speed.X) != -(int)Facing)
                    {
                        if (ClimbCheck((int)Facing))
                        {
                            Ducking = false;
                            return StClimb;
                        }

                        if (Input.MoveY < 1 && level.Wind.Y <= 0)
                        {
                            for (int i = 1; i <= ClimbUpCheckDist; i++)
                            {
                                if (!CollideCheck<Solid>(Position + Vector2.UnitY * -i) && ClimbCheck((int)Facing, -i))
                                {
                                    MoveVExact(-i);
                                    Ducking = false;
                                    return StClimb;
                                }
                            }
                        }
                    }
                }

                //Dashing
                if (CanDash)
                {
                    Speed += LiftBoost;
                    return StartDash();
                }

                //Ducking
                if (Ducking)
                {
                    if (onGround && Input.MoveY != 1)
                    {
                        if (CanUnDuck)
                        {
                            Ducking = false;
                            Sprite.Scale = new Vector2(.8f, 1.2f);
                        }
                        else if (Speed.X == 0)
                        {
                            for (int i = DuckCorrectCheck; i > 0; i--)
                            {
                                if (CanUnDuckAt(Position + Vector2.UnitX * i))
                                {
                                    MoveH(DuckCorrectSlide * Engine.DeltaTime);
                                    break;
                                }
                                else if (CanUnDuckAt(Position - Vector2.UnitX * i))
                                {
                                    MoveH(-DuckCorrectSlide * Engine.DeltaTime);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (onGround && Input.MoveY == 1 && Speed.Y >= 0)
                {
                    Ducking = true;
                    Sprite.Scale = new Vector2(1.4f, .6f);
                }
            }
            else
            {
                //Throw
                if (!Input.Grab.Check && minHoldTimer <= 0)
                    Throw();

                //Ducking
                if (!Ducking && onGround && Input.MoveY == 1 && Speed.Y >= 0)
                {
                    Drop();
                    Ducking = true;
                    Sprite.Scale = new Vector2(1.4f, .6f);
                }
            }

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

            //Vertical
            {
                //Calculate current max fall speed
                {
                    float mf = MaxFall;
                    float fmf = FastMaxFall;

                    if (level.InSpace)
                    {
                        mf *= SpacePhysicsMult;
                        fmf *= SpacePhysicsMult;
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

                        if (wallSlideDir != 0)
                        {
                            if (wallSlideTimer > WallSlideTime * .5f && ClimbBlocker.Check(level, this, Position + Vector2.UnitX * wallSlideDir))
                                wallSlideTimer = WallSlideTime * .5f;

                            max = MathHelper.Lerp(MaxFall, WallSlideStartMax, wallSlideTimer / WallSlideTime);
                            if (wallSlideTimer / WallSlideTime > .65f)
                                CreateWallSlideParticles(wallSlideDir);
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
