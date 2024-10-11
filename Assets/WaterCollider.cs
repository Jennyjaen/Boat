using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollider : MonoBehaviour
{
    public bool IsTriggerActive { get; private set; } = false;

    void Start()
    {
       
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Water")) {
            IsTriggerActive = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Water")) {
            IsTriggerActive = false;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
