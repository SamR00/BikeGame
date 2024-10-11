using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class runAway : MonoBehaviour
{
    public GameObject targetObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(this.transform.position, targetObj.transform.position, 10 * Time.deltaTime);
    }
}
