using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underwater : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Water")) {
            Debug.Log("Underwater");
        }
        else {
            Debug.Log("Overwater");
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Water")) {
            Debug.Log("ByeWater");
        }
        else {
            Debug.Log("Bye");
        }
    }
}
