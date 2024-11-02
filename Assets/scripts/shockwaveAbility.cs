using UnityEngine;

public class ShockwaveAbility : MonoBehaviour
{
    public GameObject shockwavePrefab; // Assign your shockwave prefab here
    public Transform shockwaveSpawnPoint; // The point where the shockwave will spawn

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Example trigger: Spacebar
        {
            CastShockwave();
        }
    }

    private void CastShockwave()
    {
        // Instantiate the shockwave at the specified spawn point
        Instantiate(shockwavePrefab, shockwaveSpawnPoint.position, Quaternion.identity);
    }
}
