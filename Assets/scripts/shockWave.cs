using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float maxRadius = 5f;       // Maximum radius of the shockwave
    public float expansionSpeed = 10f;  // Speed at which the shockwave expands
    public float slowEffectDuration = 3f; // Duration of the slow effect on enemies
    public float slowFactor = 0.5f;     // Factor to slow down the enemies (0 to 1)

    private void Update()
    {
        // Expand the shockwave
        float currentRadius = transform.localScale.x / 2;

        if (currentRadius < maxRadius)
        {
            transform.localScale += Vector3.one * expansionSpeed * Time.deltaTime;
        }
        else
        {
            // Destroy the shockwave after it has expanded to max radius
            Destroy(gameObject);
        }

        // Detect enemies in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // Apply the slow effect to the enemy
                EnemyController enemy = hitCollider.GetComponent<EnemyController>(); // Assuming your enemy script is named "Enemy"
                if (enemy != null)
                {
                    enemy.ApplySlow(slowEffectDuration, slowFactor);
                }
            }
        }
    }
}
