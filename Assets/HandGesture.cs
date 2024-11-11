using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGesture : MonoBehaviour
{
    [HideInInspector]
    public Input_Delim input_d;
    Rigidbody rigidbody;
    private GameObject lPaddle;
    private GameObject rPaddle;
    private bool leftKeydown = false;
    private bool rightKeydown = false;

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


    private GameObject boat;
    private FirstPersonMovement person;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        person = transform.GetComponent<FirstPersonMovement>();
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        leftPos_back = new Vector3(-0.80f, 0.52f, -0.96f);
        leftRot_back = new Quaternion(0.44040f, -0.85550f, 0.27184f, -0.01661f);
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;
        rightPos_back = new Vector3(-0.79f, 0.52f, 0.88f);
        rightRot_back = new Quaternion(0.27185f, 0.01661f, 0.44040f, 0.85550f);
        boat = GameObject.Find("Boat");
    }

    // Update is called once per frame
    void Update()
    {
        
        //아두이노로 보트 조작
        if (input_d.left_y != 0) {
            if (input_d.left_y > 0 && !input_d.l_reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.06f * input_d.left_y);
                rigidbody.AddTorque(0, 0.01f * input_d.left_y, 0);
                //rigidbody.AddTorque(0.005f * input_d.left_y, 0, 0);
            }
            else if (input_d.left_y < 0 && input_d.l_reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.06f * input_d.left_y);
                rigidbody.AddTorque(0, 0.01f * input_d.left_y, 0);
            }
            lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, rigidbody.transform.forward, (input_d.left_y /30));
            sum_left += (input_d.left_y / 30);
            if(sum_left >81) {
                sum_left = 0;
                lPaddle.transform.localPosition = leftPos;
                lPaddle.transform.localRotation = leftRot;
            }
            else if(sum_left < -1) {
                sum_left = 80;
                lPaddle.transform.localPosition = leftPos_back;
                lPaddle.transform.localRotation = leftRot_back;
            }
            //Debug.Log($"transform: {lPaddle.transform.localPosition} , rotation: {lPaddle.transform.localRotation}, sum: {sum_left}");
        }

        if (input_d.right_y != 0) {
            //Debug.Log("Right " + rserial.x + ", " + input_d.right_y);
            if (input_d.right_y > 0 && !input_d.r_reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.06f * input_d.right_y);
                rigidbody.AddTorque(0, -0.01f * input_d.right_y, 0);
                //rigidbody.AddTorque(-0.005f * input_d.right_y, 0, 0);
            }
            else if (input_d.right_y < 0 && input_d.r_reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.06f * input_d.right_y);
                rigidbody.AddTorque(0, -0.01f * input_d.right_y, 0);
            }
            //노 회전 애니메이션
            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, rigidbody.transform.forward, (input_d.right_y / 30));
            sum_right += (input_d.right_y / 30);
            if (sum_right > 81) {
                sum_right = 0;
                lPaddle.transform.localPosition = rightPos;
                lPaddle.transform.localRotation = rightRot;
            }
            else if (sum_right < -1) {
                sum_right = 80;
                lPaddle.transform.localPosition = rightPos_back;
                lPaddle.transform.localRotation = rightRot_back;
            }
            //Debug.Log($"transform: {rPaddle.transform.localPosition} , rotation: {rPaddle.transform.localRotation}, sum: {sum_right}");
        }


        // 키보드로 보트 조작 및 노 회전
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            leftKeydown = true;
        }
        if (leftKeydown) {
            //rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, 100f * Time.deltaTime);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) && leftKeydown) {
            leftKeydown = false;
            rigidbody.AddTorque(0f, -10f, 0f);
            //rigidbody.AddTorque(-5f, 0f, 0f);
            rigidbody.AddForce(-1 * transform.right * 15f);
            //rPaddle.transform.localPosition = rightPos;
            //rPaddle.transform.localRotation = rightRot;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            rightKeydown = true;
        }
        if (rightKeydown) {
            //lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, 100f * Time.deltaTime);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) && rightKeydown) {
            rightKeydown = false;
            rigidbody.AddTorque(0f, 10f, 0f);
            //rigidbody.AddTorque(5f, 0f, 0f);
            rigidbody.AddForce(-1 * transform.right * 15f);
            //lPaddle.transform.localPosition = leftPos;
            //lPaddle.transform.localRotation = leftRot;
        }
    }
}
