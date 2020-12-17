using UnityEngine;

struct RaycastOrigin
{
    public Vector2 TopLeft;
    public Vector2 BottomLeft;
    public Vector2 BottomRight;
}

class Raycast : MonoBehaviour
{
    int _totalVerticalRays = 8;
    int _totalHorizontalRays = 5;

    float _skinWidth = 0.02f;
    float _slopeLimit = 30.0f;
    float _jumpThreshold = 0.07f;
    float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);
    float _vertivalDistanceBetweenRays;
    float _horizontalDistanceBetweenRays;
    float _kSkinWidthFloatFudgeFactor = 0.001f;

    bool _onGround;

    LayerMask _multiPlatformMask;
    LayerMask _onewayPlatformMask;
    RaycastHit2D raycastHit;
    RaycastOrigin _raycastOrigin;
    BoxCollider2D _boxCollider;
    AnimationCurve _slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    void UpdateRaycastOrigin()
    {
        var bounds = _boxCollider.bounds;
        bounds.Expand(-2f * _skinWidth);
        _raycastOrigin.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigin.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigin.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    void FixedHorizontalMovement(ref Vector2 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        var initialRayOrigin = isGoingRight ? _raycastOrigin.BottomRight : _raycastOrigin.BottomLeft;

        for (int i = 0; i < _totalHorizontalRays; i++)
        {
            var rayOrigin = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _vertivalDistanceBetweenRays);
            DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            //横向射线(横向在空中可通过单向平台)
            if (i == 0 && _onGround)
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, _multiPlatformMask);
            else
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, _multiPlatformMask & ~_onewayPlatformMask);
            if (raycastHit)
            {
                deltaMovement.x = raycastHit.point.x - rayOrigin.x; //横向移动检测到碰撞时修正移动距离
                if (isGoingRight)
                {
                    deltaMovement.x -= _skinWidth;
                }
                else
                {
                    deltaMovement.x += _skinWidth;
                }
                if (Mathf.Abs(deltaMovement.x) < _skinWidth + _kSkinWidthFloatFudgeFactor)
                {
                    break;
                }
            }
        }
    }

    void FixedVerticalMovement(ref Vector2 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        var initialRayOrigin = isGoingUp ? _raycastOrigin.TopLeft : _raycastOrigin.BottomLeft;
        initialRayOrigin.x += deltaMovement.x;
        for (int i = 0; i < _totalVerticalRays; i++)
        {
            var rayOrigin = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);
            DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            //纵向射线(纵向在空中可通过单向平台)
            if (!isGoingUp && _onGround)
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, _multiPlatformMask);
            else
                raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, _multiPlatformMask & ~_onewayPlatformMask);
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
                var rayOrigin = isGoingRight ? _raycastOrigin.BottomRight : _raycastOrigin.BottomLeft;
                var raycastHit = Physics2D.Raycast(rayOrigin, deltaMovement.normalized, deltaMovement.magnitude, _onGround ? LayerMask.GetMask("", "") : LayerMask.GetMask(""));
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
        var centerOfCollider = (_raycastOrigin.BottomLeft.x + _raycastOrigin.BottomRight.x) * 0.5f;
        var rayDistance = _slopeLimitTangent * (_raycastOrigin.BottomRight.x - centerOfCollider);
        var rayDirection = Vector2.down;
        var slopeRayOrigin = new Vector2(centerOfCollider, _raycastOrigin.BottomLeft.y);
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
