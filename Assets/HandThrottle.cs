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
        float LX =accum_x / 500f;
        int accum_y = input_d.accum_ly;
        float LY = accum_y / 500f;
        //Debug.Log($"Origin LX: {LX}, Origin LY: {LY}");
        LX = Mathf.Abs(LX) < 0.15f ? 0 : Mathf.Clamp(LX, -1f, 1f);
        LY = Mathf.Abs(LY) < 0.15f ? 0 : Mathf.Clamp(LY, -1f, 1f);
        Vector3 forwardMovement = transform.right * LY * 5f * Time.deltaTime;
        float turnAmount = LX * 30f * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);

        transform.position += forwardMovement;
        transform.rotation *= rotation;
        //Debug.Log($"LX: {LX}, LY: {LY}");

        float rx = input_d.accum_rx / 750f;
        float ry = input_d.accum_ry / 750f;

        rotationX += ry * rotationSpeed * Time.deltaTime;
        rotationY += rx * rotationSpeed * Time.deltaTime;

        rotationX = Mathf.Clamp(rotationX, -40f, 40f);
        rotationY = Mathf.Clamp(rotationY, -40f, 40f);

        if (xrRig != null) {
            xrRig.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
    }
}
