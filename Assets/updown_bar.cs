using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class updown_bar : MonoBehaviour {
    private GameObject bar;
    public float speed = 30.0f;
    private int mode;
    // Start is called before the first frame update
    void Start() {
        bar = GameObject.Find("Bar_1");
        mode = 0;

    }

    // Update is called once per frame
    void Update() {
        float currentZAngle = bar.transform.rotation.eulerAngles.z;

        if (currentZAngle > 180f) {
            currentZAngle -= 360f;
        }
        if (currentZAngle < 5) { mode = 0; }
        if (currentZAngle > 40) { mode = 1; }

        if (mode == 0) {
            bar.transform.RotateAround(bar.transform.GetChild(0).position, new Vector3(-1f, 0f, 0f), speed * Time.deltaTime);
        }
        else if (mode == 1) {
            bar.transform.RotateAround(bar.transform.GetChild(0).position, new Vector3(1f, 0f, 0f), speed * Time.deltaTime);
        }
    }
}
