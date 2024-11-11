using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandThrottle : MonoBehaviour
{
    [HideInInspector]
    public Input_Delim input_d;
    private Transform xrRig;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Rigidbody rb;
    private bool colliding = false;

    private GameObject lPaddle;
    private GameObject rPaddle;

    private Vector3 leftPos;
    private Quaternion leftRot;
    private Vector3 leftPos_back;
    private Quaternion leftRot_back;

    private Vector3 rightPos;
    private Quaternion rightRot;
    private Vector3 rightPos_back;
    private Quaternion rightRot_back;
    private float sum_left = 0f;
    private float sum_right = 0f;

    // Start is called before the first frame update


    void Start()
    {
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        xrRig = GameObject.Find("XR Rig/Camera Offset")?.transform;
        if (xrRig == null) {
            Debug.Log("can not find camera");
        }
        rb = GetComponent<Rigidbody>();
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        leftPos_back = new Vector3(-0.80f, 0.52f, -0.96f);
        leftRot_back = new Quaternion(0.44040f, -0.85550f, 0.27184f, -0.01661f);
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;
        rightPos_back = new Vector3(-0.79f, 0.52f, 0.88f);
        rightRot_back = new Quaternion(0.27185f, 0.01661f, 0.44040f, 0.85550f);

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
        float rx = input_d.accum_rx / 750f;
        float ry = input_d.accum_ry / 750f;
        rx = Mathf.Abs(rx) < 0.15f ? 0 : Mathf.Clamp(rx, -1f, 1f);
        ry = Mathf.Abs(ry) < 0.15f ? 0 : Mathf.Clamp(ry, -1f, 1f);


        if(LY != 0) {
            Vector3 forwardMovement = transform.right * LY * 2.5f * Time.deltaTime;
            float turnAmount = -LY * 15f * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
            Vector3 targetPosition = rb.position + forwardMovement;
            if (!colliding) {
                rb.MovePosition(targetPosition);
            }
            transform.rotation *= rotation;
        }
        else {
            if(LX < 0) {
                rotationY += LX * 20f * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, -40f, 40f);
            }
        }
        if (ry != 0) {
            Vector3 forwardMovement = transform.right * ry * 2.5f * Time.deltaTime;
            float turnAmount = ry * 15f * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
            Vector3 targetPosition = rb.position + forwardMovement;
            if (!colliding) {
                rb.MovePosition(targetPosition);
            }
            transform.rotation *= rotation;
        }
        else {
            if(rx > 0) {
                rotationY += rx * 20f * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, -40f, 40f);
            }
        }
        if (xrRig != null) {
            xrRig.localRotation = Quaternion.Euler(0f, rotationY, 0f);
        }
    }



    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag != "Land" &&
        collision.gameObject.tag != "Grass" &&
        collision.gameObject.tag != "Water") {
            colliding = true;
        }
    }

    private void OnCollisionExit(Collision collision) {
        colliding = false;
    }
}
