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
    private Vector3 rightPos;
    private Quaternion rightRot;
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
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;
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
            //lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, (input_d.left_y / 10));

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
            //rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, (input_d.right_y / 10));

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
