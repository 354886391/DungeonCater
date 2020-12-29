using UnityEngine;

struct RaycastOrigin
{
    public Vector2 topLeft;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
}

struct CharacterState
{
    public bool left;
    public bool right;
    public bool duck;
    public bool dash;
    public bool onGround;
    public bool wasOnGround;
}

public class Raycast : MonoBehaviour
{
    int _totalVerticalRays = 8;
    int _totalHorizontalRays = 5;

    float _skinWidth = 0.02f;
    float _slopeLimit = 30.0f;
    float _jumpThreshold = 0.07f;
    float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);
    float _verticalDistanceBetweenRays;
    float _horizontalDistanceBetweenRays;
    float _kSkinWidthFloatFudgeFactor = 0.001f;

    public LayerMask multiPlatformMask;
    public LayerMask onewayPlatformMask;

    RaycastHit2D raycastHit;
    BoxCollider2D _boxCollider;
    RaycastOrigin _raycastOrigin;
    CharacterState characterState;
    AnimationCurve _slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        CalculateDistanceBetweenRays();
    }

    public void UpdateRaycastOrigin()
    {
        var bounds = _boxCollider.bounds;
        bounds.Expand(-2f * _skinWidth);
        _raycastOrigin.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigin.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigin.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    public void CalculateDistanceBetweenRays()
    {
        var colliderUseableHeight = _boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
        _verticalDistanceBetweenRays = colliderUseableHeight / (_totalHorizontalRays - 1);

        var colliderUseableWidth = _boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
        _horizontalDistanceBetweenRays = colliderUseableWidth / (_totalVerticalRays - 1);
    }

    public void FixedHorizontalMovement(ref Vector2 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        var initialRayOrigin = isGoingRight ? _raycastOrigin.bottomRight : _raycastOrigin.bottomLeft;

        for (int i = 0; i < _totalHorizontalRays; i++)
        {
            var rayOrigin = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);
            DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            //横向射线(横向在空中可通过单向平台)
            if (i == 0 && characterState.onGround)
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, multiPlatformMask);
            else
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, multiPlatformMask & ~onewayPlatformMask);
            if (raycastHit)
            {
                deltaMovement.x = raycastHit.point.x - rayOrigin.x; //横向移动检测到碰撞时修正移动距离
                if (isGoingRight)
                {
                    deltaMovement.x -= _skinWidth;
                    characterState.right = true;
                }
                else
                {
                    deltaMovement.x += _skinWidth;
                    characterState.left = true;
                }
                if (Mathf.Abs(deltaMovement.x) < _skinWidth + _kSkinWidthFloatFudgeFactor)
                {
                    break;
                }
            }
        }
    }

    public void FixedVerticalMovement(ref Vector2 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        var initialRayOrigin = isGoingUp ? _raycastOrigin.topLeft : _raycastOrigin.bottomLeft;
        initialRayOrigin.x += deltaMovement.x;
        for (int i = 0; i < _totalVerticalRays; i++)
        {
            var rayOrigin = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);
            DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            //纵向射线(纵向在空中可通过单向平台)
            if (!isGoingUp && characterState.onGround)
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, multiPlatformMask);
            else
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, multiPlatformMask & ~onewayPlatformMask);
            if (raycastHit)
            {
                deltaMovement.y = raycastHit.point.y - rayOrigin.y; //横向移动检测到碰撞时修正移动距离
                if (isGoingUp)
                {
                    deltaMovement.y -= _skinWidth;
                }
                else
                {
                    deltaMovement.y += _skinWidth;
                    characterState.onGround = true;
                }
                if (Mathf.Abs(deltaMovement.y) < _skinWidth + _kSkinWidthFloatFudgeFactor)
                {
                    break;
                }
            }
        }
    }

    private bool HandleHorizontalSlope(ref Vector2 deltaMovement, float angle)
    {
        if (Mathf.RoundToInt(angle) == 90)
        {
            return false;
        }
        if (angle < _slopeLimit)
        {
            if (deltaMovement.y < _jumpThreshold)
            {
                var slopeModifier = _slopeSpeedMultiplier.Evaluate(angle);
                deltaMovement.x *= slopeModifier;
                deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                var isGoingRight = deltaMovement.x > 0;
                var rayOrigin = isGoingRight ? _raycastOrigin.bottomRight : _raycastOrigin.bottomLeft;
                var raycastHit = Physics2D.Raycast(rayOrigin, deltaMovement.normalized, deltaMovement.magnitude, characterState.onGround ? LayerMask.GetMask("", "") : LayerMask.GetMask(""));
                if (raycastHit)
                {
                    deltaMovement = raycastHit.point - rayOrigin;
                    if (isGoingRight)
                    {
                        deltaMovement.x -= _skinWidth;
                    }
                    else
                    {
                        deltaMovement.x += _skinWidth;
                    }
                }
            }
        }
        else
        {
            deltaMovement.x = 0;
        }
        return true;
    }

    void HandleVerticalSlope(ref Vector2 deltaMovement)
    {
        var centerOfCollider = (_raycastOrigin.bottomLeft.x + _raycastOrigin.bottomRight.x) * 0.5f;
        var rayDistance = _slopeLimitTangent * (_raycastOrigin.bottomRight.x - centerOfCollider);
        var rayDirection = Vector2.down;
        var slopeRayOrigin = new Vector2(centerOfCollider, _raycastOrigin.bottomLeft.y);
        DrawRay(slopeRayOrigin, rayDirection * rayDistance, Color.yellow);
        var raycastHit = Physics2D.Raycast(slopeRayOrigin, rayDirection, rayDistance, LayerMask.GetMask("", ""));
        if (raycastHit)
        {
            var angle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (angle == 0)
            {
                return;
            }
            var isMovingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);
            if (isMovingDownSlope)
            {
                var slopeModifier = _slopeSpeedMultiplier.Evaluate(-angle);
                deltaMovement.y += raycastHit.point.y - slopeRayOrigin.y - _skinWidth;
                deltaMovement = new Vector3(0, deltaMovement.y) + Quaternion.AngleAxis(-angle, Vector3.forward) * new Vector3(deltaMovement.x * slopeModifier, 0);
            }
        }
    }

    private void DrawRay(Vector2 start, Vector2 end, Color color)
    {
        Debug.DrawRay(start, end, color);
    }
}
