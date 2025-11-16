using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public enum PlatformType
    {
        Target,
        LinearFly
    }

    [Header("Тип платформы")]
    [SerializeField] private PlatformType _platformType = PlatformType.Target;

    [Header("Целевая точка движения (в мировых координатах)")]
    [SerializeField] private Vector2 _targetPosition;

    [Header("Время остановки на целевой точке (в секундах)")]
    [SerializeField] private float _stopTimeAtTarget = 1f;

    [Header("Расстояние для начала замедления")]
    [SerializeField] private float _slowDownDistance = 2f;

    [Header("Минимальная скорость при замедлении")]
    [SerializeField] private float _minSpeed = 0.5f;

    private Vector2 _startPosition;
    private Vector2 _movingVelocity;
    private bool _isMovingToTarget = true;
    private Rigidbody2D _rigidbody;
    private float _stopTimer;

    public Vector2 MovingVelocity => _movingVelocity;

    [Header("Скорость движения")]
    [SerializeField] public float Speed = 2f;

    [Header("Направление движения для режима LinearFly")]
    public Vector2 FlyDirection { get; set; }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _startPosition = _rigidbody.position;
        _stopTimer = _stopTimeAtTarget;
    }

    void FixedUpdate()
    {
        switch (_platformType)
        {
            case PlatformType.Target:
                UpdateTargetMode();
                break;
            case PlatformType.LinearFly:
                UpdateLinearFlyMode();
                break;
        }
    }

    private void UpdateTargetMode()
    {
        Vector2 currentPos = _rigidbody.position;
        Vector2 targetPos = _isMovingToTarget ? _targetPosition : _startPosition;
        Vector2 direction = (targetPos - currentPos).normalized;
        float distanceToTarget = Vector2.Distance(currentPos, targetPos);

        if (distanceToTarget <= 0.1f)
        {
            _movingVelocity = Vector2.zero;
            if (_stopTimer > 0f)
            {
                _stopTimer -= Time.fixedDeltaTime;
            }
            else
            {
                _isMovingToTarget = !_isMovingToTarget;
                _stopTimer = _stopTimeAtTarget;
            }
        }
        else
        {
            float currentSpeed = CalculateSpeed(distanceToTarget);
            _movingVelocity = direction * currentSpeed;
        }

        _rigidbody.linearVelocity = _movingVelocity;
    }

    private void UpdateLinearFlyMode()
    {
        _rigidbody.linearVelocity = FlyDirection * Speed;
        _movingVelocity = FlyDirection * Speed;
    }

    private float CalculateSpeed(float distanceToTarget)
    {
        if (distanceToTarget >= _slowDownDistance)
        {
            return Speed;
        }

        float normalizedDistance = distanceToTarget / _slowDownDistance;
        float speedMultiplier = Mathf.SmoothStep(0f, 1f, normalizedDistance);
        return Mathf.Lerp(_minSpeed, Speed, speedMultiplier);
    }
}
