using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public enum WallCollisionSide
{
    NONE,
    LEFT,
    RIGHT,
    BOTH
}

public enum WallRaycastOrigin
{
    UPPER_LEFT,
    MIDDLE_LEFT,
    LOWER_LEFT,
    UPPER_RIGHT,
    MIDDLE_RIGHT,
    LOWER_RIGHT
}

public enum CollisionState
{
    GROUNDED,
    TOUCHING_WALL,
    ON_SLOPE,
    ON_SLIDING_SLOPE,
    GRABBING_ANGLE,
    ON_MOVING_PLATFORM,
    ON_ONE_WAY_PLATFORM,
    ON_ICE_PLATFORM,
    ON_SOLID_SURFACE
}

public struct WallRaycastHit
{
    public WallRaycastOrigin Origin;
    public RaycastHit2D Hit;
}

public struct WallCollision
{
    public WallCollisionSide Side;
    public List<WallRaycastHit> Hits;
    public float Distance;
    public int GetIntSide()
    {
        if (Side == WallCollisionSide.LEFT) return -1;
        else if (Side == WallCollisionSide.RIGHT) return 1;
        else return 0;
    }
}

public class CollisionDetector : MonoBehaviour
{
    private Dictionary<CollisionState, bool> _collisionStates = new Dictionary<CollisionState, bool>
    {
        { CollisionState.GROUNDED, false },
        { CollisionState.ON_SOLID_SURFACE, false },
        { CollisionState.TOUCHING_WALL, false },
        { CollisionState.ON_SLOPE, false },
        { CollisionState.ON_SLIDING_SLOPE, false },
        { CollisionState.GRABBING_ANGLE, false },
        { CollisionState.ON_MOVING_PLATFORM, false },
        { CollisionState.ON_ONE_WAY_PLATFORM, false }
    };
    MovementConfig _movementConfig;
    private PlayerController _controller;
    private Rigidbody2D _rigidbody;
    private Bounds _bounds;
    private BoxCollider2D _collider;
    private WallCollision _wallCollision;
    private List<WallRaycastHit> _wallHits;
    private Vector2 _slopeNormal;
    private RaycastHit2D _groundHit;
    private float _slopeAngle, _previousSlopeAngle;
    private float _groundDistance;

    public Dictionary<CollisionState, bool> CollisionStates => _collisionStates;
    public BoxCollider2D Collider => _collider;
    public WallCollision WallCollision => _wallCollision;
    public Vector2 SlopeNormal => _slopeNormal;
    public RaycastHit2D GroundHit => _groundHit;
    public float SlopeAngle => _slopeAngle;
    public float PreviousSlopeAngle => _previousSlopeAngle;
    public float GroundDistance => _groundDistance;


    private void Awake()
    {
        _movementConfig = Resources.Load<MovementConfig>("Configs/MovementConfig");
        _controller = GetComponent<PlayerController>();
        _wallHits = new List<WallRaycastHit>();
        _collider = GetComponent<BoxCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void CheckCollisions()
    {
        UpdateBounds();
        HandleGroundCollision();
        CheckWalls();
        CheckSlope();
        CheckMovingPlatform();
        HandleAngleGrab();
        HandleSolidGroundStep();
    }

    public void UpdateBounds()
    {
        _bounds = _collider.bounds;
    }

    public void HandleGroundCollision()
    {
        Vector2 boxCastOrigin = new Vector2(_bounds.center.x, _bounds.min.y);
        Vector2 boxSize = new Vector2(_bounds.size.x, _bounds.size.y / 20);
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCastOrigin,
            boxSize,
            0f,
            Vector2.down,
            100f,
            _movementConfig.GroundLayer
        );

        if (hit)
        {
            _groundDistance = hit.distance;
            if (_groundDistance < 0.02f)
            {
                _collisionStates[CollisionState.GROUNDED] = true;
                _groundHit = hit;
                _collisionStates[CollisionState.ON_ONE_WAY_PLATFORM] = hit.collider.GetComponent<OneWayPlatform>() != null;

                if (_movementConfig.DebugMode)
                {
                    DrawAngle(hit.point, _slopeNormal, _rigidbody.linearVelocity);
                }
            }
            else
            {
                _collisionStates[CollisionState.GROUNDED] = false;
            }
        }
        else
        {
            _groundDistance = -1f;
            _collisionStates[CollisionState.GROUNDED] = false;
        }
    }

    public void HandleAngleGrab()
    {
        // Размер и позиция коробки — небольшая зона перед головой
        float grabBoxWidth = _bounds.size.x * 0.7f;
        float grabBoxHeight = _bounds.size.y * 0.15f;
        Vector2 boxSize = new Vector2(grabBoxWidth, grabBoxHeight);

        // Определяем направление взгляда (например, вправо или влево)
        float lookDirection = _controller.LookingDirection;
        Vector2 castDirection = new Vector2(lookDirection, 0f);

        // Центр коробки — немного выше головы и немного вперед по взгляду
        Vector2 boxOrigin = new Vector2(
            _bounds.center.x + lookDirection * (_bounds.extents.x + grabBoxWidth / 2f),
            _bounds.max.y - grabBoxHeight / 2f
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            boxOrigin,
            boxSize,
            0f,
            castDirection,
            0.01f,
            _movementConfig.GroundLayer
        );

        _collisionStates[CollisionState.GRABBING_ANGLE] = hit;

        if (_movementConfig.DebugMode)
        {
            DrawBox(boxOrigin, boxSize, _collisionStates[CollisionState.GRABBING_ANGLE] ? Color.green : Color.red);
        }
    }

    public void CheckWalls()
    {
        _wallHits.Clear();
        bool leftHit = false;
        bool rightHit = false;
        float minDistance = float.MaxValue;

        Vector2[] originsLeft = {
            new Vector2(_bounds.min.x, _bounds.max.y), // Upper Left
            new Vector2(_bounds.min.x, _bounds.center.y), // Middle Left
            new Vector2(_bounds.min.x, _bounds.min.y) // Lower Left
        };

        Vector2[] originsRight = {
            new Vector2(_bounds.max.x, _bounds.max.y), // Upper Right
            new Vector2(_bounds.max.x, _bounds.center.y), // Middle Right
            new Vector2(_bounds.max.x, _bounds.min.y) // Lower Right
        };

        foreach (Vector2 origin in originsLeft)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                Vector2.left,
                _movementConfig.WallCollisionDistance,
                _movementConfig.WallLayer
            );
            if (hit && Mathf.Abs(hit.normal.y) < 0.1f)
            {
                leftHit = true;
                minDistance = Mathf.Min(minDistance, hit.distance);
                _wallHits.Add(new WallRaycastHit
                {
                    Origin = (WallRaycastOrigin)Array.IndexOf(originsLeft, origin),
                    Hit = hit
                });
            }
        }

        foreach (Vector2 origin in originsRight)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                Vector2.right,
                _movementConfig.WallCollisionDistance,
                _movementConfig.WallLayer
            );
            if (hit && Mathf.Abs(hit.normal.y) < 0.1f)
            {
                rightHit = true;
                minDistance = Mathf.Min(minDistance, hit.distance);
                _wallHits.Add(new WallRaycastHit
                {
                    Origin = (WallRaycastOrigin)(Array.IndexOf(originsRight, origin) + 3), // Offset by 3 for right origins
                    Hit = hit
                });
            }

            WallCollisionSide wallSide = WallCollisionSide.NONE;
            if (leftHit && rightHit) wallSide = WallCollisionSide.BOTH;
            else if (leftHit) wallSide = WallCollisionSide.LEFT;
            else if (rightHit) wallSide = WallCollisionSide.RIGHT;

            if (_wallCollision.Side != WallCollisionSide.NONE)
            {
                _collisionStates[CollisionState.TOUCHING_WALL] = true;
            }
            else
            {
                _collisionStates[CollisionState.TOUCHING_WALL] = false;
                minDistance = -1f;
            }

            _wallCollision = new WallCollision
            {
                Side = wallSide,
                Hits = _wallHits,
                Distance = minDistance
            };
        }
    }

    public void HandleSolidGroundStep()
    {
        if (_collisionStates[CollisionState.GROUNDED] && !_collisionStates[CollisionState.ON_SLIDING_SLOPE])
        {
            _collisionStates[CollisionState.ON_SOLID_SURFACE] = true;
        }
        else
        {
            _collisionStates[CollisionState.ON_SOLID_SURFACE] = false;
        }
        
    }

    public void CheckMovingPlatform()
    {
        _collisionStates[CollisionState.ON_MOVING_PLATFORM] = false;
        Vector2 boxCastOrigin = new Vector2(_bounds.center.x, _bounds.min.y + 0.01f);
        Vector2 boxSize = new Vector2(_bounds.size.x * 0.9f, 0.1f);
        float castDistance = _movementConfig.CollisionOffset * 3;

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCastOrigin,
            boxSize,
            0f,
            Vector2.down,
            castDistance,
            _movementConfig.GroundLayer
        );

        if (hit && hit.collider.GetComponent<MovingPlatform>() != null)
        {
            _collisionStates[CollisionState.ON_MOVING_PLATFORM] = true;
        }
    }

    public void CheckSlope()
    {
        Vector2 origin = new Vector2(_bounds.center.x, _bounds.min.y + 0.01f);
        Vector2 boxSize = new Vector2(_bounds.size.x * 0.9f, 0.1f);
        float castDistance = _movementConfig.CollisionOffset;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            boxSize,
            0f,
            Vector2.down,
            castDistance,
            _movementConfig.GroundLayer
        );

        _collisionStates[CollisionState.ON_SLOPE] = false;
        _collisionStates[CollisionState.ON_SLIDING_SLOPE] = false;
        _slopeNormal = Vector2.up;
        _slopeAngle = 0f;

        if (hit && Mathf.Abs(hit.normal.x) > 0.05f && Mathf.Abs(hit.normal.x) < 0.9f)
        {
            _collisionStates[CollisionState.ON_SLOPE] = true;
            _slopeNormal = hit.normal;
            float slopeAngle = Vector2.Angle(Vector2.up, _slopeNormal);
            if (slopeAngle > 0f && slopeAngle < 70f)
            {
                _slopeAngle = Vector2.Angle(Vector2.up, _slopeNormal);
            }

            if (_slopeAngle > 39f && _slopeAngle < 80f)
            {
                _collisionStates[CollisionState.ON_SLIDING_SLOPE] = true;
            }
        }
    }

    public void SnapToWall () {
        Vector2 newPosition = new Vector2(_rigidbody.position.x, _rigidbody.position.y);
        if (_wallCollision.Side == WallCollisionSide.LEFT) {
            newPosition.x = _rigidbody.position.x - _wallCollision.Distance + 0.05f;
        }
        else {
            newPosition.x = _rigidbody.position.x + _wallCollision.Distance - 0.05f;
        }
        _rigidbody.position = newPosition;
        WallCollision wallCollision = new WallCollision();
        wallCollision.Side = WallCollisionSide.NONE;
        wallCollision.Distance = 0;
        _wallCollision = wallCollision;
    }

    public bool HasFullWallContacts()
    {
        return _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.UPPER_LEFT) &&
               _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.MIDDLE_LEFT) &&
               _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.LOWER_LEFT)
            ||
               _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.UPPER_RIGHT) &&
               _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.MIDDLE_RIGHT) &&
               _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.LOWER_RIGHT);
    }

    public bool HasCornerGrabContacts()
    {
        bool hasTwoWallHits = ((_wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.MIDDLE_LEFT) && _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.LOWER_LEFT))
        ||
        (_wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.MIDDLE_RIGHT) && _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.LOWER_RIGHT)))
        && _wallCollision.Hits.Count == 2;

        return hasTwoWallHits;
    }

    public bool HasMiddleCornerGrabContact()
    {
        bool hasMiddleCornerContact = (_wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.MIDDLE_LEFT) ||
                                    _wallCollision.Hits.Any(h => h.Origin == WallRaycastOrigin.MIDDLE_RIGHT))
                                    && _wallCollision.Hits.Count == 1;
        return hasMiddleCornerContact;
    }
    
    private void DrawAngle(Vector2 origin, Vector2 direction, Vector2 normal)
    {
        Debug.DrawLine(origin, origin + direction, Color.red);
        Debug.DrawLine(origin, origin + normal, Color.green);
    }

    private void DrawBox(Vector2 origin, Vector2 size, Color color)
    {
        Vector2 topLeft = new Vector2(origin.x - size.x / 2, origin.y + size.y / 2);
        Vector2 topRight = new Vector2(origin.x + size.x / 2, origin.y + size.y / 2);
        Vector2 bottomLeft = new Vector2(origin.x - size.x / 2, origin.y - size.y / 2);
        Vector2 bottomRight = new Vector2(origin.x + size.x / 2, origin.y - size.y / 2);
        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
}
