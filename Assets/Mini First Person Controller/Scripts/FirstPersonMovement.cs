using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonMovement : MonoBehaviour {
    public float speed = 10f;
    public float turnSpeed = 5f;
    public LeftDelimiter lserial;
    public RightDelimiter rserial;

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
    private float sum_l;
    private float sum_r;
    private int stream_l;
    private int stream_r;
    private float collide;
    private float collide_ang;
    private float collide_speed;

    private Material lPanelM;
    private Material rPanelM;


    private float max_ang;
    private float min_ang;
    private float max_incline;

    [HideInInspector]
    public byte[] larray = new byte[108];
    [HideInInspector]
    public byte[] rarray = new byte[108];
    [HideInInspector]
    public bool toggle;

    void Awake() {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
        boat = GameObject.Find("Boat");
    }

    void Start() {
        max_ang = -0.1f;
        min_ang = -0.1f;
        max_incline = 0f;
        collide = 0f;
        collide_ang = 0f;
        collide_speed = 0f;

        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;

        lserial = transform.Find("LDelim").GetComponent<LeftDelimiter>();
        rserial = transform.Find("RDelim").GetComponent<RightDelimiter>();
        sum_l = 0;
        sum_r = 0;
        stream_l = 0;
        stream_r = 0;
        toggle = false;

        Transform lPanelTransform = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/LPanel");
        Transform rPanelTransform = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/RPanel");
        if (lPanelTransform != null) {
            Image lPanelImage = lPanelTransform.GetComponent<Image>();
            if (lPanelImage != null) {
                lPanelM = lPanelImage.material;
            }
            else {
                Debug.Log("No renderer left");
            }
        }
        else { Debug.Log("No Left"); }
        if (rPanelTransform != null) {
            Image rPanelImage = rPanelTransform.GetComponent<Image>();
            if (rPanelImage != null) {
                rPanelM = rPanelImage.material;
            }
            else {
                Debug.Log("No renderer left");
            }

        }
        else { Debug.Log("No Right"); }

        for (int i = 0; i < 108; i++) {
            larray[i] = (byte)0;
            rarray[i] = (byte)0;

        }

    }

    void updateArray(float collide, float angle, float clamp_ang, float col_ang, float col_s) {
        if (collide == 0.5f) {
            for (int i = 0; i < 108; i++) {
                larray[i] = (byte)0;
                rarray[i] = (byte)0;
            }
        }
        else if (collide > 0.5) {// collide
            int x_1 = 0;
            int x_2 = 0;

            //Debug.Log("collide speed: " + col_s);
            
            float intensity = Mathf.Ceil(col_s * 5) / 5;
            intensity *= 6;
            intensity = Mathf.Round(intensity);

            if (intensity == 6) {
                intensity = 5;
            }

            //Debug.Log("intensity: " + intensity);
             for (int y = 0; y < 18; y++) {
                for (int x = 0; x < 24; x++){
                    float cent_x = ((float)x +0.5f) / 24;
                    float cent_y = ((float)y + 0.5f) / 12;
                    float res;
                    if (col_ang >= 22.5 && col_ang < 67.5) {
                        if (cent_x + cent_y >= (2 - col_s * 2)) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 67.5 && col_ang < 112.5) {
                        if (cent_y >= 1 - col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 112.5 && col_ang < 157.5) {
                        if (-cent_x + cent_y >= 1 - 2 * col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 157.5 && col_ang < 202.5) {
                        if (cent_x < col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 202.5 && col_ang < 247.5) {
                        if (cent_x + cent_y <= 2 * col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 247.5 && col_ang < 292.5) {
                        if (cent_y < col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 292.5 && col_ang < 337.5) {
                        if (cent_x - cent_y >= 1 - 2 * col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else {
                        if (cent_x >= 1 - col_s) { res = intensity; }
                        else { res = 0; }
                    }

                    if (x % 2 == 0) { x_1 = (int)res; }
                    else {
                        x_2 = (int)res;
                        if (x >= 12) {
                            int index = y * 6 + ((x - 12) / 2);
                            rarray[index] = (byte)(x_1 * 6 + x_2);
                        }
                        else {
                            int index = y * 6 + (x / 2);
                            larray[index] = (byte)(x_1 * 6 + x_2);
                        }
                    }
                }
            }
        }
        else {// normal case
            float start = Mathf.Lerp(0.5f, 0f, (clamp_ang / 5));
            float end = Mathf.Lerp(0.5f, 1f, (clamp_ang / 5));

            float res;
            int x_1 = 0;
            int x_2 = 0;
            for(int y = 0; y < 18; y++) {
                for (int x = 0; x < 24; x++) {
                    float cent_x = ((float)x + 0.5f) / 24f;
                    float cent_y = ((float)y + 0.5f) / 18f;
                    float direction = 0f;
                        
                    if (angle >= 337.5 || angle < 22.5) { direction = cent_x; }
                    else if (angle >= 22.5 && angle < 67.5) { direction = (1.0f + cent_x - cent_y) * 0.5f; }
                    else if (angle >= 67.5 && angle < 112.5) {direction = 1.0f - cent_y; }
                    else if (angle >= 112.5 && angle < 157.5) {  direction = (2.0f - cent_x - cent_y) * 0.5f; }
                    else if (angle >= 157.5 && angle < 202.5) { direction = 1.0f - cent_x; }
                    else if (angle >= 202.5 && angle < 247.5) { direction = (1.0f - cent_x + cent_y) * 0.5f; }
                    else if (angle >= 247.5 && angle < 292.5) {  direction = cent_y; }
                    else if (angle >= 292.5 && angle < 337.5) { direction = (cent_x + cent_y) * 0.5f;}
                    /*
                    if(angle >= 45 && angle < 135) { direction = 1.0f - cent_y; }
                    else if (angle >= 135 && angle < 225) { direction = 1.0f - cent_x; }
                    else if(angle >= 225 && angle < 315) { direction = cent_y; }
                    else { direction = cent_x; }*/
                    res = Mathf.Lerp(end, start, direction);
                        
                    res *= 6;
                    res = Mathf.Floor(res);
                    if (res == 6) { res = 5; }
                    /*
                    res *= 3;
                    res = Mathf.Floor(res);
                    if (res == 3) { res =2; }
                    res *= 2;
                    res += 1;*/
                    //if (x % 2 == 0 && res ==0) { Debug.Log("x: " + x + "y: " + y + " centx: " + cent_x + " direction: "+ direction+ "check: "+ check); }
                    //if (x % 2 == 0) { Debug.Log("res: " + res); }

                    if (x % 2 == 0) { 
                        x_1 = (int)res; 
                        //if(x_1 == 0) { Debug.Log("x: " + x + "y: " + y + " mode: " + mode + " garo: " + garo + "sero: " + sero); }
                    }
                    else {
                        x_2 = (int)res;
                        if (x >= 12) {
                            int index = y * 6 + ((x - 12) / 2);
                            rarray[index] = (byte)(x_1 * 6 + x_2);
                        }
                        else {
                            int index = y * 6 + (x / 2);
                            larray[index] = (byte)(x_1 * 6 + x_2);
                            }
                    }

                }
            }
        }
        //printArray(larray);
        //Debug.Log(string.Join(",", larray));
    }

    void printArray(byte[] array) {
        string output = "";  // 출력할 문자열을 저장할 변수

        for (int i = 0; i < array.Length; i++) {
            output += (int)(array[i] / 6) + " "+ (int)(array[i] % 6)+" ";  // 배열의 값을 출력 문자열에 추가

            // groupSize 개씩 출력할 때마다 줄바꿈
            if ((i + 1) % 6 == 0) {
                output += "\n";  // groupSize마다 줄바꿈 추가
            }
        }

        // 최종 출력
        Debug.Log(output);
    }

    void updateEachArray(bool isLeft, float vel, bool reverse, int sum) {

        byte[] arr = new byte[108];
        int max_vib = 0;
        int width = 4;
        if(Mathf.Abs(sum )> 10) {
            if(vel <= 1) { max_vib = 5;}
            else if(vel <= 3) { max_vib = 4;}
            else if(vel <= 5) { max_vib = 3; }
            else if(vel <= 7) { max_vib = 2; }
            else { max_vib = 1; }

        }
        if (reverse && sum > 0) { max_vib = 0; }
        if(!reverse && sum < 0) { max_vib = 0; }
        int sero = sum / 120;
        for(int y = 0; y< 18; y++) {
            for(int n = 0; n<6; n++) {
                if(y >= sero && y < sero + width) {
                    arr[y * 6 + n] = (byte) (max_vib * 6 + max_vib);
                }
                else { arr[y * 6  +n] =(byte) 0; }
            }
        }
        if (!reverse) { System.Array.Reverse(arr); }
        if (isLeft) { larray = arr; }
        else { rarray = arr; }
}

    void Update() {
        Vector3 rotation = transform.eulerAngles;
        if (rotation.x > 180){rotation.x -= 360;}
        rotation.x = Mathf.Clamp(rotation.x, -40f, 40f);
        transform.eulerAngles = rotation;

        if (lserial.y != 0) {
            //Debug.Log("Left " + lserial.x + ", " + lserial.y);

            if (lserial.y > 0 && !toggle) {
                rigidbody.AddForce(-1 * transform.right * 0.08f * lserial.y);
                rigidbody.AddTorque(0, 0.01f * lserial.y, 0);
                //rigidbody.AddTorque(0.005f * lserial.y, 0, 0);
            }
            else if(lserial.y <0 && toggle) {
                rigidbody.AddForce(-1 * transform.right * 0.08f * lserial.y);
                rigidbody.AddTorque(0, -0.01f * lserial.y, 0);
            }
                //노 회전 애니메이션
            lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, (lserial.y / 10));
            
        }


        if (rserial.y != 0) {
            //Debug.Log("Right " + rserial.x + ", " + rserial.y);
            if (rserial.y > 0 && !toggle) {
                rigidbody.AddForce(-1 * transform.right * 0.08f * rserial.y);
                rigidbody.AddTorque(0, -0.01f * rserial.y, 0);
                //rigidbody.AddTorque(-0.005f * rserial.y, 0, 0);
            }
            else if (rserial.y < 0 && toggle) {
                rigidbody.AddForce(-1 * transform.right * 0.08f * rserial.y);
                rigidbody.AddTorque(0, -0.01f * rserial.y, 0);
            }
            //노 회전 애니메이션
            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, (rserial.y / 10));

        }


        // 키보드로 보트 조작 및 노 회전
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            leftKeydown = true;
        }
        if (leftKeydown) {
            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, 100f * Time.deltaTime);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) && leftKeydown) {
            leftKeydown = false;
            rigidbody.AddTorque(0f, -10f, 0f);
            //rigidbody.AddTorque(-5f, 0f, 0f);
            rigidbody.AddForce(-1 * transform.right * 15f);
            rPaddle.transform.localPosition = rightPos;
            rPaddle.transform.localRotation = rightRot;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            rightKeydown = true;
        }
        if (rightKeydown) {
            lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, 100f * Time.deltaTime);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) && rightKeydown) {
            rightKeydown = false;
            rigidbody.AddTorque(0f, 10f, 0f);
            //rigidbody.AddTorque(5f, 0f, 0f);
            rigidbody.AddForce(-1 * transform.right * 15f);
            lPaddle.transform.localPosition = leftPos;
            lPaddle.transform.localRotation = leftRot;
        }


        //최고속도 조절
        if (rigidbody.velocity.magnitude > 15.0f) {
            rigidbody.velocity = rigidbody.velocity.normalized * 15.0f;
        }
        if (rigidbody.angularVelocity.magnitude > 1.0f) {
            rigidbody.angularVelocity = rigidbody.angularVelocity.normalized * 1.0f;
        }


        Vector3 up_vector = transform.up;
        Vector3 forward_vector = -transform.forward;
        float ang = Vector3.Angle(up_vector, Vector3.up);
        float clamp = Mathf.Clamp(ang * 3f, 0, 5);
        Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
        Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
        float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
        float c_speed = Mathf.Clamp(collide_speed, 0, 3);
        c_speed /= 3;
        if (direct_ang < 0) { direct_ang += 360; }
        //Debug.Log("angle "+direct_ang);
        //collide = 1.0f;

        if(collide == 0f || collide ==2f) {
            if(lserial.zerostream > 50 && rserial.zerostream > 50) { collide = 0f; }
            else { collide = 2.0f; }
        }

        //Debug.Log(collide);
        if(collide != 2.0f) {updateArray(collide, direct_ang,  clamp, collide_ang, c_speed); }
        else { 
            updateEachArray(true, rigidbody.velocity.magnitude, toggle, lserial.sum); //left hand
            updateEachArray(false, rigidbody.velocity.magnitude, toggle, rserial.sum); //right hand
        }

        /*
        else
        {
            Debug.Log("Can not find renderer");
        }*/

        if (max_ang < transform.position.y) {
            max_ang = transform.position.y;
        }
        if (max_incline < ang) {
            max_incline = ang;
        }
        if (min_ang > transform.position.y) {
            min_ang = transform.position.y;
        }
        /*
        Debug.Log("max height: "+max_ang); 
        Debug.Log("min height: " + min_ang);
        Debug.Log("height diff: " + (max_ang - min_ang));
        Debug.Log("max incline: " + max_incline);*/
    }

    void OnCollisionEnter(Collision c) {
        StartCoroutine(CollisionControl());
        Vector3 colPoint = c.contacts[0].point;
        Vector3 playerPoint = transform.position;
        Vector3 direction = colPoint - playerPoint;
        Vector3 localDirection = transform.InverseTransformDirection(direction).normalized;

        Vector3 colVelocity = c.relativeVelocity;
        float colForce = colVelocity.magnitude;
        collide_speed = colVelocity.magnitude;

        float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        angle *= -1;
        if (angle < 0) {
            angle += 360;
        }
        collide_ang = angle;

        Debug.Log(colForce);
        if (angle < 45.0f && angle > -45.0f) {
            Debug.Log("Right");
        }
        else if (angle > -135.0f && angle < -45.0f) {
            Debug.Log("Back");
        }
        else if (angle < 135.0f && angle > 45.0f) {
            Debug.Log("Front");
        }
        else {
            Debug.Log("Left");
        }
    }

    private IEnumerator CollisionControl() {
        collide = 1.0f;
        yield return new WaitForSeconds(2.0f);

        collide = 0.5f;
        yield return new WaitForSeconds(1.0f);

        collide = 0.0f;
    }




}