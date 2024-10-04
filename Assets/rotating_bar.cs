using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotating_bar : MonoBehaviour
{
    // Start is called before the first frame update
    public float rotationSpeed = 30.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
