using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float delay = 3f;
    bool hasExploded = false;
    private float countdown;

    void Start()
    {
        countdown = delay;
    }

    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded) // Changed to <= to trigger when countdown reaches zero
        {
            Explode();
            hasExploded = true;
            countdown = delay; // Reset countdown if you want it to repeat, or remove this line to only explode once
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        Debug.Log("Power Picked Up");
    }
    
    void Explode()
    {
        // show effect
        
    }
}
