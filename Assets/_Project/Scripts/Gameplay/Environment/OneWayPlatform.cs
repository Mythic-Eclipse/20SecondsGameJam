using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
    private Collider2D _collider;
    private float _effectorResetTimer = 0f;
    private const float EFFECTOR_RESET_TIME = 0.3f; // Time in seconds to reset the effector

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
        {
            Debug.LogError("OneWayPlatform requires a Collider2D component.");
        }
    }

    private void FixedUpdate()
    {
        if (_effectorResetTimer > 0f)
        {
            _effectorResetTimer -= Time.fixedDeltaTime;
            if (_effectorResetTimer <= 0f)
            {
                SwitchCollider(true);
                _effectorResetTimer = 0f;
            }
        }
    }

    public void SwitchCollider(bool enable)
    {
        _collider.enabled = enable;
        _effectorResetTimer = EFFECTOR_RESET_TIME;
    }
}
