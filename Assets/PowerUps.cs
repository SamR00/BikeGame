using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class PowerUps : MonoBehaviour
{
    public GameObject pickupeffect;
    public float multiplier = 1.4f;
    public float duration = 3f;

    void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Pickup(other));
        }
    }

    IEnumerator Pickup(Collider Player)
    {
        //spawn a cool effect
        Instantiate(pickupeffect, transform.position, transform.rotation);
        
        //apply effect to the player
        Player.transform.localScale *= multiplier;
        
        PlayerStats stats = Player.GetComponent<PlayerStats>();
        stats.health *= multiplier;

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        //wait x amount of seconds
        yield return new WaitForSeconds(duration);

        //reverse effect on player
        stats.health /= multiplier;
        Player.transform.localScale /= multiplier;

        //remove power up object
        Destroy(gameObject);
    }
}
