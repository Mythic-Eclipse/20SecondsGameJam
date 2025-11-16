using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Vector3 _respawnPosition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            other.transform.position = _respawnPosition;
        }
    }
}