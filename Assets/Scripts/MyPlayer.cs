using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : MonoBehaviour
{

    int _totalVerticalRays;
    int _totalHorizontalRays;

    float _skinWidth = 0.02f;
    float _slopeLimit = 30.0f;
    float _jumpThreshold = 0.07f;
    float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);
    float _vertivalDistanceBetweenRays;
    float _horizontalDistanceBetweenRays;
    float _kSkinWidthFloatFudgeFactor = 0.001f;

    bool _onGround;
    bool _isGoingUpSlope;
    RaycastOrigin _raycastOrigins;
    RaycastHit2D raycastHit;
    BoxCollider2D _boxCollider;
    AnimationCurve _slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));


    void CalcuRaycastOrigins()
    {
        var bounds = _boxCollider.bounds;
        bounds.Expand(-2f * _skinWidth);
        _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    private void DrawRay(Vector2 start, Vector2 end, Color color)
    {
        Debug.DrawRay(start, end, color);
    }

    void MoveHorizontally(ref Vector2 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        var initialRayOrigin = isGoingRight ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;

        for (int i = 0; i < _totalHorizontalRays; i++)
        {
            var rayOrigin = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _vertivalDistanceBetweenRays);
            DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            var raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, i == 0 && _onGround ? LayerMask.GetMask("Platform", "OneWayPlatform") : LayerMask.GetMask(""));
            if (raycastHit)
            {
                if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(raycastHit.normal, Vector2.up)))
                {
                    // if we weren't grounded last frame, that means we're landing on a slope horizontally.
                    // this ensures that we stay flush to that slope
                }
                deltaMovement.x = raycastHit.point.x - rayOrigin.x;
                rayDistance = Mathf.Abs(deltaMovement.x);
                if (isGoingRight)
                {
                    deltaMovement.x -= _skinWidth;
                }
                else
                {
                    deltaMovement.x += _skinWidth;
                }
                if (rayDistance < _skinWidth + _kSkinWidthFloatFudgeFactor)
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
                var rayOrigin = isGoingRight ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;
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

    void MoveVertically(ref Vector2 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        var initialRayOrigin = isGoingUp ? _raycastOrigins.TopLeft : _raycastOrigins.BottomLeft;
        initialRayOrigin.x += deltaMovement.x;
        var mask = isGoingUp && !_onGround ? LayerMask.GetMask("") : LayerMask.GetMask("", "");
        for (int i = 0; i < _totalVerticalRays; i++)
        {
            var rayOrigin = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);
            DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
            raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, mask);
            if (raycastHit)
            {
                deltaMovement.y = raycastHit.point.y - rayOrigin.y;
                rayDistance = Mathf.Abs(deltaMovement.y);
                if (isGoingUp)
                {
                    deltaMovement.y -= _skinWidth;
                }
                else
                {
                    deltaMovement.y += _skinWidth;
                }
                if (!isGoingUp && deltaMovement.y > 0.00001f)
                {
                    _isGoingUpSlope = true;
                }
                if (rayDistance < _skinWidth + _kSkinWidthFloatFudgeFactor)
                {
                    break;
                }
            }
        }
    }

    void HandleVerticalSlope(ref Vector2 deltaMovement)
    {
        var centerOfCollider = (_raycastOrigins.BottomLeft.x + _raycastOrigins.BottomRight.x) * 0.5f;
        var rayDistance = _slopeLimitTangent * (_raycastOrigins.BottomRight.x - centerOfCollider);
        var rayDirection = Vector2.down;
        var slopeRayOrigin = new Vector2(centerOfCollider, _raycastOrigins.BottomLeft.y);
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
}
