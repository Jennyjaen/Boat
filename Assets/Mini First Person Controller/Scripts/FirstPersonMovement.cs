using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed =10f;
    public float turnSpeed = 5f;
    public SerialController serial;

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
    private float sum_x;
    private float sum_y;

    private float max_ang;

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
        max_ang = 0f;
        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;

        serial = FindObjectOfType<SerialController>();
        sum_x = 0;
        sum_y = 0;
    }



    void Update()
    {

        if(serial.x != 0 || serial.y != 0)
        {

            if(serial.x < 0) { rigidbody.AddTorque(0, 10f * (serial.y/ 2200), 0);}
            else { rigidbody.AddTorque(0, -10f * (serial.y / 2200), 0); }
            if(serial.y > 0) {
                rigidbody.AddForce(-1 * transform.right * 15f * (serial.y / 500)); 
                if(serial.x >0) { rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward,  (serial.y/8 )); }
                else { lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, (serial.y/8)); }
                if(sum_y < 0)
                {
                    sum_x = 0;
                    sum_y = 0;
                    rPaddle.transform.localPosition = rightPos;
                    rPaddle.transform.localRotation = rightRot;
                    lPaddle.transform.localPosition = leftPos;
                    lPaddle.transform.localRotation = leftRot;
                }
            }
            else
            {
                if (serial.x <0) { rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, (-serial.y /8 )); }
                else { lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, (-serial.y /8)); }
                if(sum_y > 0)
                {
                    sum_x = 0;
                    sum_y = 0;
                }
            }
            sum_x += serial.x;
            sum_y += serial.y;

        }
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
        if(rigidbody.velocity.magnitude > 15.0f)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * 15.0f;
        }
        if(rigidbody.angularVelocity.magnitude > 1.0f)
        {
            rigidbody.angularVelocity = rigidbody.angularVelocity.normalized * 1.0f;
        }
        //Debug.Log("Velocity: " + rigidbody.velocity.magnitude);
        //Debug.Log("Angular Velocity: " + rigidbody.angularVelocity.magnitude);
        Vector3 up_vector = transform.up;
        Vector3 forward_vector = - transform.forward;
        float ang = Vector3.Angle(up_vector, Vector3.up);
        Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
        Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
        float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
        Debug.Log("angle "+direct_ang);
        
        if(max_ang > transform.position.y)
        {
            max_ang = transform.position.y;
        }
        Debug.Log("max height: "+max_ang); 
    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log("collide");
        Vector3 colPoint = c.contacts[0].point;
        Vector3 playerPoint = transform.position;
        Vector3 direction = colPoint - playerPoint;
        Vector3 localDirection =  transform.InverseTransformDirection(direction).normalized;
        float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        Debug.Log(angle);
        if(angle < 45.0f && angle > -45.0f)
        {
            Debug.Log("Right");
        }
        else if(angle > -135.0f && angle < - 45.0f)
        {
            Debug.Log("Front");
        }
        else if(angle < 135.0f && angle > 45.0f)
        {
            Debug.Log("Back");
        }
        else
        {
            Debug.Log("Left");
        }
    }




}