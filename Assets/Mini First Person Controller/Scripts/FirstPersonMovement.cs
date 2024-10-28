using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonMovement : MonoBehaviour {
    [HideInInspector]
    public Underwater underwater;
    [HideInInspector]
    public Input_Delim input_d;
    private float water_status;
    private int[] grass;

    private bool leftKeydown = false;
    private bool rightKeydown = false;
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

    private float previouspos =0f;

    //기울기 피드백 개선: dead zone 만들기, 새로 체크
    public float incline_deadzone = 1.5f;
    public int max_v = 5;
    public int min_v = 1;
    public int max_width = 12;
    public float collide_height = 0.12f;
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


    void updateArray(float collide, float angle, float clamp_ang, float col_ang, float col_s) {
        if (collide == 0.5f) {
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
            rarray = arr;
        }
        else {// normal case
            if(clamp_ang < incline_deadzone) {
                for(int i= 0; i<108; i++) {
                    larray[i] = (byte)0;
                    rarray[i] = (byte)0;
                    return;
                }
            }

            float start = Mathf.Lerp(0.5f, 0f, (clamp_ang / 5));
            float end = Mathf.Lerp(0.5f, 1f, (clamp_ang / 5));
            int maxim= (int) Mathf.Floor(end *6);
            if(maxim == 6) { maxim = 5; }
            int minim = (int)Mathf.Floor(start * 6);


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
                        /*오른쪽*/
                        for(int y = 0; y< 18; y++) {
                            for(int x = 0; x<12; x++) {
                                if(x < 12 - vib_width) { res = 0; }
                                else { res = vib_level; }
                                if(x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[y * 6 + (x / 2)] = (byte)( x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 22.5 && angle < 67.5) {
                        /*우측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + y < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 67.5 && angle < 112.5) {
                        /*위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y  >= vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 112.5 && angle < 157.5) {
                        /*좌측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if(x + y < vib_width) { res = vib_level;  }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 157.5 && angle < 202.5) {
                        /*왼쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x >= vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)0;
                                }
                            }
                        }

                    }
                    else if (angle >= 202.5 && angle < 247.5) {
                        /*좌측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + (17- y) < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 247.5 && angle < 292.5) {
                        /*아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y < 18 - vib_width) { res = 0; }
                                else { res = vib_level; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 292.5 && angle < 337.5) {
                        /*우측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + (17 - y) < vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    break;


                case Choice.InclineHeight:
                    //배의 높이에 따라 진동 시작/ 끝점이 달라짐. -> 어떻게? + 가로, 세로, 대각선을 다 할 것인가?
                    float height = transform.position.y;
                    if(height < 0) { height = 0; }
                    int start_point = (int)Mathf.Floor(height/ 0.1f);

                    valid_ang = clamp_ang - incline_deadzone;
                    valid_level = max_v - min_v + 1;
                    vib_level = (int)Mathf.Floor((valid_ang * valid_level / (5 - incline_deadzone))) + min_v;
                    if (vib_level > max_v) { vib_level = max_v; }

                    vib_width = 3;

                    res = 0;
                    x_1 = 0;

                    if (angle >= 337.5 || angle < 22.5) {
                        /*오른쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ( 11- x >= start_point && 11 -x < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 22.5 && angle < 67.5) {
                        /*우측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + y < start_point + vib_width && (11 - x) + y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }
                    else if (angle >= 67.5 && angle < 112.5) {
                        /*위*/
                        start_point = (int) Mathf.Floor(start_point * 2 / 3);
                        if(start_point > 6) { start_point = 6; }
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (y >= start_point && y < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 112.5 && angle < 157.5) {
                        /*좌측 위*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + y < vib_width + start_point && x+ y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 157.5 && angle < 202.5) {
                        /*왼쪽*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x >= start_point && x < start_point + vib_width) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)0;
                                }
                            }
                        }

                    }
                    else if (angle >= 202.5 && angle < 247.5) {
                        /*좌측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if (x + (17 - y) < vib_width + start_point && x + 17 - y >= start_point) { res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                    rarray[y * 6 + (x / 2)] = (byte)0;
                                }
                            }
                        }
                    }
                    else if (angle >= 247.5 && angle < 292.5) {
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
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }

                    }
                    else if (angle >= 292.5 && angle < 337.5) {
                        /*우측 아래*/
                        for (int y = 0; y < 18; y++) {
                            for (int x = 0; x < 12; x++) {
                                if ((11 - x) + (17 - y) < vib_width + start_point && (28 - x- y) >= start_point ){ res = vib_level; }
                                else { res = 0; }
                                if (x % 2 == 0) { x_1 = res; }
                                else {
                                    larray[y * 6 + (x / 2)] = (byte)0;
                                    rarray[y * 6 + (x / 2)] = (byte)(x_1 * 6 + res);
                                }
                            }
                        }
                    }

                    break;


                case Choice.InclineHeight_Custom:
                    bool[] under;


                    break;


                case Choice.InclineHeight_Semi:
                    height = transform.position.y * (2/3);
                    if (height < 0) { height = 0; }
                    if(height > 0.6f) { height = 0.6f; }
                    start_point = (int)Mathf.Floor(height / 0.1f);

                    valid_ang = clamp_ang - incline_deadzone;
                    valid_level = max_v - min_v + 1;
                    vib_level = (int)Mathf.Floor((valid_ang * valid_level / (5 - incline_deadzone))) + min_v;
                    if (vib_level > max_v) { vib_level = max_v; }

                    vib_width = (int)Mathf.Floor(clamp_ang * (max_width + 1) / 5);
                    if (vib_width > max_width) { vib_width = max_width; }

                    res = 0;
                    x_1 = 0;

                    break;

            }
            
        }
        //printArray(larray);
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
        else { rarray = arr; }
}
    void GrassEffect() {
        for(int i = 0; i< 6; i++) {
            grass[i] = Random.Range(i * 3, i* 3 + 2);
        }
    }

    bool[] CheckHeight(int length, int diagonal) {
        bool[] under_height = new bool[length];
        Vector3 localPos = new Vector3(0, collide_height, 0);
        Vector3 worldPos = transform.TransformPoint(localPos);

        if(length == 18) {
            //세로 체크
            // -2 ~ 2: -2가 앞임.
            for(int i = 0; i< 18; i++) {
                float boat_x = -4 * (i + 0.5f) / 18 + 2;
                Vector3 boatPos = new Vector3(boat_x, 0.18f, 0);
                Vector3 boatWld = transform.TransformPoint(boatPos);
                if(boatWld.y < worldPos.y) {
                    under_height[i] = true;
                }
                else {
                    under_height[i] = false;
                }
            }
        
        }
        else if(length == 24) {
            // 가로 체크 -> 근데 가로가 이렇게 많이 나눌 가치가 있나..? 너무 specific 할 것 같음. z가 -0.75 ~ 0.75
            //0.75가 오른쪽
            for(int i = 0; i< 24; i++) {
                float boat_z = -1.5f * (i + 0.5f) / 24 + 0.75f;
                Vector3 boatPos = new Vector3(0, 0.18f, boat_z);
                Vector3 boatWld = transform.TransformPoint(boatPos);
                if (boatWld.y < worldPos.y) {
                    under_height[i] = true;
                }
                else {
                    under_height[i] = false;
                }
            }
            

        }
        else if(length == 21) {
            Vector3 boatPos;
            //대각선? 가로세로 0.7씩 하는 게 맞음.
            for(int i= 0; i<21; i++) {
                float boat_d = -1.4f * (i + 0.5f) / 21 + 0.7f;
                if(diagonal == 0) {
                    //우측 상단으로 
                    boatPos = new Vector3(- boat_d, 0.18f, boat_d);
                }
                else {
                    //좌측 상단으로 
                    boatPos = new Vector3(boat_d, 0.18f, boat_d);
                }
                Vector3 boatWld = transform.TransformPoint(boatPos);
                if (boatWld.y < worldPos.y) {
                    under_height[i] = true;
                }
                else {
                    under_height[i] = false;
                }
            }
            
        }
        return under_height;
    }


    void Update() {
        Vector3 rotation = transform.eulerAngles;
        if (rotation.x > 180){rotation.x -= 360;}
        rotation.x = Mathf.Clamp(rotation.x, -40f, 40f);
        transform.eulerAngles = rotation;

        //아두이노로 보트 조작
        if (input_d.left_y != 0) {
            if (input_d.left_y > 0 && !input_d.reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.04f * input_d.left_y);
                rigidbody.AddTorque(0, 0.01f * input_d.left_y, 0);
                //rigidbody.AddTorque(0.005f * input_d.left_y, 0, 0);
            }
            else if(input_d.left_y <0 && input_d.reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.04f * input_d.left_y);
                rigidbody.AddTorque(0, -0.01f * input_d.left_y, 0);
            }
            lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, boat.transform.forward, (input_d.left_y / 10));
            
        }

        if (input_d.right_y != 0) {
            //Debug.Log("Right " + rserial.x + ", " + input_d.right_y);
            if (input_d.right_y > 0 && !input_d.reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.08f * input_d.right_y);
                rigidbody.AddTorque(0, -0.01f * input_d.right_y, 0);
                //rigidbody.AddTorque(-0.005f * input_d.right_y, 0, 0);
            }
            else if (input_d.right_y < 0 && input_d.reverse) {
                rigidbody.AddForce(-1 * transform.right * 0.08f * input_d.right_y);
                rigidbody.AddTorque(0, -0.01f * input_d.right_y, 0);
            }
            //노 회전 애니메이션
            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, boat.transform.forward, (input_d.right_y / 10));

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
        Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
        Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
        float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
        Debug.Log(transform.position.y);
        float c_speed = Mathf.Clamp(collide_speed, 0, 3);
        c_speed /= 3;
        if (direct_ang < 0) { direct_ang += 360; }
    
        float bef_coll = collide;
        if (underwater.underwater) {
            collide = 1.5f;
            float currentPositionY = front.position.y;
            float diff = underwater.water_y - currentPositionY;
            ang = diff;
        }
        else {
            collide = bef_coll;
            if(bef_coll == 1.5f) {
                collide = 2.0f;
            }
        }
        
        if(collide == 0f || collide ==2f) {
            if(input_d.zerostream > 50) { collide = 0f; }
            else { collide = 2.0f; }
        }
        

        if(collide<2.0f) {updateArray(collide, direct_ang,  ang, collide_ang, c_speed); } // 그 외: 충돌, 물에 빠짐, 출렁임
        else { // 노 젓기
            updateEachArray(true, rigidbody.velocity.magnitude, input_d.reverse, input_d.sum_l, water_status); //left hand
            updateEachArray(false, rigidbody.velocity.magnitude, input_d.reverse, input_d.sum_r, water_status); //right hand
        }
        

        
        if (max_incline < ang) {
            max_incline = ang;
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
        if (!c.collider.CompareTag("Water") && !c.collider.CompareTag("Grass")) {
            if (collide_land && c.collider.CompareTag("Land")) { }
            else {
                StartCoroutine(CollisionControl());
                water_status = 0f;
            }
            
        }
        else {
            if (c.collider.CompareTag("Land")) {
                Debug.Log("collide with land");
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

        Vector3 colVelocity = c.relativeVelocity;
        float colForce = colVelocity.magnitude;
        collide_speed = colVelocity.magnitude;

        float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        angle *= -1;
        if (angle < 0) {
            angle += 360;
        }
        collide_ang = angle;

    }

    private void OnCollisionStay(Collision c) {
        if (c.collider.CompareTag("Land")){
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
        }
        
    }

    void OnCollisionExit(Collision c) {
        collide = 0f;
        water_status = 0f;
        if (c.collider.CompareTag("Land")) {
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




}