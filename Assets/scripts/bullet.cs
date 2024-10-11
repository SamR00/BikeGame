using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f;

    private void Start()
    {
        // Destroy the bullet after a certain time to avoid clutter in the scene
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // You can handle what happens when the bullet hits something here (if needed)
        // For example: Only destroy the bullet if it hits certain objects
        if (!collision.collider.CompareTag("enemy"))
        {
            Destroy(gameObject);
        }
        
    }
}