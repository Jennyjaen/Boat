using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed =10f;
    public float turnSpeed = 5f;

    private bool leftKeydown = false;
    private bool rightKeydown = false;
    Rigidbody rigidbody;
    private GameObject lPaddle;
    private GameObject rPaddle;
    private GameObject boat;

    private Vector3 leftPos;
    private Quaternion leftRot;
    private Vector3 rightPos;
    private Quaternion rightRot;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();



    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
        boat = GameObject.Find("Boat");
    }

    void Start()
    {   
        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;
    }



    void Update()
    {
        // 키보드로 보트 조작 및 노 회전
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { 
            leftKeydown = true;
        }
        if (leftKeydown) {
            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, 100f * Time.deltaTime);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) && leftKeydown) { 
            leftKeydown=false;
            rigidbody.AddTorque(0f, - 10f, 0f);
            rigidbody.AddForce(-1 * transform.right * 10f);
            rPaddle.transform.localPosition = rightPos;
            rPaddle.transform.localRotation = rightRot;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightKeydown = true;
        }
        if (rightKeydown)
        {
            lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, 100f * Time.deltaTime);
        }
            if (Input.GetKeyUp(KeyCode.RightArrow) && rightKeydown)
        {
            rightKeydown = false;
            rigidbody.AddTorque(0f, 10f, 0f);
            rigidbody.AddForce(-1 * transform.right * 10f);
            lPaddle.transform.localPosition = leftPos;
            lPaddle.transform.localRotation = leftRot;
        }


        //최고속도 조절
        if(rigidbody.velocity.magnitude > 10.0f)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * 10.0f;
        }
        if(rigidbody.angularVelocity.magnitude > 1.0f)
        {
            rigidbody.angularVelocity = rigidbody.angularVelocity.normalized * 1.0f;
        }
        Debug.Log("Velocity: " + rigidbody.velocity.magnitude);
        Debug.Log("Angular Velocity: " + rigidbody.angularVelocity.magnitude);
    }
}