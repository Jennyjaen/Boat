﻿using System.Collections.Generic;
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


    private bool collide_land;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    [HideInInspector]
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private float collide;
    private float collide_ang;
    private float collide_speed;

    private float incline_deadzone = 1.8f;
    private int max_v = 5;
    private int min_v = 1;
    private int max_width = 9;
    private int min_width = 3;

    private bool grassboat;
    private int enterCount = 0;
    private bool grassin = false;
    private int[,] left_grass = new int[12, 92];
    private int[,] right_grass = new int[12, 92];
    private bool grass_front = false;

    [HideInInspector]
    public bool waterincline = false;
    private int waterfall = 0;
    private GamePadState state;

    private string land_name;
    private Vector3 land_down;

    private VerticalCheck left_boat;
    private VerticalCheck right_boat;
    private WaterBump waterbump;

    private float moving_time; //처음 부딪혔을 때 세게 부딪히는 것 확인.
    private bool cancollide = true;
    //Input 방법을 여러개로 바꾸기
    public enum InputMethod {
        GamePad,
        HandStickThrottle,
        HandStickGesture, 
        GestureThrottle
    }
    public InputMethod inputMethod;
    [SerializeField] private bool disableWater = false;
    public enum Track {
        Practice,
        Track1,
        Track2
    }

    public Track track;

    [HideInInspector]
    public GamePadInput GamePadInput;
    [HideInInspector]
    public HandThrottle HandThrottle;
    [HideInInspector]
    public HandGesture HandGesture;

    private float max_incline;

    [HideInInspector]
    public byte[] larray = new byte[108];
    [HideInInspector]
    public byte[] rarray = new byte[108];

    private Transform triggered;

    private float bouncy_time;
    private bool from_zero; //update 주기로 인해 충돌할때 시작점이 0이 아닐때가 있음.

    void Awake() {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
        front = transform.Find("Front");
    }

    void Start() {
        from_zero = false;
        moving_time = 0;
        land_name = "";
        land_down = new Vector3(0, 0, 0);
        max_incline = 0f;
        collide = 0f;
        collide_ang = 0f;
        collide_speed = 0f;
        water_status = 0;
        collide_land = false;
        bouncy_time = 0;

        underwater = transform.Find("Front").GetComponent<Underwater>();
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        grass = new int[6];

        GamePadInput = GetComponent<GamePadInput>();
        HandThrottle = GetComponent<HandThrottle>();
        HandGesture = GetComponent<HandGesture>();
        
        grassboat = false;
        for (int i = 0; i < 108; i++) {
            larray[i] = (byte)0;
            rarray[i] = (byte)0;

        }
        InitArray();
        MarkArray(left_grass, GeneratePoints());
        MarkArray(right_grass, GeneratePoints());
        triggered = null;
        waterbump = GetComponent<WaterBump>();
        left_boat = GameObject.Find("LeftVertical").transform.GetComponent<VerticalCheck>();
        right_boat = GameObject.Find("RightVertical").transform.GetComponent<VerticalCheck>();


        switch (track) {
            case Track.Practice:
                GameObject practiceObject = GameObject.Find("Practice");
                    if (practiceObject != null) {
                        Transform practicePoint = practiceObject.transform.Find("Point");
                        if (practicePoint != null) {
                            transform.position = practicePoint.position;
                        }
                    }
                break;
            case Track.Track1:
                practiceObject = GameObject.Find("Track1");
                if (practiceObject != null) {
                    Transform practicePoint = practiceObject.transform.Find("Point");
                    if (practicePoint != null) {
                        transform.position = practicePoint.position;
                    }
                }
                Vector3 currentRotation = transform.eulerAngles;
                currentRotation.y = 160f;
                transform.eulerAngles = currentRotation;
                break;

            case Track.Track2:
                practiceObject = GameObject.Find("Track2");
                if (practiceObject != null) {
                    Transform practicePoint = practiceObject.transform.Find("Point");
                    if (practicePoint != null) {
                        transform.position = practicePoint.position;
                    }
                }
                currentRotation = transform.eulerAngles;
                currentRotation.y = 160f;
                transform.eulerAngles = currentRotation;
                break;
        }
    }

    void InitArray() {
        for(int i = 0; i < left_grass.GetLength(0); i++) {
            for(int j= 0; j< right_grass.GetLength(1); j++) {
                left_grass[i, j] = 0;
                right_grass[i, j] = 0;
            }
        }
    }

    List<Vector2Int> GeneratePoints() {
        List<Vector2Int> points = new List<Vector2Int>();
        int previousX = -1;

        for (int i = 0; i < 9; i++) {
            int x;
            do {
                x = Random.Range(0, 3); // 가로 0, 1, 2 중 하나 선택
            } while (x == previousX); // 이전 점의 가로 좌표와 다르게 선택

            previousX = x;

            // 3. 가로에 따른 세로 좌표 범위 설정
            int yRangeStart = 6 * i + 19;
            int yRangeEnd = 6 * i + 23;
            int y = Random.Range(yRangeStart, yRangeEnd);

            int specificX;
            if (x == 0)
                specificX = Random.Range(1, 4); // 가로가 0일 때 1~3 중 선택
            else if (x == 1)
                specificX = Random.Range(4, 8); // 가로가 1일 때 4~7 중 선택
            else
                specificX = Random.Range(8, 12); // 가로가 2일 때 8~10 중 선택

            points.Add(new Vector2Int(specificX, y));
        }

        return points;
    }

    void MarkArray(int[,] targetArray, List<Vector2Int> points) {
        foreach (var point in points) {
            int startX = Mathf.Max(0, point.x - 1);
            int endX = Mathf.Min(targetArray.GetLength(0) - 1, point.x);
            int startY = Mathf.Max(0, point.y - 1);
            int endY = Mathf.Min(targetArray.GetLength(1) - 1, point.y + 1);

            for (int x = startX; x <= endX; x++) {
                for (int y = startY; y <= endY; y++) {
                    targetArray[x, y] = 4;
                }
            }
        }
        //Print12Array(targetArray);
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

            //float intensity = Mathf.Ceil(col_s * 5) / 5;
            float intensity = col_s * 18;
            intensity = Mathf.Round(intensity);
            intensity = Mathf.Clamp(intensity, 3, 5);
            int width = Mathf.FloorToInt(col_s * 6) + 5;
            float time_consume = Time.time - bouncy_time;

            if (time_consume <= 0.1f && time_consume > 0.05f) {
                //intensity += 1;
                intensity -= 1;
            }
            else if (time_consume <= 0.2f) {
                //width -= 2;
                intensity -= 2;
            }
            else {
                //width -= 3;
                intensity -= 3;
            }


            if (intensity > 5) {
                intensity = 5;
            }
            if (intensity < 3) {
                intensity = 3;
            }
            for (int y = 0; y < 18; y++) {
                for (int x = 0; x < 24; x++) {
                    float cent_x = ((float)x + 0.5f) / 24;
                    float cent_y = ((float)y + 0.5f) / 18;
                    float res = 0;

                    if (col_ang >= 22.5 && col_ang < 67.5) {
                        //Debug.Log("ru");
                        int col_pos;
                        
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.005f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.007f) + 20;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.014f) + 34;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }

                                if (y - x >= -24 + col_pos && y - x < -24 + col_pos + width) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.007f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.01f) + 14;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.017f) + 24;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                if (y - x < -24 + col_pos +12) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        
                    }
                    else if (col_ang >= 67.5 && col_ang < 112.5) {
                        //위
                        //Debug.Log("up");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.011f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.016f) + 9;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.033f) + 15;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }

                                if (y >= col_pos && y < col_pos + width) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.02f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.05f) + 5;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.05f) + 7;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }

                                if (y < col_pos +9) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        

                    }
                    else if (col_ang >= 112.5 && col_ang < 157.5) {
                        //Debug.Log("lu");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.005f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.007f) + 20;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.014f) + 34;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }

                                //Debug.Log(col_pos + " , "+ width);
                                if (y + x >= col_pos && y + x < col_pos + width) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.007f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.01f) + 14;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.017f) + 24;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }

                                //Debug.Log(col_pos + " , "+ width);
                                if (y + x < col_pos + 12) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        
                        
                    }
                    else if (col_ang >= 157.5 && col_ang < 202.5) {
                        //왼쪽
                        //Debug.Log("Left");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.0083f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.0125f) + 12;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.025f) + 20;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                if (x >= col_pos && x < col_pos + width) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.0125f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.02f) + 8;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.033f) + 13;
                                }

                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                if (x < col_pos + 8) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        
                    }
                    else if (col_ang >= 202.5 && col_ang < 247.5) {
                        //Debug.Log("ld");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.005f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.007f) + 20;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.014f) + 34;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (y - x < 18 - col_pos && y - x >= 18 - (col_pos + width)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.007f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.01f) + 14;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.017f) + 24;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (y - x >= 18 - (col_pos + 12)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        
                    }
                    else if (col_ang >= 247.5 && col_ang < 292.5) {
                        //아래
                        //Debug.Log("down");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.011f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.016f) + 9;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.033f) + 15;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (y < 18 - col_pos && y >= 18 - (col_pos + width)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.02f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.05f) + 5;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.05f) + 7;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (y >= 18 - (col_pos + 9)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }      
                                break;
                        }
                        
                    }
                    else if (col_ang >= 292.5 && col_ang < 337.5) {
                        //Debug.Log("rd");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.005f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.007f) + 20;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.014f) + 34;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (x + y < 42 - col_pos && x + y >= 42 - (col_pos + width)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.007f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.01f) + 14;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.017f) + 24;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if ( x + y >= 42 - (col_pos + 12)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        
                    }
                    else {
                        //오른쪽?
                        //Debug.Log("right");
                        int col_pos;
                        switch (inputMethod) {
                            case InputMethod.HandStickThrottle:
                            case InputMethod.GestureThrottle:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.0083f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.0125f) + 12;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.025f) + 20;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (x < 24 - col_pos && x >= 24 - (col_pos + width)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                            case InputMethod.HandStickGesture:
                                if (time_consume <= 0.1f) {
                                    col_pos = Mathf.FloorToInt(time_consume / 0.0125f);
                                }
                                else if (time_consume <= 0.2f) {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.1f) / 0.02f) + 8;
                                }
                                else {
                                    col_pos = Mathf.FloorToInt((time_consume - 0.2f) / 0.033f) + 13;
                                }
                                if (col_pos == 0) {
                                    from_zero = true;
                                }
                                if (!from_zero) {
                                    col_pos = 0;
                                    from_zero = true;
                                }
                                //Debug.Log(col_pos + " , "+ width);
                                if (x >= 24 - (col_pos + 8)) {
                                    res = (int)intensity;
                                }
                                else {
                                    res = 0;
                                }
                                break;
                        }
                        
                    }

                    if (x % 2 == 0) { x_1 = (int)res; }
                    else {
                        x_2 = (int)res;
                        if (x >= 12) {
                            int index = y * 6 + Mathf.FloorToInt((x - 12) / 2);
                            rarray[107 - index] = (byte)(x_1 + x_2 * 6); //rarray 뒤집었음.
                        }
                        else {
                            int index = y * 6 + Mathf.FloorToInt(x / 2);
                            larray[index] = (byte)(x_1 * 6 + x_2);
                        }
                    }

                }
            }
        }
        else if (collide == 1.5f) { // 물에 빠졌을 때
            byte[] arr = new byte[108];
            int sero = (int)(clamp_ang / 0.03f);
            for (int y = 0; y < 18; y++) {
                for (int n = 0; n < 6; n++) {
                    if (y >= 0 && y <= sero) {
                        arr[y * 6 + n] = (byte)(5 * 6 + 5);
                    }
                    else { arr[y * 6 + n] = (byte)0; }
                }
            }
            larray = (byte[])arr.Clone();
            System.Array.Reverse(arr); //오른쪽 뒤집었음.
            rarray = arr;
        }
        else if (collide == 0f) {// 기울기: 0f
            if (clamp_ang < incline_deadzone) {
                //Debug.Log("deadzone");
                for (int i = 0; i < 108; i++) {
                    larray[i] = (byte)0;
                    rarray[i] = (byte)0;
                }
                return;
            }

            float valid_ang = clamp_ang - incline_deadzone;
            if (waterfall > 0 && waterincline) {
                valid_ang = clamp_ang - 16;
            }
            int vib_level;
            int vib_width;
            Vector3 localvel = transform.InverseTransformDirection(rigidbody.velocity);
            float velo = Mathf.Sqrt(localvel.y * localvel.y + localvel.z * localvel.z);
            if (velo > 0.1) {
                vib_level = Mathf.FloorToInt(valid_ang / 1.8f) +1;
                if (vib_level >3) { vib_level =3; }
            }
            else { //속도 느릴때: 이때 max magnitude를 3으로 한정하는 것이 나을 것 같음.

                if (valid_ang < 0) { vib_level = 0; }
                else {
                    vib_level = Mathf.FloorToInt(valid_ang) + 1;
                    switch (inputMethod) {
                        case InputMethod.GamePad:
                            if (vib_level > 2) { vib_level =2; }
                            break;
                        default:
                            if (vib_level > 3) { vib_level = 3; }
                            break;
                    }
                    
                }
            }
            float height = transform.position.y;
            if (height < 0) { height = 0; }
            vib_width = Mathf.FloorToInt(30f * height) + min_width;
            if (vib_width > max_width) { vib_width = max_width; }

            if (waterfall > 0) {
                vib_width = 6;
            }
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
                //Debug.Log("left down");
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
                            rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 + res * 6);
                        }
                    }
                }

            }
            else if (angle >= 112.5 && angle < 157.5) {
                /*우측 아래*/
                //Debug.Log("right down");
                for (int y = 0; y < 18; y++) {
                    for (int x = 0; x < 12; x++) {
                        if ((11 - x) + (17 - y) < vib_width) { res = vib_level; }
                        else { res = 0; }
                        if (x % 2 == 0) { x_1 = res; }
                        else {
                            larray[y * 6 + (x / 2)] = (byte)0;
                            rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 + res * 6);
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
                            rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 + res * 6);
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
                            rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 + res * 6);
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
                            rarray[107 - (y * 6 + (x / 2))] = (byte)(x_1 + res * 6);
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

        }
        else if (collide == 1.3f) { //Moving Object용
            int x_1 = 0;
            int x_2 = 0;

            float intensity = 3;
            if (Time.time - moving_time < 0.3f) {
                intensity = 5;
            }
            float width = 0.2f;
            float adj_ang = 20f;
            for (int y = 0; y < 18; y++) {
                for (int x = 0; x < 24; x++) {
                    float cent_x = ((float)x + 0.5f) / 24;
                    float cent_y = ((float)y + 0.5f) / 12;
                    float res;
                    if (col_ang >= 90 && col_ang < 270 - adj_ang) {
                        //왼쪽
                        if (cent_x < width) {
                            res = intensity;
                            if (collide_speed > 13) { //collide speed가 중앙으로부터 거리로 매핑됨
                                int upper = Mathf.FloorToInt((collide_speed - 13) / 0.2f);
                                if (y <= upper) { res = 0; }
                            }
                        }
                        else { res = 0; }
                        switch (inputMethod) {
                            case InputMethod.HandStickGesture:
                                //우측 손이 움직일 때 움직이는 피드백 필요
                                int sero = Mathf.CeilToInt(input_d.sum_r / 80);
                                if (x > 11) { //오른 손
                                    if (input_d.r_reverse) { //뒤로 저을 때, 우상에서 좌하로 가야함
                                        if((23 - x)+ y > sero && (23 - x + y) <= sero + 4) {
                                            res = 4;
                                        }
                                    }
                                    else { //앞으로 갈 때
                                        if ((23 - x) + (17-y) > sero && (23 - x + 17- y) <= sero + 4) {
                                            res = 4;
                                        }
                                    }
                                }

                                break;
                        }
                    }
                    else if (col_ang >= 270 - adj_ang && col_ang < 270 + adj_ang) {
                        //아래
                        //Debug.Log("down");
                        if (cent_y >= 1 - width) {
                            res = intensity;
                            if (collide_speed > 15.3) {
                                int upper = Mathf.FloorToInt((collide_speed - 15.3f) / 0.05f); //24칸임
                                if (x <= upper) { res = 0; }
                            }
                        }
                        else { res = 0; }

                        switch (inputMethod) {
                            case InputMethod.HandStickGesture:
                                int sero = Mathf.CeilToInt(input_d.sum_l / 80);
                                if (input_d.l_reverse) { //뒤로 저을 때, 우상에서 좌하로 가야함
                                    if (y > sero &&  y <= sero + 4) {
                                        res = 4;
                                    }
                                }
                                else { //앞으로 갈 때
                                    if ((17 - y) > sero && (17 - y) <= sero + 4) {
                                        res = 4;
                                    }
                                }
                                sero = Mathf.CeilToInt(input_d.sum_r / 80);
                                if (input_d.r_reverse) { //뒤로 저을 때, 우상에서 좌하로 가야함
                                    if (y > sero && y <= sero + 4) {
                                        res = 4;
                                    }
                                }
                                else { //앞으로 갈 때
                                    if ((17 - y) > sero && (17 - y) <= sero + 4) {
                                        res = 4;
                                    }
                                }
                                break;
                        }
                    }
                    else {
                        //오른쪽
                        //Debug.Log("right");
                        if (cent_x >= 1 - width) {
                            res = intensity;
                            if (collide_speed > 13) {
                                int upper = Mathf.FloorToInt((collide_speed - 13) / 0.2f);
                                if (y >= 17 - upper) { res = 0; }
                            }
                        }
                        else { res = 0; }
                        switch (inputMethod) {
                            case InputMethod.HandStickGesture:
                                //우측 손이 움직일 때 움직이는 피드백 필요
                                int sero = Mathf.CeilToInt(input_d.sum_l / 80);
                                if (x <= 11) { //왼손
                                    if (input_d.l_reverse) { //뒤로 저을 때, 우상에서 좌하로 가야함
                                        if (x + y > sero && (x + y) <= sero + 4) {
                                            res = 4;
                                        }
                                    }
                                    else { //앞으로 갈 때
                                        if ( x+ (17 - y) > sero && ( x + 17 - y) <= sero + 4) {
                                            res = 4;
                                        }
                                    }
                                }

                                break;
                        }
                    }

                    if (x % 2 == 0) { x_1 = (int)res; }
                    else {
                        x_2 = (int)res;
                        if (x >= 12) {
                            int index = y * 6 + ((x - 12) / 2);
                            rarray[107 - index] = (byte)(x_1 + x_2 * 6); //rarray 뒤집었음.
                        }
                        else {
                            int index = y * 6 + (x / 2);
                            larray[index] = (byte)(x_1 * 6 + x_2);
                        }
                    }
                }
            }
            //printArray(rarray);
        }
        else if (collide == 1.8f) {
            float rotationZ = transform.rotation.eulerAngles.z;
            if (rotationZ > 180) {
                rotationZ -= 360;
            }
            float intensity = (rotationZ + 19f) / 20f;
            intensity = Mathf.Clamp(intensity, 0, 1);
            intensity *= 5;
            int intense = Mathf.CeilToInt(intensity);
            int start_row = Mathf.CeilToInt(Mathf.Clamp((col_s - 2.9f) / 0.04f, 0, 12));
            start_row += 3; //3
            int width = Mathf.Clamp(Mathf.CeilToInt((rotationZ + 19f) * 0.6f), 0, 15);
            //Debug.Log($"water: {start_row} ~ {width}, intensity: {intense}");
            for (int y = 0; y < 18; y++) {
                for (int x = 0; x < 6; x++) {
                    if (y >= start_row) {
                        larray[y * 6 + x] = 0;
                        rarray[(17 - y) * 6 + x] = 0;
                    }
                    else if (y < start_row - width) {
                        larray[y * 6 + x] = 0;
                        rarray[(17 - y) * 6 + x] = 0;
                    }
                    else {
                        larray[y * 6 + x] = (byte)(intense * 7);
                        rarray[(17 - y) * 6 + x] = (byte)(intense * 7);
                    }
                }
            }
        }
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
    int[,] SliceArray(int[,] originalArray, int startX, int startY, int rows, int cols) {
        int[,] result = new int[rows, cols];

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                //Debug.Log($"Position is: {startX + i} , {startY + j}");
                result[i, j] = originalArray[startX + i, startY + j];
            }
        }

        return result;
    }

    byte[] int2byteArray(int[,] array, float speed, bool reverse) {
        byte[] result = new byte[108];
        for (int y = 0; y < 18; y++) {
            for (int x = 0; x < 6; x++) {
                if (reverse) {
                    result[107 - (y * 6 + x)] = (byte)(Mathf.CeilToInt(array[x * 2, y] * speed) + Mathf.CeilToInt(array[x * 2 + 1, y] * speed) * 6);
                }
                else {
                    result[y * 6 + x] = (byte)(Mathf.CeilToInt(array[x * 2, y] * speed) * 6 + Mathf.CeilToInt(array[x * 2 + 1, y] * speed));
                }

            }
        }

        return result;
    }

    public float CalculateDistanceToPlane(Transform planeTransform, Transform objectTransform) {
        // 평면의 법선 벡터
        Vector3 planeNormal = planeTransform.up;

        // 평면의 기준점
        Vector3 planePoint = planeTransform.position;

        // 물체 위치
        Vector3 objectPosition = objectTransform.position;

        // 물체에서 평면까지의 벡터
        Vector3 vectorToPoint = objectPosition - planePoint;

        // 수직 거리 계산 (벡터와 법선 벡터의 내적)
        float distance = Vector3.Dot(vectorToPoint, planeNormal);

        return distance;
    }

    void updateEachArray(bool isLeft, float vel, bool reverse, int sum, float water) {
        //water: 0 둘다 일반 노젓기 1: 왼쪽 땅, 1.5: 양쪽 땅, 2: 오른쪽 땅, 3: 풀 위
        //water incline
        byte[] arr = new byte[108];

        switch (inputMethod) {
            case InputMethod.HandStickThrottle:
                float moving = Mathf.Abs(HandThrottle.ly) + Mathf.Abs(HandThrottle.ry);
                moving /= 2;
                int intense = Mathf.CeilToInt(3 * moving);
                float t = Time.time;
                if (water == 1) {
                    for (int y = 0; y < 18; y++) {
                        if (left_boat.collidingChildIndices.Contains(y + 1)) {
                            for (int x = 0; x < 6; x++) {
                                if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                    larray[y * 6 + x] = (byte)(intense * 7);
                                }
                                else {
                                    if (Random.Range(0, 5) > 1) { larray[y * 6 + x] = (byte)0; }
                                    else { larray[y * 6 + x] = (byte)(intense * 7); }
                                }
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }

                    }
                }
                else if (water == 1.5f) {
                    for (int y = 0; y < 18; y++) {
                        if (left_boat.collidingChildIndices.Contains(y + 1)) {
                            for (int x = 0; x < 6; x++) {
                                if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                    larray[y * 6 + x] = (byte)(intense * 7);
                                }
                                else {
                                    if (Random.Range(0, 5) > 1) { larray[y * 6 + x] = (byte)0; }
                                    else { larray[y * 6 + x] = (byte)(intense * 7); }
                                }
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                            }
                        }
                        if (right_boat.collidingChildIndices.Contains(18 - y)) {
                            for (int x = 0; x < 6; x++) {
                                if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                    rarray[y * 6 + x] = (byte)(intense * 7);
                                }
                                else {
                                    if (Random.Range(0, 5) > 1) { rarray[y * 6 + x] = (byte)0; }
                                    else { rarray[y * 6 + x] = (byte)(intense * 7); }
                                }
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }

                    }
                }
                else if (water == 2f) {
                    for (int y = 0; y < 18; y++) {
                        if (right_boat.collidingChildIndices.Contains(18 - y)) {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                                if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                    rarray[y * 6 + x] = (byte)(intense * 7);
                                }
                                else {
                                    if (Random.Range(0, 5) > 1) { rarray[y * 6 + x] = (byte)0; }
                                    else { rarray[y * 6 + x] = (byte)(intense * 7); }
                                }
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }
                    }
                    }
                else if(water == 3f) {
                    int boat_pos = 0;
                    if (triggered != null) {
                        float boat = CalculateDistanceToPlane(triggered, transform);
                        //Debug.Log(boat);
                        boat_pos = Mathf.FloorToInt((- boat + 2.43f) * 2);
                        if (boat_pos < 0) { boat_pos = 0; }
                        //Debug.Log(boat_pos);
                        if (!grass_front) {
                            boat_pos = 74 - boat_pos;
                        }
                    }
                    //Debug.Log($"{transform.position.z} is here so boat pos is {boat_pos}");

                    int[,] left_slice = SliceArray(left_grass,0, boat_pos, 12, 18);
                    int[,] right_slice = SliceArray(right_grass,0, boat_pos,12, 18);
                    larray = int2byteArray(left_slice, moving, true);
                    rarray = int2byteArray(right_slice, moving, false);
                    
                }
                break;
            case InputMethod.GestureThrottle:
                moving = rigidbody.velocity.magnitude/3;
                intense = Mathf.Clamp(Mathf.CeilToInt(moving), 0, 3);
                if (Mathf.Abs(input_d.sum_l) < 10 && Mathf.Abs(input_d.sum_r) < 10) {
                    moving = 0;
                    intense = 0;
                }
                t = Time.time;
                if (water == 1) {
                    for (int y = 0; y < 18; y++) {
                        if (left_boat.collidingChildIndices.Contains(y + 1)) {
                            for (int x = 0; x < 6; x++) {
                                if ((reverse && input_d.sum_l < 0) || (!reverse && input_d.sum_l > 0)) {
                                    if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                        larray[y * 6 + x] = (byte)(intense * 7);
                                    }
                                    else {
                                        if (Random.Range(0, 5) > 1) { larray[y * 6 + x] = (byte)0; }
                                        else { larray[y * 6 + x] = (byte)(intense * 7); }
                                    }
                                }
                                else {
                                    larray[y * 6 + x] = (byte)0;
                                }
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }

                    }
                }
                else if (water == 1.5f) {
                    for (int y = 0; y < 18; y++) {
                        if (left_boat.collidingChildIndices.Contains(y + 1)) {
                            for (int x = 0; x < 6; x++) {
                                if ((reverse && input_d.sum_l < 0) || (!reverse && input_d.sum_l > 0)) {
                                    if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                        larray[y * 6 + x] = (byte)(intense * 7);
                                    }
                                    else {
                                        if (Random.Range(0, 5) > 1) { larray[y * 6 + x] = (byte)0; }
                                        else { larray[y * 6 + x] = (byte)(intense * 7); }
                                    }
                                }
                                else { larray[y * 6 + x] = (byte)0; }
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                            }
                        }
                        if (right_boat.collidingChildIndices.Contains(18 - y)) {
                            for (int x = 0; x < 6; x++) {
                                if ((reverse && input_d.sum_r < 0) || (!reverse && input_d.sum_r > 0)) {
                                    if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                        rarray[y * 6 + x] = (byte)(intense * 7);
                                    }
                                    else {
                                        if (Random.Range(0, 5) > 1) { rarray[y * 6 + x] = (byte)0; }
                                        else { rarray[y * 6 + x] = (byte)(intense * 7); }
                                    }
                                }
                                else {
                                    rarray[y * 6 + x] = (byte)0;
                                }
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }

                    }
                }
                else if (water == 2f) {
                    for (int y = 0; y < 18; y++) {
                        if (right_boat.collidingChildIndices.Contains(18 - y)) {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                                if ((reverse && input_d.sum_r < 0) || (!reverse && input_d.sum_r > 0)) {
                                    if (t * 100 - Mathf.Floor(t * 100) > 0.5f) {
                                        rarray[y * 6 + x] = (byte)(intense * 7);
                                    }
                                    else {
                                        if (Random.Range(0, 5) > 1) { rarray[y * 6 + x] = (byte)0; }
                                        else { rarray[y * 6 + x] = (byte)(intense * 7); }
                                    }
                                }
                                else {
                                    rarray[y * 6 + x] = (byte)0;
                                }
                            }
                        }
                        else {
                            for (int x = 0; x < 6; x++) {
                                larray[y * 6 + x] = (byte)0;
                                rarray[y * 6 + x] = (byte)0;
                            }
                        }
                    }
                }
                else if (water == 3f) {
                    int boat_pos = 0;
                    if (triggered != null) {
                        float boat = CalculateDistanceToPlane(triggered, transform);
                        //Debug.Log(boat);
                        boat_pos = Mathf.FloorToInt((-boat + 2.43f) * 2);
                        if (boat_pos < 0) { boat_pos = 0; }
                        //Debug.Log(boat_pos);
                        if (!grass_front) {
                            boat_pos = 74 - boat_pos;
                        }
                    }
                    //Debug.Log($"{transform.position.z} is here so boat pos is {boat_pos}");

                    int[,] left_slice = SliceArray(left_grass, 0, boat_pos, 12, 18);
                    int[,] right_slice = SliceArray(right_grass, 0, boat_pos, 12, 18);
                    larray = int2byteArray(left_slice, moving, true);
                    rarray = int2byteArray(right_slice, moving, false);

                }
                break;

            case InputMethod.HandStickGesture: // 노젓기
                int max_vib = 0;
                int width = 4;
                int sero = Mathf.Abs(sum) / 120;

                if (Mathf.Abs(sum )> 10) {
                    if (isLeft && water == 1) {
                        width = 8;
                        sero = Mathf.Abs(sum) / 200;
                        if(left_boat.collidingChildIndices.Contains(sero + 1)) {
                            max_vib = 5;
                        }
                        else {
                            max_vib = 3;
                        }
                     }
                    else if (!isLeft && water == 2) {
                        width = 8;
                        sero = Mathf.Abs(sum) / 200;
                        if (right_boat.collidingChildIndices.Contains(sero + 1)) {
                            max_vib = 5;
                        }
                        else {
                            max_vib = 3;
                        }
                    }
                    else if (water == 1.5f) {
                        width = 8;
                        sero = Mathf.Abs(sum) / 200;
                        if (isLeft) {
                            if (left_boat.collidingChildIndices.Contains(sero + 1)) {
                                max_vib = 5;
                            }
                            else {
                                 max_vib = 3;
                            }
                        }
                        else {
                            if (right_boat.collidingChildIndices.Contains(sero + 1)) {
                                max_vib = 5;
                            }
                            else {
                                max_vib = 3;
                            }
                        }
                    }
                    else { 
                        if (vel <= 5) { max_vib = 3;}
                        else if(vel <=9) { max_vib = 2;}
                        else { max_vib = 1; }    
                    }

                    if (waterincline) {

                        float zRot = transform.rotation.eulerAngles.z;
                        if (zRot > 180) { zRot -= 360; }
                        if (zRot > 15) {
                            sero = Mathf.Abs(sum) / 100;
                            max_vib = 2;
                            width = 2;
                        }
                        else if (zRot < -15) {
                            sero = Mathf.Abs(sum) / 200;
                            max_vib = 4;
                            width = 6;
                        }

                    }
                }

                if (reverse && sum > 0) { max_vib = 0; }
                if(!reverse && sum < 0) { max_vib = 0; }


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
                if (isLeft) { larray = (byte[])arr.Clone(); }
                else { 
                    System.Array.Reverse(arr); //오른쪽 장치 뒤집어서 거꾸로 넣어줘야함.
                    rarray = (byte[])arr.Clone();
                }

                
                break;
        }
        
}
    void GrassEffect() {
        for(int i = 0; i< 6; i++) {
            grass[i] = Random.Range(i * 3, i* 3 + 2);
        }
    }
    void Print12Array(int[,] targetArray) {
        string result = ""; // 전체 배열을 담을 문자열

        for (int i = 0; i < targetArray.GetLength(0); i++) {
            for (int j = 0; j < targetArray.GetLength(1); j++) {
                result += targetArray[i, j] + " ";
            }
            result = result.TrimEnd() + "\n"; // 각 행의 끝에서 공백 제거 후 줄바꿈 추가
        }

        Debug.Log(result.TrimEnd()); // 전체 문자열을 한 번에 출력
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
            case InputMethod.GestureThrottle:
                GamePadInput.enabled = false;
                HandThrottle.enabled = false;
                HandGesture.enabled = true;
                break;
        }

        //최고속도 조절
        if (rigidbody.velocity.magnitude > 10.0f) {
            rigidbody.velocity = rigidbody.velocity.normalized * 10.0f;
        }
        if (rigidbody.angularVelocity.magnitude > 1.0f) {
            rigidbody.angularVelocity = rigidbody.angularVelocity.normalized * 1.0f;
        }

        Vector3 rotation = transform.eulerAngles;
        if (rotation.x > 180){rotation.x -= 360;}
        rotation.x = Mathf.Clamp(rotation.x, -40f, 40f);
        if (rotation.z > 180) { rotation.z -= 360; }
        rotation.z = Mathf.Clamp(rotation.z, -40f, 40f);
        transform.eulerAngles = rotation;
        if (underwater.underwater) {
            waterincline = false;
            waterfall = 0;
        }
        switch (inputMethod) {
            case InputMethod.HandStickGesture:
            case InputMethod.GestureThrottle:
                float zRot = transform.rotation.eulerAngles.z;
                if (zRot > 180) { zRot -= 360; }
                if (zRot < -15) {
                    HandGesture.speed_m = 0.07f;
                }
                else {
                    HandGesture.speed_m = 0.05f;
                    }
                break;
    }
        

        if (left_boat.on_land || right_boat.on_land) { //배가 어딘가 바닥에 부딪힘.
            HandThrottle.front_m = 1.5f;
            HandThrottle.rotation_m = 9f;
            GamePadInput.front_m = 1.5f;
            HandThrottle.rotation_m = 9f;
            collide = 2;
            if (!right_boat) {
                water_status = 1;
            }
            else if (!left_boat) {
                water_status = 2;
            }
            else { water_status = 1.5f; }

        }
        else {
            if(collide ==2 && water_status >=1 && water_status <= 2) {
                collide = 0;
                water_status = 0;
                HandThrottle.front_m = 2.5f;
                HandThrottle.rotation_m = 15f;
                GamePadInput.front_m = 2.5f;
                HandThrottle.rotation_m = 15f;
            }
            else {
                if (water_status != 3 && collide ==2) {
                    HandThrottle.front_m = 2.5f;
                    HandThrottle.rotation_m = 15f;
                    GamePadInput.front_m = 2.5f;
                    HandThrottle.rotation_m = 15f;
                }
            }
        }



        switch (inputMethod) {
            case InputMethod.GamePad: //게임 패드에 햅틱 피드백을 주는 경우
                if (underwater.underwater) {
                    float currentPositionY = front.position.y;
                    float diff = underwater.water_y - currentPositionY;
                    float intensity = Mathf.Clamp(diff * 5, 0, 1);
                    GamePad.SetVibration(PlayerIndex.One, 0, intensity);
                }
                else if (waterbump.start_bump) { //처음에 물이 닿음
                    float rotationZ = transform.rotation.eulerAngles.z;
                    if (rotationZ > 180) {
                        rotationZ -= 360;
                    }
                    float intensity = (rotationZ + 19f) / 20f;
                    intensity = Mathf.Clamp(intensity, 0, 1);
                    GamePad.SetVibration(PlayerIndex.One, 0, intensity);
                }
                else {
                    if (waterbump.enabled) {
                        GamePad.SetVibration(PlayerIndex.One, 0, 0);
                    }
                }
                break;
            case InputMethod.HandStickThrottle:
            case InputMethod.GestureThrottle://장치에 햅틱 피드백을 주는 경우
                Vector3 up_vector = transform.up;
                Vector3 forward_vector = -transform.forward;
                float ang = Vector3.Angle(up_vector, Vector3.up);
                Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
                Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
                float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
                float c_ang = ang;

                float c_speed = Mathf.Clamp(collide_speed, 0, 1);
                if (direct_ang < 0) { direct_ang += 360; }
                float bef_coll = collide;
                if (underwater.underwater) {
                    collide = 1.5f;
                    float currentPositionY = front.position.y;
                    float diff = underwater.water_y - currentPositionY;
                    c_ang = diff;
                    waterbump.enabled = false;
                }
                else {
                    collide = bef_coll;
                    if (bef_coll == 1.5f) {
                        collide = 0f;
                    }
                }

                if (waterbump.enabled) {
                    if (waterbump.start_bump) {
                        collide = 1.8f;
                        c_speed = waterbump.height;
                    }
                    else {
                        if (collide != 1.5f) {
                            collide = 0f;
                        }

                    }
                }
                if (collide <2f || water_status ==0) {
                    if(waterfall > 0) {
                        if (!waterbump.start_bump) {
                            collide = 0f;
                        }
                    }
                    //Debug.Log($"{collide}, {water_status}, {underwater.underwater}, {waterincline}");
                    switch (track) {
                        case Track.Practice:
                            if(collide == 0) {
                                updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            else {
                                updateArray(collide, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            break;
                        default:
                            if (disableWater) {
                                if (collide == 0) {
                                    updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                                }
                                else { updateArray(collide, direct_ang, c_ang, collide_ang, c_speed); }
                            }
                            else {
                                updateArray(collide, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            
                            break;
                    }
                    
                }
                else if(water_status != 0f) { // 무언가와 부딪히는 중; 땅, 풀
                    updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                    updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                }
                
                //Debug.Log($"Collide: {collide}, Water status: {water_status}");
                break;

            case InputMethod.HandStickGesture:
                //Debug.Log("collide" + collide + " water status: " + water_status);
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
                if (waterbump.enabled) {
                    if (waterbump.start_bump) {
                        collide = 1.8f;
                        c_speed = waterbump.height;
                    }
                    else {
                        if (collide != 1.5f) {
                            collide = 2f;
                        }

                    }
                }
                if (collide == 0f) {
                    collide = 2f;
                }
                if(collide<2.0f) {updateArray(collide, direct_ang,  c_ang, collide_ang, c_speed); } // 그 외: 충돌, 물에 빠짐, 출렁임
                else { // 노 젓기
                    updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                    updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
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
        if(collide != 1.0f) { //충돌 도중 속도 바뀌지 않도록.
            collide_speed = colVelocity.magnitude;
        }
        switch (inputMethod) {
            case InputMethod.GamePad:
                if (!c.collider.CompareTag("Water")) {
                    if (c.collider.CompareTag("Grass")) {
                        StartCoroutine(ShortVibration(0.2f));
                    }
                    else {
                        float c_speed = Mathf.Clamp(collide_speed, 0, 1);
                        //Debug.Log($"start coroutine: {colVelocity.magnitude}");
                        StartCoroutine(ShortVibration(c_speed));
                    }
                }
                break;
            case InputMethod.HandStickThrottle:
            case InputMethod.GestureThrottle:
            case InputMethod.HandStickGesture:
                if (!c.collider.CompareTag("Water") && !c.collider.CompareTag("Grass") && !c.collider.CompareTag("Moving")) {
                    if (collide_land && c.collider.CompareTag("Land")) { collide = 2.0f; }
                    else {
                        if(cancollide) {
                            StartCoroutine(CollisionControl());
                            water_status = 0f;
                        } 
                    }
                }
                else {
                    if (!c.collider.CompareTag("Water")) {
                        collide = 2.0f;
                    }
                    if (c.collider.CompareTag("Grass")) {
                        //water_status = 3.0f;
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
            if(land_name != c.collider.name) {
                land_name = c.collider.name;
                land_down = (transform.position - c.collider.bounds.center).normalized;
                }
            rigidbody.AddForce(land_down * 0.3f);
            switch (inputMethod) {
                case InputMethod.GamePad:
                   state = GamePad.GetState(PlayerIndex.One);
                    if (state.IsConnected) {
                        float RY = state.ThumbSticks.Right.Y;
                        float LY = state.ThumbSticks.Left.Y;

                        if (RY > 0 || LY > 0) {
                            float magnitude = Mathf.Sqrt(RY * RY + LY * LY); // 벡터의 크기 계산
                            float normalizedIntensity = Mathf.Clamp01(magnitude);
                            float intensity = 0.3f * normalizedIntensity;
                            GamePad.SetVibration(PlayerIndex.One, 0, intensity);
                        }
                        else {
                            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                            if(rigidbody.velocity.y > 0 && RY ==0 && LY ==0) { //배가 산타는거 방지
                                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
                            }
                        }
                    }
                    else {
                        Debug.Log("GamePad disconnected.");
                    }
                    break;
                case InputMethod.HandStickThrottle:
                case InputMethod.GestureThrottle:
                case InputMethod.HandStickGesture:
                    collide = 2f;
                    if (rigidbody.velocity.y > 0 && HandThrottle.ry == 0 && HandThrottle.ly == 0) { //배가 산타는거 방지
                        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
                    }
                    break;
            }
        }
        if (c.collider.CompareTag("Moving")) {
            if(collide != 1.3f) {
                moving_time = Time.time;
            }
            collide = 1.3f;
            Vector3 colVelocity = c.relativeVelocity;
            
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

            Vector3 col_center = c.collider.bounds.center;
            Vector3 my_center = GetComponent<Collider>().bounds.center;
            Vector3 dir = (my_center - col_center);
            float depth = dir.magnitude;
            collide_speed = depth;
            dir.Normalize();

            switch (inputMethod) {
                case InputMethod.GamePad:
                    if (depth < 7) {
                        transform.position += dir * 2.0f * Time.deltaTime;
                    }
                    else if (depth < 10) {
                        transform.position += dir * 1.5f * Time.deltaTime;
                    }
                    else {
                        transform.position += dir * 0.5f * Time.deltaTime;
                    }
                    float s = 1f;
                    if (depth > 14) {
                        s = Mathf.Abs((depth - 16.5f)) / 3f;
                    }
                    float c_speed = Mathf.Clamp(s, 0, 1);
                    GamePad.SetVibration(PlayerIndex.One, c_speed, c_speed);
                    break;
                case InputMethod.HandStickThrottle:
                    if (depth < 7) {
                        transform.position += dir * 2.0f * Time.deltaTime;
                    }
                    else if (depth < 10) {
                        transform.position += dir * 1.5f * Time.deltaTime;
                    }
                    else {
                        transform.position += dir * 0.5f * Time.deltaTime;
                    }
                    break;
            }
     
        }
    }

    void OnCollisionExit(Collision c) {
        /*
        if(collide != 1f && collide != 0.5f) {
            collide = 0f;
        }*/
        if (!c.collider.CompareTag("Grass")) {
            water_status = 0f;
        }
        
        if (c.collider.CompareTag("Land")) {
            switch (inputMethod) {
                case InputMethod.GamePad:
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                    break;
            }
            if (rigidbody.velocity.y > 0 && transform.position.y > 2f) { // 배 산타기 방지
                rigidbody.velocity = new Vector3(0, 0, 0);
            }
            collide_land = false;
            collide = 0f;
            water_status = 0f;
            HandThrottle.front_m = 2.5f;
            HandThrottle.rotation_m = 15f;
            GamePadInput.front_m = 2.5f;
            HandThrottle.rotation_m = 15f;
        }
        if (c.collider.CompareTag("Moving")) {
            collide = 0f;
            switch (inputMethod) {
                case InputMethod.GamePad:
                    GamePad.SetVibration(PlayerIndex.One, 0, 0);
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("WaterE")){
            waterincline = true;
            waterbump.enabled = true;
            water_status = 0f;
            waterfall++;
        }
        if (other.CompareTag("WaterO")) {
            waterincline = false;
            water_status = 0f;
            if (underwater.underwater) {
                underwater.underwater = false;
                underwater.water_y = 0f;
                GamePad.SetVibration(PlayerIndex.One, 0, 0);
                collide = 0f;
                waterbump.enabled = false;
            }
        }
        if (other.CompareTag("GrassE")) {
            if (water_status != 3f && enterCount== 0) {
                water_status = 3f;
                collide = 2f;
                triggered = other.transform;
                Vector3 planeV = other.transform.up;
                //Debug.Log("plane forward "+ planeV + " transform forward: " + transform.forward + " right: " + transform.right);
                Vector3 myF = transform.right;
                float dot = Vector3.Dot(planeV, myF);
                if (dot >= 0) {
                    grass_front = true;
                }
                else if (dot < 0) {
                    grass_front = false;
                }
                HandThrottle.front_m = 2.0f;
                HandThrottle.rotation_m = 10f;
                GamePadInput.front_m = 2.0f;
                HandThrottle.rotation_m = 10f;

            }
            if (grassboat && enterCount ==0) {
                grassboat = false;
            }  
            enterCount++;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("GrassE")) {
            if (water_status == 3f && enterCount == 4 && !grassin) {
                grassboat = true;
                grassin = true;
            }
            if (!grassboat && enterCount ==1) {
                water_status = 0f;
                collide = 0f;
                grassin = false;
                triggered = null;
                HandThrottle.front_m = 2.5f;
                HandThrottle.rotation_m = 15f;
                GamePadInput.front_m = 2.5f;
                HandThrottle.rotation_m = 15f;
                if (left_grass != null && right_grass != null) {
                    InitArray();

                    MarkArray(left_grass, GeneratePoints());
                    MarkArray(right_grass, GeneratePoints());
                }
            }
            enterCount--;
        }
    }
    private IEnumerator CollisionControl() {
        if(collide == 1.0f || collide == 0.5f || !cancollide) {
            yield break; //이미 충돌 신호가 1 이상 들어와 있으면 중복해서 다시 시작하는 것 금지.
        }
        cancollide = false;
        collide = 0.5f;
        yield return new WaitForSeconds(0.1f);

        if (collide != 2) {
            transform.position += transform.right * 0.1f;
            //rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        }
        //transform.position += transform.right;
        collide = 1.0f;
        water_status = 0;
        bouncy_time = Time.time;
        yield return new WaitForSeconds(0.3f);

        collide = 0.5f;
        yield return new WaitForSeconds(0.2f);
        /*
        switch (inputMethod) {
            case InputMethod.HandStickGesture:
                if(water_status >= 1 && water_status <=2 && collide == 2) {
                    //rigidbody.constraints = RigidbodyConstraints.None;
                    yield break;
                }
                break;
        }*/

        collide = 0.0f;
        from_zero = false;
        yield return new WaitForSeconds(0.5f);
        cancollide = true;
    }
    IEnumerator ShortVibration(float intensity) {
        GamePad.SetVibration(PlayerIndex.One, intensity, intensity);
        yield return new WaitForSeconds(0.1f);
        GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
    }



}