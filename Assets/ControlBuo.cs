using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlBuo : MonoBehaviour
{

    private inclinebuoyancy[] buoyancy;
    private FirstPersonMovement person;
    // Start is called before the first frame update
    void Start()
    {
        person = transform.parent.GetComponent<FirstPersonMovement>();
        buoyancy = GetComponents<inclinebuoyancy>();
        if(buoyancy.Length > 0) {
            foreach (var b in buoyancy) {
                b.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (person.track) {
            case FirstPersonMovement.Track.Track1:
                if (transform.position.z <50) {
                    buoyancy[1].enabled = true;
                    buoyancy[0].enabled = false;
                }
                else {
                    buoyancy[0].enabled = true;
                    buoyancy[1].enabled = false;
                }
                buoyancy[2].enabled = false;
                buoyancy[3].enabled = false;
                break;
            case FirstPersonMovement.Track.Track2:
                if (transform.position.z >71.5f) {
                    buoyancy[3].enabled = true;
                    buoyancy[2].enabled = false;
                }
                else {
                    buoyancy[2].enabled = true;
                    buoyancy[3].enabled = false;
                }
                buoyancy[0].enabled = false;
                buoyancy[1].enabled = false;
                break;
        }
    }
}
