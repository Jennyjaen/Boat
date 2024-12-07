using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandThrottle : MonoBehaviour
{
    [HideInInspector]
    public Input_Delim input_d;
    [HideInInspector]
    public float rx;
    [HideInInspector]
    public float ry;
    [HideInInspector]
    public float lx;
    [HideInInspector]
    public float ly;


    private Rigidbody rb;
    private bool colliding = false;

    [HideInInspector]
    public float front_m = 2.5f;
    [HideInInspector]
    public float rotation_m = 15f;
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

    private float last_ly = 0;
    private float last_ry = 0;
    void Start()
    {
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        
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
        lx =accum_x / 400f;
        int accum_y = input_d.accum_ly;
        ly = accum_y / 400f;
        lx = Mathf.Abs(lx) < 0.15f ? 0 : Mathf.Clamp(lx, -1f, 1f);
        ly = Mathf.Abs(ly) < 0.15f ? 0 : Mathf.Clamp(ly, -1f, 1f);
        rx = input_d.accum_rx / 400f;
        ry = input_d.accum_ry / 400f;
        rx = Mathf.Abs(rx) < 0.15f ? 0 : Mathf.Clamp(rx, -1f, 1f);
        ry = Mathf.Abs(ry) < 0.15f ? 0 : Mathf.Clamp(ry, -1f, 1f);


        if(ly != 0) {
            Vector3 forwardMovement = transform.right * ly * front_m * Time.deltaTime;
            float turnAmount = -ly * rotation_m * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
            Vector3 targetPosition = rb.position + forwardMovement;
            if (!colliding) {
                rb.MovePosition(targetPosition);
            }
            transform.rotation *= rotation;

            sum_left += (-ly * Time.deltaTime * 30);
            if (ly < 0) {
                if (last_ly >= 0) { //후진하다 전진하는 케이스
                    lPaddle.transform.localPosition = leftPos;
                    lPaddle.transform.localRotation = leftRot;
                }
                else {
                    if (sum_left > 81) {
                        sum_left = 0;
                        lPaddle.transform.localPosition = leftPos;
                        lPaddle.transform.localRotation = leftRot;
                    }
                }
            }
            else if (ly > 0) {
                if (last_ly <= 0) { //전진하다 후진하는 케이스
                    lPaddle.transform.localPosition = leftPos_back;
                    lPaddle.transform.localRotation = leftRot_back;
                }
                else {
                    if (sum_left < -1) {
                        sum_left = 80;
                        lPaddle.transform.localPosition = leftPos_back;
                        lPaddle.transform.localRotation = leftRot_back;
                    }
                }
            }
            lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, rb.transform.forward, (-ly * Time.deltaTime * 30));

            last_ly = ly;

        }
        /*
        else {
            if(lx < 0) {
                rotationY += lx * 20f * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, -40f, 40f);
            }
        }*/
        if (ry != 0) {
            Vector3 forwardMovement = transform.right * ry * front_m * Time.deltaTime;
            float turnAmount = ry * rotation_m * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
            Vector3 targetPosition = rb.position + forwardMovement;
            if (!colliding) {
                rb.MovePosition(targetPosition);
            }
            transform.rotation *= rotation;

            sum_right += (-ry * Time.deltaTime * 30);
            if (ry < 0) {
                if (last_ry >= 0) { //후진하다 전진하는 케이스
                    rPaddle.transform.localPosition = rightPos;
                    rPaddle.transform.localRotation = rightRot;
                }
                else {
                    if (sum_right > 81) {
                        sum_right = 0;
                        rPaddle.transform.localPosition = rightPos;
                        rPaddle.transform.localRotation = rightRot;
                    }
                }
            }
            else if (ry > 0) {
                if (last_ry <= 0) { //전진하다 후진하는 케이스
                    rPaddle.transform.localPosition = rightPos_back;
                    rPaddle.transform.localRotation = rightRot_back;
                }
                else {
                    if (sum_right < -1) {
                        sum_right = 80;
                        rPaddle.transform.localPosition = rightPos_back;
                        rPaddle.transform.localRotation = rightRot_back;
                    }
                }
            }
            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, rb.transform.forward, (-ry * Time.deltaTime * 30));

            last_ry = ry;
        }
        //Debug.Log($"left : {ly} sum: {sum_left}, right: {ry}, sum: {sum_right}");
    }



    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag != "Land" &&
        collision.gameObject.tag != "Grass" &&
        collision.gameObject.tag != "Water"&&
        collision.gameObject.tag !="Moving"
        )
       {
            colliding = true;
        }
    }

    private void OnCollisionExit(Collision collision) {
        colliding = false;
    }
}
