using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underwater : MonoBehaviour
{

    [HideInInspector]
    public bool underwater = false;

    private bool water = false;
    private bool history = false;
    private bool down = false;
    
    private bool[] childArray = {false, false, false };
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float zRot = 0;
        Transform parent = transform.parent;
        if(parent != null) {
            zRot = parent.eulerAngles.z;
            if(zRot > 180) { zRot -= 360; }
            if(zRot > 15) { down = true; }
            else { down = false; }
        }

        int idx = 0;
        foreach(Transform child in transform) {
            WaterCollider collide = child.GetComponent<WaterCollider>();
            if(collide != null) {
                if(collide.IsTriggerActive == true) {
                    childArray[idx] = true;
                }
                else { childArray[idx] = false; }
                idx += 1;
            } 
        }

        for(int i = 0; i<3; i++) {
            if (childArray[i]) { 
                water = true;
                break;
            }
            else { water = false; }
        }

        if (history == false && down == true && water == true) {
            Debug.Log("Water in");
            underwater = true;
        }
        /*
        else {
            Debug.Log("water: " + water + " history: " + history + "  down: " + down);
            if (down) {
                Debug.Log(zRot);
            }
        }*/
        if(history == true && down == false && water == false) {
            Debug.Log("Water out");
            underwater = false;
        }
        history = water;
        
    }

    
}
