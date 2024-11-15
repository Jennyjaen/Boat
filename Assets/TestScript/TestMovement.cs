using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class TestMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private UserTest situation;

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

    [HideInInspector]
    public bool waterincline = false;
    private int waterfall = 0;
    private float max_incline;
    private GamePadState state;
    private Transform triggered;

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

    [HideInInspector]
    public byte[] larray = new byte[108];
    [HideInInspector]
    public byte[] rarray = new byte[108];

    void Start()
    {
        situation = GameObject.Find("TestEnvironment").GetComponent<UserTest>();
        rigidbody = GetComponent<Rigidbody>();
        front = transform.Find("Front");
        max_incline = 0f;
        collide = 0f;
        collide_ang = 0f;
        collide_speed = 0f;
        water_status = 0;
        collide_land = false;

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
        switch (inputMethod) {
            case InputMethod.GamePad:
                foreach(Transform child in transform) {
                    if(child.name == "Input") {
                        child.gameObject.SetActive(false);
                    }
                }
                break;
        }
    }

    void InitArray() {
        for (int i = 0; i < left_grass.GetLength(0); i++) {
            for (int j = 0; j < right_grass.GetLength(1); j++) {
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
                x = Random.Range(0, 3); // ���� 0, 1, 2 �� �ϳ� ����
            } while (x == previousX); // ���� ���� ���� ��ǥ�� �ٸ��� ����

            previousX = x;

            // 3. ���ο� ���� ���� ��ǥ ���� ����
            int yRangeStart = 6 * i + 19;
            int yRangeEnd = 6 * i + 23;
            int y = Random.Range(yRangeStart, yRangeEnd);

            int specificX;
            if (x == 0)
                specificX = Random.Range(1, 4); // ���ΰ� 0�� �� 1~3 �� ����
            else if (x == 1)
                specificX = Random.Range(4, 8); // ���ΰ� 1�� �� 4~7 �� ����
            else
                specificX = Random.Range(8, 12); // ���ΰ� 2�� �� 8~10 �� ����

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

    void updateArray(float collide, float angle, float clamp_ang, float col_ang, float col_s) { //�浹, ���� ����, ���� ����
        if (collide == 0.5f) { // collide �� �ƹ��� �ǵ�� x
            for (int i = 0; i < 108; i++) {
                larray[i] = (byte)0;
                rarray[i] = (byte)0;
            }
        }
        else if (collide == 1f) {// collide
            int x_1 = 0;
            int x_2 = 0;

            //float intensity = Mathf.Ceil(col_s * 5) / 5;
            float intensity = col_s * 6;
            intensity = Mathf.Round(intensity);
            Debug.Log($"col_s: {col_s}, intensity: {intensity}");
            if (intensity == 6) {
                intensity = 5;
            }
            //Debug.Log($"collide speed: {col_s}, so intensity: {intensity}");
            for (int y = 0; y < 18; y++) {
                for (int x = 0; x < 24; x++) {
                    float cent_x = ((float)x + 0.5f) / 24;
                    float cent_y = ((float)y + 0.5f) / 12;
                    float res;
                    if (col_ang >= 22.5 && col_ang < 67.5) {
                        //
                        //Debug.Log("ru");
                        if (cent_x + cent_y >= (2 - col_s * 2)) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 67.5 && col_ang < 112.5) {
                        //��
                        //Debug.Log("up");
                        if (cent_y < col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 112.5 && col_ang < 157.5) {
                        //Debug.Log("lu");
                        if (cent_x + cent_y <= 2 * col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 157.5 && col_ang < 202.5) {
                        //����
                        //Debug.Log("Left");
                        if (cent_x < col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 202.5 && col_ang < 247.5) {
                        //Debug.Log("ld");
                        if (-cent_x + cent_y >= 1 - 2 * col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 247.5 && col_ang < 292.5) {
                        //�Ʒ�
                        //Debug.Log("down");
                        if (cent_y >= 1 - col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else if (col_ang >= 292.5 && col_ang < 337.5) {
                        //Debug.Log("rd");
                        if (cent_x - cent_y >= 1 - 2 * col_s) { res = intensity; }
                        else { res = 0; }
                    }
                    else {
                        //������?
                        //Debug.Log("right");
                        if (cent_x >= 1 - col_s) { res = intensity; }
                        else { res = 0; }
                    }

                    if (x % 2 == 0) { x_1 = (int)res; }
                    else {
                        x_2 = (int)res;
                        if (x >= 12) {
                            int index = y * 6 + ((x - 12) / 2);
                            rarray[107 - index] = (byte)(x_1 + x_2 * 6); //rarray ��������.
                        }
                        else {
                            int index = y * 6 + (x / 2);
                            larray[index] = (byte)(x_1 * 6 + x_2);
                        }
                    }
                }
            }
        }
        else if (collide == 1.5f) { // ���� ������ ��
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
            System.Array.Reverse(arr); //������ ��������.
            rarray = arr;
        }
        else if (collide == 0f) {// ����: 0f
            if (clamp_ang < incline_deadzone) {
                //Debug.Log("deadzone");
                for (int i = 0; i < 108; i++) {
                    larray[i] = (byte)0;
                    rarray[i] = (byte)0;
                    return;
                }
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
                vib_level = Mathf.FloorToInt(valid_ang / 1.8f) + 2;
                if (vib_level > max_v) { vib_level = max_v; }
            }
            else { //�ӵ� ������: �̶� max magnitude�� 3���� �����ϴ� ���� ���� �� ����.

                if (valid_ang < 0) { vib_level = 0; }
                else {
                    vib_level = Mathf.FloorToInt(valid_ang) + 1;
                    if (vib_level > 3) { vib_level = 3; }
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
                /*����*/
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
                /*���� �Ʒ�*/
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
                /*�Ʒ�*/
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
                /*���� �Ʒ�*/
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
                /*������*/
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
                /*���� ��*/
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
                // ��
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
                /*���� ��*/
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

        //Debug.Log(string.Join(",", larray));
    }

    void printArray(byte[] array) {
        string output = "";

        for (int i = 0; i < array.Length; i++) {
            output += (int)(array[i] / 6) + " " + (int)(array[i] % 6) + " ";

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

    byte[] int2byteArray(int[,] array, bool reverse) {
        byte[] result = new byte[108];
        for (int y = 0; y < 18; y++) {
            for (int x = 0; x < 6; x++) {
                if (reverse) {
                    result[107 - (y * 6 + x)] = (byte)(array[x * 2, y] + array[x * 2 + 1, y] * 6);
                }
                else {
                    result[y * 6 + x] = (byte)(array[x * 2, y] * 6 + array[x * 2 + 1, y]);
                }

            }
        }

        return result;
    }
    void updateEachArray(bool isLeft, float vel, bool reverse, int sum, float water) {
        //water: 0 �Ѵ� �Ϲ� ������ 1: ���� ��, 1.5: ���� ��, 2: ������ ��, 3: Ǯ ��
        //water incline
        byte[] arr = new byte[108];

        switch (inputMethod) {
            case InputMethod.HandStickThrottle:
                if (water == 1) {
                    for (int i = 0; i < 108; i++) {
                        larray[i] = (byte)21;
                        rarray[i] = (byte)0;
                    }
                }
                else if (water == 1.5f) {
                    for (int i = 0; i < 108; i++) {
                        larray[i] = (byte)21;
                        rarray[i] = (byte)21;
                    }
                }
                else if (water == 2f) {
                    for (int i = 0; i < 108; i++) {
                        larray[i] = (byte)0;
                        rarray[i] = (byte)21;
                    }
                }
                else if (water == 3f) {
                    int boat_pos = 0;
                    if(triggered!= null) {
                        boat_pos = Mathf.FloorToInt((transform.position.z - triggered.position.z + 2.43f) * 2);
                        if(boat_pos < 0) { boat_pos = 0; }
                    }
                    //Debug.Log($"{transform.position.z} is here so boat pos is {boat_pos}");
                    Debug.Log(transform.position.z - triggered.position.z);
                    Debug.Log(boat_pos);
                    int[,] left_slice = SliceArray(left_grass, 0, boat_pos, 12, 18);
                    int[,] right_slice = SliceArray(right_grass, 0, boat_pos, 12, 18);
                    larray = int2byteArray(left_slice, true);
                    rarray = int2byteArray(right_slice, false);

                }
                break;
            case InputMethod.HandStickGesture: // ������
                int max_vib = 0;
                int width = 4;

                if (Mathf.Abs(sum) > 10) {
                    if (isLeft && water == 1) { width = 8; max_vib = 5; }
                    else if (!isLeft && water == 2) { width = 8; max_vib = 5; }
                    else if (water == 1.5f) { width = 8; max_vib = 5; }
                    else {
                        if (vel <= 3) { max_vib = 5; }
                        else if (vel <= 5) { max_vib = 4; }
                        else if (vel <= 7) { max_vib = 3; }
                        else if (vel <= 9) { max_vib = 2; }
                        else { max_vib = 1; }
                    }
                }

                if (reverse && sum > 0) { max_vib = 0; }
                if (!reverse && sum < 0) { max_vib = 0; }
                int sero = Mathf.Abs(sum) / 120;
                if (waterincline) {
                    //zRot = parent.eulerAngles.z;
                    //if (zRot > 180) { zRot -= 360; }
                    //if (zRot > 15) { down = true; }
                    float zRot = transform.rotation.eulerAngles.z;
                    if (zRot > 180) { zRot -= 360; }
                    if (zRot > 15) {
                        sero = Mathf.Abs(sum) / 100;
                    }
                    else if (zRot < -15) {
                        sero = Mathf.Abs(sum) / 210;
                    }

                }
                if (isLeft && water == 1) { sero = Mathf.Abs(sum) / 210; }
                if (!isLeft && water == 2) { sero = Mathf.Abs(sum) / 210; }
                if (water == 1.5f) { sero = Mathf.Abs(sum) / 210; }
                if (water == 3) {
                    sero = Mathf.Abs(sum) / 120;
                    int grass_idx = sero / 3;
                    if (grass_idx >= 6) { grass_idx = 5; }
                    if (grass[grass_idx] != sero) {
                        if (grass_idx > 0) { sero = grass[grass_idx - 1]; }
                        else { sero = 0; }
                    }

                }
                for (int y = 0; y < 18; y++) {
                    for (int n = 0; n < 6; n++) {
                        if (y >= sero && y < sero + width) {
                            arr[y * 6 + n] = (byte)(max_vib * 6 + max_vib);
                        }
                        else { arr[y * 6 + n] = (byte)0; }
                    }
                }
                if (!reverse) { System.Array.Reverse(arr); }
                if (isLeft) { larray = (byte[])arr.Clone(); }
                else {
                    System.Array.Reverse(arr); //������ ��ġ ����� �Ųٷ� �־������.
                    rarray = (byte[])arr.Clone();
                }

                break;
        }

    }
    void GrassEffect() {
        for (int i = 0; i < 6; i++) {
            grass[i] = Random.Range(i * 3, i * 3 + 2);
        }
    }
    void Print12Array(int[,] targetArray) {
        string result = ""; // ��ü �迭�� ���� ���ڿ�

        for (int i = 0; i < targetArray.GetLength(0); i++) {
            for (int j = 0; j < targetArray.GetLength(1); j++) {
                result += targetArray[i, j] + " ";
            }
            result = result.TrimEnd() + "\n"; // �� ���� ������ ���� ���� �� �ٹٲ� �߰�
        }

        Debug.Log(result.TrimEnd()); // ��ü ���ڿ��� �� ���� ���
    }

    void Update() {
        //Debug.Log($"collide: {collide} , water: {water_status}");
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
        if (rotation.x > 180) { rotation.x -= 360; }
        rotation.x = Mathf.Clamp(rotation.x, -40f, 40f);
        if (rotation.z > 180) { rotation.z -= 360; }
        rotation.z = Mathf.Clamp(rotation.z, -40f, 40f);
        transform.eulerAngles = rotation;
        if (underwater.underwater) {
            waterincline = false;
            waterfall = 0;
        }

        //�ְ�ӵ� ����
        if (rigidbody.velocity.magnitude > 15.0f) {
            rigidbody.velocity = rigidbody.velocity.normalized * 15.0f;
        }
        if (rigidbody.angularVelocity.magnitude > 1.0f) {
            rigidbody.angularVelocity = rigidbody.angularVelocity.normalized * 1.0f;
        }

        switch (inputMethod) {
            case InputMethod.GamePad: //���� �е忡 ��ƽ �ǵ���� �ִ� ���
                switch (situation.env) {
                    case UserTest.Environment.Waterfall:
                        if (underwater.underwater) {
                            float currentPositionY = front.position.y;
                            float diff = underwater.water_y - currentPositionY;
                            float intensity = Mathf.Clamp(diff * 5, 0, 1);
                            GamePad.SetVibration(PlayerIndex.One, 0, intensity);
                        }
                        break;

                }
                break;
            case InputMethod.HandStickThrottle: //��ġ�� ��ƽ �ǵ���� �ִ� ���
                Vector3 up_vector = transform.up;
                Vector3 forward_vector = -transform.forward;
                float ang = Vector3.Angle(up_vector, Vector3.up);
                Vector3 up_projected = new Vector3(up_vector.x, 0, up_vector.z);
                Vector3 for_projected = new Vector3(forward_vector.x, 0, forward_vector.z);
                float direct_ang = Vector3.SignedAngle(up_projected, for_projected, Vector3.up);
                float c_ang = ang;
                float c_speed = Mathf.Clamp(collide_speed * 5f, 0, 3);
                c_speed /= 3f;
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
                        collide = 0f;
                    }
                }

                if (collide < 2f || water_status == 0) {
                    if (waterfall > 0) {
                        collide = 0f;
                    }
                    switch (situation.env) {
                        case UserTest.Environment.Water:
                            if(collide == 0) {
                                updateArray(collide, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            break;
                        case UserTest.Environment.Waterfall:
                            if(collide == 1.5f) {
                                updateArray(collide, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            else {
                                updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            break;
                        case UserTest.Environment.Collision:
                        case UserTest.Environment.Moving:
                            if (collide == 1.0f) {
                                updateArray(collide, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            else {
                                updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            break;
                        case UserTest.Environment.Grass:
                        case UserTest.Environment.Land:
                        case UserTest.Environment.Rowing:
                            updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            break;
                    }
                }
                else if (water_status != 0f) { // ���𰡿� �ε����� ��; ��, Ǯ
                    switch (situation.env) {
                        case UserTest.Environment.Collision:
                        case UserTest.Environment.Waterfall:
                        case UserTest.Environment.Moving:
                        case UserTest.Environment.Rowing:
                        case UserTest.Environment.Water:
                            break;
                        case UserTest.Environment.Land:
                            if(water_status >= 1 && water_status <= 2) {
                                updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                                updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                            }
                            else {
                                updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            break;
                        case UserTest.Environment.Grass:
                            if (water_status ==3) {
                                updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                                updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                            }
                            else {
                                updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            }
                            break;
                    }

                }

                //Debug.Log($"Collide: {collide}, Water status: {water_status}");
                break;

            case InputMethod.HandStickGesture:
                direct_ang = 0f; //���� ǥ�� x�̱� ������ ��� �ʿ� x.
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
                    if (bef_coll == 1.5f) {
                        collide = 2.0f;
                    }
                }
                if (collide == 0f) {
                    collide = 2f;
                }
                if (collide < 2.0f) { // �� ��: �浹, ���� ����, �ⷷ��
                    switch (situation.env) {
                        case UserTest.Environment.Collision:
                        case UserTest.Environment.Moving:
                            if (collide == 1.0f) { updateArray(collide, direct_ang, c_ang, collide_ang, c_speed); }
                            else { updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed); }
                            break;
                        case UserTest.Environment.Waterfall:
                            if(collide == 1.5f) { updateArray(collide, direct_ang, c_ang, collide_ang, c_speed); }
                            else { updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed); }
                            break;
                        case UserTest.Environment.Land:
                        case UserTest.Environment.Grass:
                        case UserTest.Environment.Rowing:
                        case UserTest.Environment.Water:
                            updateArray(0.5f, direct_ang, c_ang, collide_ang, c_speed);
                            break;
                    }

                
                } 
                else { // �� ����
                    switch (situation.env) {
                        case UserTest.Environment.Land:
                            if(water_status >= 1 && water_status <= 2) {
                                updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                                updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                            }
                            else { updateArray(0.5f, 0, 0, 0, 0); }
                            break;
                        case UserTest.Environment.Grass:
                            if (water_status== 3) {
                                updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                                updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                            }
                            else { updateArray(0.5f, 0, 0, 0, 0); }
                            break;
                        case UserTest.Environment.Water:
                            if (water_status ==0) {
                                updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                                updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                            }
                            else { updateArray(0.5f, 0, 0, 0, 0); }
                            break;
                        case UserTest.Environment.Rowing:
                            updateEachArray(true, rigidbody.velocity.magnitude, input_d.l_reverse, input_d.sum_l, water_status); //left hand
                            updateEachArray(false, rigidbody.velocity.magnitude, input_d.r_reverse, input_d.sum_r, water_status); //right hand
                            break;
                        case UserTest.Environment.Waterfall:
                        case UserTest.Environment.Moving:
                        case UserTest.Environment.Collision:
                            updateArray(0.5f, 0, 0, 0, 0);
                            break;
                    }
                }

                break;
        }


    }

    public float AverageZ(ContactPoint[] contacts) {
        if (contacts.Length == 0) { return 0f; }
        float sumZ = 0f;
        foreach (ContactPoint contact in contacts) {
            Vector3 world_collide = contact.point;
            Vector3 local_collide = transform.InverseTransformPoint(world_collide);
            sumZ += local_collide.z;
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
                        switch (situation.env) {
                            case UserTest.Environment.Collision:
                            case UserTest.Environment.Moving:
                            case UserTest.Environment.Land:
                                float c_speed = Mathf.Clamp(collide_speed * 10f, 0, 1);
                                StartCoroutine(ShortVibration(c_speed));
                                break;
                        }
                        
                    }
                }
                break;
            case InputMethod.HandStickThrottle:
            case InputMethod.HandStickGesture:
                if (!c.collider.CompareTag("Water") && !c.collider.CompareTag("Grass")) {
                    if (collide_land && c.collider.CompareTag("Land")) { collide = 2.0f; }
                    else {
                        if (collide != 1.0f && collide != 0.5f) {
                            StartCoroutine(CollisionControl());
                            water_status = 0f;
                        }

                    }

                }
                else {
                    collide = 2.0f;
                    if (c.collider.CompareTag("Land")) {
                        collide_land = true;
                        float col = AverageZ(c.contacts);
                        if (col >= 0) { water_status = 2.0f; }
                        else if (col < 0) { water_status = 1.0f; }
                        else {
                            water_status = 0f;
                        }
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
            switch (inputMethod) {
                case InputMethod.GamePad:
                    switch (situation.env) {
                        case UserTest.Environment.Land:
                            state = GamePad.GetState(PlayerIndex.One);
                            if (state.IsConnected) {
                                float LX = state.ThumbSticks.Left.X;
                                float LY = state.ThumbSticks.Left.Y;

                                if (LX > 0 || LY > 0) {
                                    float magnitude = Mathf.Sqrt(LX * LX + LY * LY); // ������ ũ�� ���
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
                    }
                    
                    break;
                case InputMethod.HandStickThrottle:
                case InputMethod.HandStickGesture:
                    collide = 2f;
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
        if (c.collider.CompareTag("Moving")) {
            collide = 1.0f;
            Vector3 colVelocity = c.relativeVelocity;
            //collide_speed = colVelocity.magnitude;
            switch (inputMethod) {
                case InputMethod.HandStickThrottle:
                    collide_speed = 0.24f;
                    break;
                case InputMethod.HandStickGesture:
                    collide_speed = 2.0f;
                    break;
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
            switch (inputMethod) {
                case InputMethod.GamePad:
                    switch (situation.env) {
                        case UserTest.Environment.Moving:
                            float c_speed = Mathf.Clamp(collide_speed * 8f, 0, 1);
                            GamePad.SetVibration(PlayerIndex.One, c_speed, c_speed);
                            break;
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
            collide_land = false;
            collide = 0f;
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
        if (other.CompareTag("WaterE")) {
            waterincline = true;
            water_status = 0f;
            waterfall++;
        }
        if (other.CompareTag("WaterO")) {
            waterincline = false;
            water_status = 0f;
        }
        if (other.CompareTag("GrassE")) {
            if (water_status != 3f && enterCount == 0) {
                water_status = 3f;
                collide = 2f;
                triggered = other.transform;
            }
            if (grassboat && enterCount == 0) {
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
            if (!grassboat && enterCount == 1) {
                water_status = 0f;
                collide = 0f;
                grassin = false;
                triggered = null;

                if(left_grass != null && right_grass != null) {
                    InitArray();
                    
                    MarkArray(left_grass, GeneratePoints());
                    MarkArray(right_grass, GeneratePoints());
                }

            }
            enterCount--;
        }
    }
    private IEnumerator CollisionControl() {
        if (collide == 1.0f || collide == 0.5f) {
            yield break; //�̹� �浹 ��ȣ�� 1 �̻� ���� ������ �ߺ��ؼ� �ٽ� �����ϴ� �� ����.
        }
        collide = 1.0f;
        yield return new WaitForSeconds(2.0f);

        switch (inputMethod) {
            case InputMethod.HandStickGesture:
                if (water_status >= 1 && water_status <= 2 && collide == 2) {
                    yield break;
                }
                break;
        }
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
