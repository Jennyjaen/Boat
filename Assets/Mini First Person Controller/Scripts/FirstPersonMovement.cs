using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private Material lPanelM;
    private Material rPanelM;

    private float max_ang;
    private float min_ang;
    private float max_incline;

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
        max_ang = -0.1f;
        min_ang = -0.1f;
        max_incline = 0f;

        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;

        serial = FindObjectOfType<SerialController>();
        sum_x = 0;
        sum_y = 0;

        Transform lPanelTransform = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/LPanel");
        Transform rPanelTransform = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/RPanel");
        if (lPanelTransform != null)
        {
            Image lPanelImage = lPanelTransform.GetComponent<Image>();
            if (lPanelImage != null)
            {
                lPanelM = lPanelImage.material;
            }
            else
            {
                Debug.Log("No renderer left");
            }
        }
        else { Debug.Log("No Left"); }
        if (rPanelTransform != null) {
            Image rPanelImage = rPanelTransform.GetComponent<Image>();
            if (rPanelImage != null)
            {
                rPanelM = rPanelImage.material;
            }
            else
            {
                Debug.Log("No renderer left");
            }

        }
        else { Debug.Log("No Right"); }

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

        Vector3 up_vector = transform.up;
        Vector3 forward_vector = - transform.forward;
        float ang = Vector3.Angle(up_vector, Vector3.up);
        Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
        Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
        float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
        if(direct_ang< 0) { direct_ang += 360; }
        //Debug.Log("angle "+direct_ang);
        if (lPanelM != null && rPanelM != null) {
            lPanelM.SetFloat("_Angle", direct_ang);
            rPanelM.SetFloat("_Angle", direct_ang);
            float clamp = Mathf.Clamp(ang, 0, 5);
            lPanelM.SetFloat("_Intensity", clamp / 5.0f);
            rPanelM.SetFloat("_Intensity", clamp / 5.0f);

            float max_height = -0.05f;
            float min_height = -0.15f;
            Debug.Log("height: " + transform.position.y);
            float height_clamp = Mathf.Clamp01((transform.position.y - min_height) / (max_height - min_height));
            height_clamp = 1.0f -height_clamp * 0.5f;
            lPanelM.SetFloat("_Scale", height_clamp);
            rPanelM.SetFloat("_Scale", height_clamp);
        }
       
        /*
        else
        {
            Debug.Log("Can not find renderer");
        }*/
        
        if(max_ang < transform.position.y)
        {
            max_ang = transform.position.y;
        }
        if (max_incline < ang)
        {
            max_incline = ang;
        }
        if (min_ang > transform.position.y)
        {
            min_ang = transform.position.y;
        }
        /*
        Debug.Log("max height: "+max_ang); 
        Debug.Log("min height: " + min_ang);
        Debug.Log("height diff: " + (max_ang - min_ang));
        Debug.Log("max incline: " + max_incline);*/
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