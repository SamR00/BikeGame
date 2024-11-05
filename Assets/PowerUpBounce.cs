using UnityEngine;

public class BounceAndSpin : MonoBehaviour
{
    // Speed of rotation and bounce
    public float rotationSpeed = 100f;       
    public float bounceSpeed = 2f;           
    public float bounceHeight = 0.5f;       

    // Original Y position of the object
    private float initialY;

    void Start()
    {
        // Store the initial Y position of the object
        initialY = transform.position.y;
    }

    void Update()
    {
        // Rotate the object around its Y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bounce the object up and down
        float newY = initialY + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
