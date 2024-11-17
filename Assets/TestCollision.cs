using UnityEngine;
using XInputDotNetPure;

public class TestCollision : MonoBehaviour {
    private Rigidbody rb;
    private TestMovement person;
    private MonoBehaviour GamePadInput;
    private MonoBehaviour HandThrottle;
    private MonoBehaviour HandGesture;
    private Input_Delim input_d;
    private bool insitu;
    private GameObject rock;
    public float throwSpeed = 0.7f;
    private float distance;
    public float spawnDistance = 10f;
    Transform explainImage;

    private void Start() {
        // Rigidbody ������Ʈ ��������
        rb = GetComponent<Rigidbody>();

        if (rb == null) {
            Debug.LogError("Rigidbody�� �� ������Ʈ�� �����ϴ�.");
        }
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        person = GetComponent<TestMovement>();
        GamePadInput = GetComponent<GamePadInput>();
        HandThrottle = GetComponent<HandThrottle>();
        HandGesture = GetComponent<HandGesture>();

        insitu = false;
        rock = GameObject.Find("Collision/Rock");
        distance = spawnDistance;
        explainImage = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/Explain");
    }


    float CalculateAngle(float x, float y) {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        if (angle < 0) {
            angle += 360f;
        }

        return angle;
    }

    public void ThrowRock(float angle, float relative) {
        float angleRad = angle * Mathf.Deg2Rad;

        Vector3 spawnPosition = new Vector3(
            Mathf.Cos(angleRad) * distance,
            0f,
            Mathf.Sin(angleRad) * distance
        ) + transform.position;

        rock.transform.position = spawnPosition;

        Vector3 direction = (transform.position - spawnPosition).normalized;

        Rigidbody rigidb = rock.GetComponent<Rigidbody>();
        if (rigidb != null) {
            rigidb.velocity = direction * throwSpeed * relative;
        }
    }

    public float GetMagnitude(float angle) {
        float max_i;
        if (angle < 90) {
            float tangent = Mathf.Tan(Mathf.Deg2Rad * angle);
            if (angle <= 45) { max_i = 2 * Mathf.Sqrt(1 + tangent * tangent); }
            else { max_i = 2 * Mathf.Sqrt(1 + (1 / tangent) * (1 / tangent)); }
        }
        else if (angle < 180) {
            float new_angle = angle - 90;
            float tangent = Mathf.Tan(Mathf.Deg2Rad * new_angle);
            if (new_angle <= 45) { max_i = 2 * Mathf.Sqrt(1 + tangent * tangent); }
            else { max_i = 2 * Mathf.Sqrt(1 + (1 / tangent) * (1 / tangent)); }
        }
        else if (angle < 270) {
            float new_angle = angle - 180;
            float tangent = Mathf.Tan(Mathf.Deg2Rad * new_angle);
            if (new_angle <= 45) { max_i = 2 * Mathf.Sqrt(1 + tangent * tangent); }
            else { max_i = 2 * Mathf.Sqrt(1 + (1 / tangent) * (1 / tangent)); }

        }
        else {
            float new_angle = angle - 270;
            float tangent = Mathf.Tan(Mathf.Deg2Rad * new_angle);

            if (new_angle <= 45) { max_i = 2 * Mathf.Sqrt(1 + tangent * tangent); }
            else { max_i = 2 * Mathf.Sqrt(1 + (1 / tangent) * (1 / tangent)); }
        }

        return max_i;
    }

    public void EndScenario() {
        insitu = false;
        rb.constraints = RigidbodyConstraints.None;
        Vector3 newPosition = transform.position;
        newPosition.z += 25f;
        transform.position = newPosition;
        explainImage.gameObject.SetActive(false);

        GamePadInput.enabled = true;
    }

    private void Update() {
        if (insitu) {
            switch (person.inputMethod) {
                case TestMovement.InputMethod.GamePad:
                    GamePadState state = GamePad.GetState(PlayerIndex.One);

                    float lx = state.ThumbSticks.Left.X; 
                    float ly = state.ThumbSticks.Left.Y;  
                    float rx = state.ThumbSticks.Right.X;
                    float ry = state.ThumbSticks.Right.Y;

                    if(lx == -1 && rx == 1) {
                        EndScenario();
                    }
                    float angle = CalculateAngle(lx + rx, ly -ry);

                    float max_i = GetMagnitude(angle);
                    float magnitude = Mathf.Sqrt((lx + rx) * (lx + rx) + (ly - ry) * (ly - ry)) / max_i;

                    if (lx == 0 && ly == 0 && rx == 0 && ry == 0) {
                        distance = spawnDistance;
                    }
                    else {
                        distance -= magnitude * Time.deltaTime * 5f;
                        distance = Mathf.Max(distance, 0.5f);
                    }
                    ThrowRock(angle, magnitude);
                    break;
                case TestMovement.InputMethod.HandStickThrottle:
                    int accum_x = input_d.accum_lx;
                    lx = accum_x / 800f;
                    int accum_y = input_d.accum_ly;
                    ly = accum_y / 800f;
                    lx = Mathf.Abs(lx) < 0.15f ? 0 : Mathf.Clamp(lx, -1f, 1f);
                    ly = Mathf.Abs(ly) < 0.15f ? 0 : Mathf.Clamp(ly, -1f, 1f);
                    rx = input_d.accum_rx / 800f;
                    ry = input_d.accum_ry / 800f;
                    rx = Mathf.Abs(rx) < 0.15f ? 0 : Mathf.Clamp(rx, -1f, 1f);
                    ry = Mathf.Abs(ry) < 0.15f ? 0 : Mathf.Clamp(ry, -1f, 1f);
                    if (lx == -1 && rx == 1) {
                        EndScenario();
                    }
                    angle = CalculateAngle(lx + rx, -ly- ry);

                    max_i = GetMagnitude(angle);
                    magnitude = Mathf.Sqrt((lx + rx) * (lx + rx) + (ly + ry) * (ly + ry)) / max_i;

                    if (lx == 0 && ly == 0 && rx == 0 && ry == 0) {
                        distance = spawnDistance;
                    }
                    else {
                        distance -= magnitude * Time.deltaTime * 5f;
                        distance = Mathf.Max(distance, 0.5f);
                    }
                    ThrowRock(angle, magnitude);
                    break;
                case TestMovement.InputMethod.HandStickGesture:
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // Rock�� Boat�� �浹�� ��� distance �ʱ�ȭ
        if (collision.gameObject == rock) {
            distance = spawnDistance;
            //canThrow = false;  
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Collide")) {
            if (explainImage != null)
            {
                explainImage.gameObject.SetActive(true);
            }

            GameObject testEnvironment = GameObject.Find("TestEnvironment");
            if (testEnvironment != null) {
                Transform testPoint = testEnvironment.transform.Find("Collision/TestPoint");
                if (testPoint != null) {
                    transform.position = testPoint.position;
                    transform.rotation = Quaternion.Euler(0, 90, 0);

                    if (rb != null) {
                        rb.constraints = RigidbodyConstraints.FreezePositionX |
                                         RigidbodyConstraints.FreezePositionZ |
                                         RigidbodyConstraints.FreezeRotationY;
                    }
                }
                
            }

            switch (person.inputMethod) {
                case TestMovement.InputMethod.GamePad:
                    GamePadInput.enabled = false;
                    break;
                case TestMovement.InputMethod.HandStickThrottle:
                    HandThrottle.enabled = false;
                    break;
                case TestMovement.InputMethod.HandStickGesture:
                    HandGesture.enabled = false;
                    break;
            }

            insitu = true;

        }
    }
}