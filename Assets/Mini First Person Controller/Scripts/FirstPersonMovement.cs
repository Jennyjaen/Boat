using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

//해당 script에서는 총괄과 haptic feedback을 담당함.
public class FirstPersonMovement : MonoBehaviour {
    [HideInInspector]
    public Underwater underwater;
    [HideInInspector]
    public Input_Delim input_d;
    private float water_status;
    private int[] grass;

    Rigidbody rigidbody;
    Transform front;
    private GameObject lPaddle;
    private GameObject rPaddle;
    private GameObject boat;

    private Vector3 leftPos;
    private Quaternion leftRot;
    private Vector3 rightPos;
    private Quaternion rightRot;
    private bool collide_land;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    [HideInInspector]
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private float collide;
    private float collide_ang;
    private float collide_speed;

    public float incline_deadzone = 1.5f;
    public int max_v = 5;
    public int min_v = 1;
    public int max_width = 12;
    public int min_width = 3;
    public float collide_height = 0.12f;

    private GamePadState state;
    //Input 방법을 여러개로 바꾸기 + 코드 쪼개서 로드 줄이기
    public enum InputMethod {
        GamePad,
        HandStickThrottle,
        HandStickGesture
    }
    public InputMethod inputMethod;
    [HideInInspector]
    public MonoBehaviour GamePadInput;
    [HideInInspector]
    public MonoBehaviour HandThrottle;
    [HideInInspector]
    public MonoBehaviour HandGesture;




    public enum Choice {
        InclineOnly,
        InclineHeight,
        InclineHeight_Semi,
        InclineHeight_Custom
    }
    public Choice selected;

    //배가 뒤집어지지 않게 하기 -> TODO: 제대로 안되고 있는 것 같으니 확인
    private float max_incline;

    [HideInInspector]
    public byte[] larray = new byte[108];
    [HideInInspector]
    public byte[] rarray = new byte[108];
    [HideInInspector]

    void Awake() {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
        boat = GameObject.Find("Boat");
        front = transform.Find("Front");
    }

    void Start() {
        max_incline = 0f;
        collide = 0f;
        collide_ang = 0f;
        collide_speed = 0f;
        water_status = 0;
        collide_land = false;
        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;

        underwater = transform.Find("Front").GetComponent<Underwater>();
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        grass = new int[6];

        GamePadInput = GetComponent<GamePadInput>();
        HandThrottle = GetComponent<HandThrottle>();
        HandGesture = GetComponent<HandGesture>();

        for (int i = 0; i < 108; i++) {
            larray[i] = (byte)0;
            rarray[i] = (byte)0;

        }

    }

    int Is_max(int maxim, int res) {
        if(res == maxim) { return res; }
        //else if(res == minim) { return res + 1; }
        else { return 0; }      
    }


    void updateArray(float collide, float angle, float clamp_ang, float col_ang, float col_s) { //충돌, 물에 빠짐, 배의 기울기
        if (collide == 0.5f) { // collide 후 아무런 피드백 x
            for (int i = 0; i < 108; i++) {
                larray[i] = (byte)0;
                rarray[i] = (byte)0;
            }
        }
        else if (collide == 1f) {// collide
            int x_1 = 0;
            int x_2 = 0;

            float intensity = Mathf.Ceil(col_s * 5) / 5;
            intensity *= 6;
            intensity = Mathf.Round(intensity);

            if (intensity == 6) {
                intensity = 5;
            }

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
                            rarray[107- index] = (byte)(x_1 * 6 + x_2); //rarray 뒤집었음.
                        }
                        else {
                            int index = y * 6 + (x / 2);
                            larray[index] = (byte)(x_1 * 6 + x_2);
                        }
                    }
                }
            }
        }
        else if(collide == 1.5f) { // 물에 빠졌을 때
            byte[] arr = new byte[108];
            int sero = (int) (clamp_ang / 0.03f);
            for (int y = 0; y < 18; y++) {
                for (int n = 0; n < 6; n++) {
                    if (y >= 0 && y <= sero ) {
                        arr[y * 6 + n] = (byte)(5 * 6 + 5);
                    }
                    else { arr[y * 6 + n] = (byte)0; }
                }
            }
            larray = arr;
            System.Array.Reverse(arr); //오른쪽 뒤집었음.
            rarray = arr;
        }
        else {// 기울기: 0f
            if(clamp_ang < incline_deadzone) {
                //Debug.Log("deadzone");
                for(int i= 0; i<108; i++) {
                    larray[i] = (byte)0;
                    rarray[i] = (byte)0;
                    return;
                }
            }

            switch (selected) {
                case Choice.InclineOnly:
                    //무조건 가장자리에서 시작, 진동의 세기와 두께 모두 기울기와 비례/ 절반을 넘어가지 x.
                    // angle에 따라 방향 결정
                    // clamp_ang에 따라 현재 기울기에 따른 두께와 세기 결정.
                    float valid_ang = clamp_ang - incline_deadzone;
                    int valid_level = max_v - min_v + 1;
                    int vib_level = (int)Mathf.Floor((valid_ang * valid_level / (5 - incline_deadzone))) + min_v;
                    if(vib_level > max_v) { vib_level = max_v; }
                    int vib_width =(int) Mathf.Floor(clamp_ang * (max_width +1) / 5);
                    if(vib_width > max_width) { vib_width = max_width; }

                    int res = 0;
                    int x_1 = 0;

                    if (angle >= 337.5 || angle < 22.5) {
                        /*왼쪽*/
                        //Debug.Log("left");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x >= vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 22.5 && angle < 67.5) {
                       // Debug.Log("left down");
                        /*좌측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + (17 - y) < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 67.5 && angle < 112.5) {
                        /*아래*/
                        //Debug.Log("down");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y < 18 - vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 112.5 && angle < 157.5) {
                        /*우측 아래*/
                       // Debug.Log("right down");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + (17 - y) < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                        
                    }
                    else if (angle >= 157.5 && angle < 202.5) {
                        /*오른쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x < 12 - vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 202.5 && angle < 247.5) {
                        //Debug.Log("right up");
                        /*우측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + y < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                        
                    }
                    else if (angle >= 247.5 && angle < 292.5) {
                        // 위
                        //Debug.Log("up");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y >= vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                        

                    }
                    else if (angle >= 292.5 && angle < 337.5) {
                        /*좌측 위*/
                        //Debug.Log("left up");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + y < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    break;


                case Choice.InclineHeight:
                    //배의 높이에 따라 진동 시작/ 끝점이 달라짐. -> 어떻게? + 가로, 세로, 대각선을 다 할 것인가?
                    float height = col_s;
                    if(height < 0) { height = 0; }
                    int start_point = (int)Mathf.Floor(height/ 0.1f);
                    start_point = Mathf.FloorToInt(start_point * 2f / 3f);
                    valid_ang = clamp_ang - incline_deadzone;
                    valid_level = max_v - min_v + 1;
                    vib_level = (int)Mathf.Floor((valid_ang * valid_level / (5 - incline_deadzone))) + min_v;
                    if (vib_level > max_v) { vib_level = max_v; }

                    vib_width = 3;

                    res = 0;
                    x_1 = 0;

                    if (angle >= 337.5 || angle < 22.5) {
                        /*왼쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x >= start_point && x < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 22.5 && angle < 67.5) {
                        /*좌측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + (17 - y) < vib_width + start_point && x + 17 - y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 67.5 && angle < 112.5) {
                        /*아래*/
                        start_point = (int)Mathf.Floor(start_point * 2 / 3);
                        if (start_point > 6) { start_point = 6; }
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (17 - y < vib_width + start_point && 17 - y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 112.5 && angle < 157.5) {
                        /*우측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + (17 - y) < vib_width + start_point && (28 - x - y) >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 157.5 && angle < 202.5) {
                        /*오른쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (11 - x >= start_point && 11 - x < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 202.5 && angle < 247.5) {
                        /*우측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + y < start_point + vib_width && (11 - x) + y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 247.5 && angle < 292.5) {
                        /*위*/
                        start_point = (int)Mathf.Floor(start_point * 2 / 3);
                        if (start_point > 6) { start_point = 6; }
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y >= start_point && y < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 292.5 && angle < 337.5) {
                        /*좌측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + y < vib_width + start_point && x + y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }

                    break;


                case Choice.InclineHeight_Custom:
                    valid_ang = clamp_ang - incline_deadzone;
                    valid_level = max_v - min_v + 1;
                    vib_level = (int)Mathf.Floor((valid_ang * valid_level / (5 - incline_deadzone))) + min_v;
                    if (vib_level > max_v) { vib_level = max_v; }

                    height = col_s;
                    if (height < 0) { height = 0; }
                    vib_width = Mathf.FloorToInt((max_width - min_width) * height) + min_width;
                    if(vib_width > max_width) { vib_width = max_width; }


                    res = 0;
                    x_1 = 0;

                    if (angle >= 337.5 || angle < 22.5) {
                        /*왼쪽*/
                        Debug.Log("left");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x >= vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 22.5 && angle < 67.5) {
                        Debug.Log("left down");
                        /*좌측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + (17 - y) < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 67.5 && angle < 112.5) {
                        /*아래*/
                        Debug.Log("down");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y < 18 - vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 112.5 && angle < 157.5) {
                        /*우측 아래*/
                        Debug.Log("right down");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + (17 - y) < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 157.5 && angle < 202.5) {
                        /*오른쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x < 12 - vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 202.5 && angle < 247.5) {
                        Debug.Log("right up");
                        /*우측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + y < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 247.5 && angle < 292.5) {
                        // 위
                        Debug.Log("up");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y >= vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }


                    }
                    else if (angle >= 292.5 && angle < 337.5) {
                        /*좌측 위*/
                        Debug.Log("left up");
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + y < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    break;


                    break;


                case Choice.InclineHeight_Semi:
                    height = col_s * (2f/3f);
                    if (height < 0) { height = 0; }
                    if(height > 0.6f) { height = 0.6f; }
                    start_point = (int)Mathf.Floor(height / 0.1f);
                    Debug.Log(start_point);
                    valid_ang = clamp_ang - incline_deadzone;
                    valid_level = max_v - min_v + 1;
                    vib_level = (int)Mathf.Floor((valid_ang * valid_level / (5 - incline_deadzone))) + min_v;
                    if (vib_level > max_v) { vib_level = max_v; }

                    //전체의 절반을 ceil 한 것만큼 동안 커지고, 나머지는 진동이 세지기만 하도록 디자인 예정.
                    int increase_lev = Mathf.CeilToInt(valid_level / 2) + min_v -1; 
                    vib_width = (int)Mathf.Floor(clamp_ang * (6 + 1) / increase_lev);
                    if(vib_level > increase_lev) {
                        vib_width = 6;
                    }
                    if (vib_width > max_width) { vib_width = max_width; }

                    res = 0;
                    x_1 = 0;

                    if (angle >= 337.5 || angle < 22.5) {
                        /*왼쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x >= start_point && x < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                        
                    }
                    else if (angle >= 22.5 && angle < 67.5) {
                        /*좌측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + (17 - y) < vib_width + start_point && x + 17 - y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                        
                    }
                    else if (angle >= 67.5 && angle < 112.5) {
                        /*아래*/
                        start_point = (int)Mathf.Floor(start_point * 2 / 3);
                        if (start_point > 6) { start_point = 6; }
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (17 - y < vib_width + start_point && 17 - y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 112.5 && angle < 157.5) {
                        /*우측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + (17 - y) < vib_width + start_point && (28 - x - y) >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 157.5 && angle < 202.5) {
                        /*오른쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (11 - x >= start_point && 11 - x < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 202.5 && angle < 247.5) {
                        /*우측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + y < start_point + vib_width && (11 - x) + y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 247.5 && angle < 292.5) {
                        /*위*/
                        start_point = (int)Mathf.Floor(start_point * 2 / 3);
                        if (start_point > 6) { start_point = 6; }
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y >= start_point && y < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 292.5 && angle < 337.5) {
                        /*좌측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + y < vib_width + start_point && x + y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[107 - (y * 6 + (x / 2))] = (byte)0;
                                }
                            }
                        }
                    }
                    break;

            }
            
        }
        printArray(larray);
        //Debug.Log(string.Join(",", larray));
    }

    void printArray(byte[] array) {
        string output = "";

        for (int i = 0; i < array.Length; i++) {
            output += (int)(array[i] / 6) + " "+ (int)(array[i] % 6)+" "; 

            if ((i + 1) % 6 == 0) {
                output += "\n"; 
            }
        }

        Debug.Log(output);
    }

    void updateEachArray(bool isLeft, float vel, bool reverse, int sum, float water) {
        //water: 0 둘다 일반 노젓기 1: 왼쪽 땅, 1.5: 양쪽 땅, 2: 오른쪽 땅, 3: 풀 위
        byte[] arr = new byte[108];

        switch (inputMethod) {
            case InputMethod.HandStickThrottle:
                if(water == 1) {
                    for(int i = 0; i<108; i++) {
                        larray[i] = (byte)3;
                        rarray[i] = (byte)0;
                    }
                }
                else if(water == 1.5f) {
                    for (int i = 0; i < 108; i++) {
                        larray[i] = (byte)3;
                        rarray[i] = (byte)3;
                    }
                }
                else if(water == 2f) {
                    for (int i = 0; i < 108; i++) {
                        larray[i] = (byte)0;
                        rarray[i] = (byte)3;
                    }
                }
                else if(water == 3f) {

                }
                break;
            case InputMethod.HandStickGesture: // 노젓기
                int max_vib = 0;
                int width = 4;

                if(Mathf.Abs(sum )> 10) {
                    if (isLeft && water == 1) { width = 8; max_vib = 5; }
                    else if (!isLeft && water == 2) { width = 8; max_vib = 5; }
                    else if (water == 1.5f) { width = 8; max_vib = 5; }
                    else { 
                        if (vel <= 3) { max_vib = 5;}
                        else if(vel <= 5) { max_vib = 4;}
                        else if(vel <= 7) { max_vib = 3; }
                        else if(vel <= 9) { max_vib = 2; }
                        else { max_vib = 1; }    
                    }  
                }

                if (reverse && sum > 0) { max_vib = 0; }
                if(!reverse && sum < 0) { max_vib = 0; }
                int sero = Mathf.Abs(sum) / 120;
        
                if (isLeft && water == 1) { sero = Mathf.Abs(sum) / 210; }
                if (!isLeft && water == 2) { sero = Mathf.Abs(sum) / 210; }
                if(water == 1.5f) { sero = Mathf.Abs(sum) / 210; }
                if(water == 3) {
                    sero = Mathf.Abs(sum) / 120;
                    int grass_idx = sero / 3;
                    if(grass_idx >= 6) { grass_idx = 5; }
                    if(grass[grass_idx]!= sero) {
                        if(grass_idx > 0) { sero = grass[grass_idx - 1]; }
                        else { sero = 0; }
                    }
            
                }
                for (int y = 0; y< 18; y++) {
                    for(int n = 0; n<6; n++) {
                        if(y >= sero && y < sero + width) {
                            arr[y * 6 + n] = (byte) (max_vib * 6 + max_vib);
                        }
                        else { arr[y * 6  +n] =(byte) 0; }
                    }
                }
                if (!reverse) { System.Array.Reverse(arr); }
                if (isLeft) { larray = arr; }
                else { 
                    System.Array.Reverse(arr); //오른쪽 장치 뒤집어서 거꾸로 넣어줘야함.
                    rarray = arr;
                }

                break;
        }
        
}
    void GrassEffect() {
        for(int i = 0; i< 6; i++) {
            grass[i] = Random.Range(i * 3, i* 3 + 2);
        }
    }


    void Update() {
        switch (inputMethod) {
            case InputMethod.GamePad:
                GamePadInput.enabled = true;
                HandThrottle.enabled = false;
                HandGesture.enabled = false;
                break;
            case InputMethod.HandStickThrottle:
                GamePadInput.enabled = false;
                HandThrottle.enabled = true;
                HandGesture.enabled = false;
                break;
            case InputMethod.HandStickGesture:
                GamePadInput.enabled = false;
                HandThrottle.enabled = false;
                HandGesture.enabled = true;
                break;
        }

        Vector3 rotation = transform.eulerAngles;
        if (rotation.x > 180){rotation.x -= 360;}
        rotation.x = Mathf.Clamp(rotation.x, -40f, 40f);
        if (rotation.z > 180) { rotation.z -= 360; }
        rotation.z = Mathf.Clamp(rotation.z, -40f, 40f);
        transform.eulerAngles = rotation;


        //최고속도 조절
        if (rigidbody.velocity.magnitude > 15.0f) {
            rigidbody.velocity = rigidbody.velocity.normalized * 15.0f;
        }
        if (rigidbody.angularVelocity.magnitude > 1.0f) {
            rigidbody.angularVelocity = rigidbody.angularVelocity.normalized * 1.0f;
        }

        switch (inputMethod) {
            case InputMethod.GamePad: //게임 패드에 햅틱 피드백을 주는 경우
                if (underwater.underwater) {
                    float currentPositionY = front.position.y;
                    float diff = underwater.water_y - currentPositionY;
                    float intensity = Mathf.Clamp(diff * 5, 0, 1);
                    GamePad.SetVibration(PlayerIndex.One, 0, intensity);
                }
                break;
            case InputMethod.HandStickThrottle: //장치에 햅틱 피드백을 주는 경우
                Vector3 up_vector = transform.up;
                Vector3 forward_vector = -transform.forward;
                float ang = Vector3.Angle(up_vector, Vector3.up);
                Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
                Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
                float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
                float c_ang = Mathf.Clamp(ang, 0, 5);
                float c_speed = Mathf.Clamp(collide_speed, 0, 5);
                c_speed /= 5;
                if (direct_ang < 0) { direct_ang += 360; }
                float bef_coll = collide;
                if (underwater.underwater) {
                    collide = 1.5f;
                    float currentPositionY = front.position.y;
                    float diff = underwater.water_y - currentPositionY;
                    c_ang = diff;
                }
                else {
                    collide = bef_coll;
                    if (bef_coll == 1.5f) {
                        collide = 2.0f;
                    }
                }
                if(water_status != 0f) { // 무언가와 부딪히는 중
                    updateEachArray(true, rigidbody.velocity.magnitude, input_d.reverse, input_d.sum_l, water_status); //left hand
                    updateEachArray(false, rigidbody.velocity.magnitude, input_d.reverse, input_d.sum_r, water_status); //right hand
                }
                else {
                    if(input_d.zerostream >= 100) {
                        collide = 0f; //기울기 피드백은 여기서
                    }
                    updateArray(collide, direct_ang, c_ang, collide_ang, c_speed); 
                }
                
                

                // 노젓기가 없기 때문에 update Each Array를 할 필요가 있나? -> 땅, 풀 처리할거면 필요하긴 함? water status만 보면 될 것 같은데.
                break;
            case InputMethod.HandStickGesture:
                direct_ang = 0f; //기울기 표현 x이기 때문에 계산 필요 x.
                c_ang = 0f;
                c_speed = Mathf.Clamp(collide_speed, 0, 5);
                c_speed /= 5;
                if (direct_ang < 0) { direct_ang += 360; }
                bef_coll = collide;
                if (underwater.underwater) {
                    collide = 1.5f;
                    float currentPositionY = front.position.y;
                    float diff = underwater.water_y - currentPositionY;
                    c_ang = diff;
                }
                else {
                    collide = bef_coll;
                    if(bef_coll == 1.5f) {
                        collide = 2.0f;
                    }
                }
        
                if(collide<2.0f) {updateArray(collide, direct_ang,  c_ang, collide_ang, c_speed); } // 그 외: 충돌, 물에 빠짐, 출렁임
                else { // 노 젓기
                    updateEachArray(true, rigidbody.velocity.magnitude, input_d.reverse, input_d.sum_l, water_status); //left hand
                    updateEachArray(false, rigidbody.velocity.magnitude, input_d.reverse, input_d.sum_r, water_status); //right hand
                }
        
                break;
        }
        

    }

    public float AverageZ(ContactPoint[] contacts) {
        if(contacts.Length == 0) { return 0f; }
        float sumZ = 0f;
        foreach(ContactPoint contact in contacts) {
            Vector3 world_collide = contact.point;
            Vector3 local_collide = transform.InverseTransformPoint(world_collide);
            sumZ +=local_collide.z;
        }
        return sumZ / contacts.Length;
    }

    public int[] CountZ(ContactPoint[] contacts) {
        int[] result = new int[2];

        if (contacts.Length == 0) { return result; }
        foreach (ContactPoint contact in contacts) {
            Vector3 world_collide = contact.point;
            Vector3 local_collide = transform.InverseTransformPoint(world_collide);

            if (local_collide.z > 0.1f) {
                result[1]++;
            }
            else if (local_collide.z < -0.1f) {
                result[0]++;
            }
        }
        return result;
    }
    
    void OnCollisionEnter(Collision c) {
        Vector3 colVelocity = c.relativeVelocity;
        collide_speed = colVelocity.magnitude;

        switch (inputMethod) {
            case InputMethod.GamePad:
                if (!c.collider.CompareTag("Water")) {
                    if (c.collider.CompareTag("Grass")) {
                        StartCoroutine(ShortVibration(0.2f));
                    }
                    else {
                        float c_speed = Mathf.Clamp(collide_speed * 7f, 0, 1);
                        Debug.Log(c_speed);
                        StartCoroutine(ShortVibration(c_speed));
                    }
                }
                break;
            case InputMethod.HandStickThrottle:
            case InputMethod.HandStickGesture:
                if (!c.collider.CompareTag("Water") && !c.collider.CompareTag("Grass")) {
                    if (collide_land && c.collider.CompareTag("Land")) { }
                    else {
                        StartCoroutine(CollisionControl());
                        water_status = 0f;
                    }
            
                }
                else {
                    if (c.collider.CompareTag("Land")) {
                        collide_land = true;
                        float col = AverageZ(c.contacts);
                        if(col >= 0) { water_status = 2.0f; }
                        else if(col < 0){ water_status = 1.0f; }
                        else {
                            water_status = 0f; }
                    }
                    if (c.collider.CompareTag("Grass")) {
                        water_status = 3.0f;
                        GrassEffect();
                    }
                }

                Vector3 colPoint = c.contacts[0].point;
                Vector3 playerPoint = transform.position;
                Vector3 direction = colPoint - playerPoint;
                Vector3 localDirection = transform.InverseTransformDirection(direction).normalized;

                float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
                angle *= -1;
                if (angle < 0) {
                    angle += 360;
                }
                collide_ang = angle;

                break;
        }  
    }

    private void OnCollisionStay(Collision c) {
        if (c.collider.CompareTag("Land")) {
            switch (inputMethod) {
                case InputMethod.GamePad:
                   state = GamePad.GetState(PlayerIndex.One);
                    if (state.IsConnected) {
                        float LX = state.ThumbSticks.Left.X;
                        float LY = state.ThumbSticks.Left.Y;

                        if (LX > 0 || LY > 0) {
                            float magnitude = Mathf.Sqrt(LX * LX + LY * LY); // 벡터의 크기 계산
                            float normalizedIntensity = Mathf.Clamp01(magnitude);
                            float intensity = 0.3f * normalizedIntensity;
                            GamePad.SetVibration(PlayerIndex.One, 0, intensity);
                        }
                        else {
                            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                        }
                    }
                    else {
                        Debug.Log("GamePad disconnected.");
                    }
                    break;
                case InputMethod.HandStickThrottle:
                case InputMethod.HandStickGesture:

                    int[] count = CountZ(c.contacts);
                    if (count[0] >= 1 && count[1] >= 1) {
                        water_status = 1.5f;
                    }
                    else if (count[0] == 0 && count[1] > 0) {
                        water_status = 2f;
                    }
                    else if (count[1] == 0 && count[0] > 0) {
                        water_status = 1f;
                    }
                    break;
            }
        }
    }

    void OnCollisionExit(Collision c) {
        collide = 0f;
        water_status = 0f;
        if (c.collider.CompareTag("Land")) {
            switch (inputMethod) {
                case InputMethod.GamePad:
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                    break;
            }
            collide_land = false;
        }
    }

    private IEnumerator CollisionControl() {
        collide = 1.0f;
        yield return new WaitForSeconds(2.0f);

        collide = 0.5f;
        yield return new WaitForSeconds(1.0f);

        collide = 0.0f;
    }
    IEnumerator ShortVibration(float intensity) {
        GamePad.SetVibration(PlayerIndex.One, intensity, intensity);
        yield return new WaitForSeconds(0.1f);
        GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
    }



}