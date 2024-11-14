using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlBuoyancy : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject player;
    InclineBuo2[] scripts;
    void Start()
    {
        scripts = GetComponents<InclineBuo2>();
        player = GameObject.Find("First Person Controller Minimal");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(player.transform.position.z);
        if(player.transform.position.z <3.5f) {
            for (int i = 0; i < scripts.Length; i++) {
                if (i == 0)
                    scripts[i].enabled = true; 
                else
                    scripts[i].enabled = false;
            }
        }
        else if(player.transform.position.z < 42) {
            for (int i = 0; i < scripts.Length; i++) {
                if (i == 1)
                    scripts[i].enabled = true;
                else
                    scripts[i].enabled = false;
            }
        }
        else {
            for (int i = 0; i < scripts.Length; i++) {
                if (i == 2)
                    scripts[i].enabled = true;
                else
                    scripts[i].enabled = false;
            }
        }
    }
}
