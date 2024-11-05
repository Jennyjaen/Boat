using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandThrottle : MonoBehaviour
{
    [HideInInspector]
    public Input_Delim input_d;
    private Transform xrRig;
    public float rotationSpeed = 20f;
    private float rotationX = 0f;
    private float rotationY = 0f;
    // Start is called before the first frame update

    public enum InputMethod {
        GamePad,
        Throttle
    }

    public InputMethod inputMethod;

    void Start()
    {
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        xrRig = GameObject.Find("XR Rig/Camera Offset")?.transform;
        if (xrRig == null) {
            Debug.Log("can not find camera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        int accum_x = input_d.accum_lx;
        float LX =accum_x / 900f;
        int accum_y = input_d.accum_ly;
        float LY = accum_y / 600f;
        LX = Mathf.Abs(LX) < 0.15f ? 0 : Mathf.Clamp(LX, -1f, 1f);
        LY = Mathf.Abs(LY) < 0.15f ? 0 : Mathf.Clamp(LY, -1f, 1f);
        float rx = -input_d.accum_rx / 750f;
        float ry = -input_d.accum_ry / 750f;
        rx = Mathf.Abs(rx) < 0.15f ? 0 : Mathf.Clamp(rx, -1f, 1f);
        ry = Mathf.Abs(ry) < 0.15f ? 0 : Mathf.Clamp(ry, -1f, 1f);
        Debug.Log($"RX: {rx} , RY: {ry}");
        switch (inputMethod) {
            case InputMethod.GamePad:
                Vector3 forwardMovement = transform.right * LY * 5f * Time.deltaTime;
                float turnAmount = LX * 30f * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);

                transform.position += forwardMovement;
                transform.rotation *= rotation;
                rotationX += ry * rotationSpeed * Time.deltaTime;
                rotationY += rx * rotationSpeed * Time.deltaTime;

                rotationX = Mathf.Clamp(rotationX, -40f, 40f);
                rotationY = Mathf.Clamp(rotationY, -40f, 40f);

                if (xrRig != null) {
                    xrRig.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
                }

                break;
            case InputMethod.Throttle:
                if(LY != 0) {
                    forwardMovement = transform.right * LY * 2.5f * Time.deltaTime;
                    turnAmount = -LY * 10f * Time.deltaTime;
                    rotation = Quaternion.Euler(0, turnAmount, 0);
                    transform.position += forwardMovement;
                    transform.rotation *= rotation;
                }
                else {
                    if(LX < 0) {
                        rotationY += LX * rotationSpeed * Time.deltaTime;
                        rotationY = Mathf.Clamp(rotationY, -40f, 40f);
                    }
                }
                if (ry != 0) {
                    forwardMovement = transform.right * ry * 2.5f * Time.deltaTime;
                    turnAmount = ry * 10f * Time.deltaTime;
                    rotation = Quaternion.Euler(0, turnAmount, 0);
                    transform.position += forwardMovement;
                    transform.rotation *= rotation;
                }
                else {
                    if(rx > 0) {
                        rotationY += rx * rotationSpeed * Time.deltaTime;
                        rotationY = Mathf.Clamp(rotationY, -40f, 40f);
                    }
                }
                if (xrRig != null) {
                    xrRig.localRotation = Quaternion.Euler(0f, rotationY, 0f);
                }
                break;

        }
        //Debug.Log($"Origin LX: {LX}, Origin LY: {LY}");
        
        
        //Debug.Log($"LX: {LX}, LY: {LY}");


        
    }
}
