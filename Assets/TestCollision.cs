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
    public float throwSpeed = 5f;
    private float distance;
    public float spawnDistance = 20f;
    private bool canThrow = false;


    private void Start() {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();

        if (rb == null) {
            Debug.LogError("Rigidbody가 이 오브젝트에 없습니다.");
        }

        person = GetComponent<TestMovement>();
        GamePadInput = GetComponent<GamePadInput>();
        HandThrottle = GetComponent<HandThrottle>();
        HandGesture = GetComponent<HandGesture>();

        insitu = false;
        rock = GameObject.Find("Collision/Rock");
        distance = spawnDistance;
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

    private void Update() {
        if (insitu) {
            switch (person.inputMethod) {
                case TestMovement.InputMethod.GamePad:
                    GamePadState state = GamePad.GetState(PlayerIndex.One);

                    float LX = state.ThumbSticks.Left.X; 
                    float LY = state.ThumbSticks.Left.Y;  
                    float RX = state.ThumbSticks.Right.X;
                    float RY = state.ThumbSticks.Right.Y;

                    float angle = CalculateAngle(LX + RX, LY -RY);

                    float max_i = GetMagnitude(angle);
                    float magnitude = Mathf.Sqrt((LX + RX) * (LX + RX) + (LY - RY) * (LY - RY)) / max_i;

                    if (LX == 0 && LY == 0 && RX == 0 && RY == 0) {
                        distance = spawnDistance;
                        canThrow = false;
                    }
                    else {
                        distance -= magnitude * Time.deltaTime * 5f;
                        distance = Mathf.Max(distance, 0.5f);
                        ThrowRock(angle, magnitude);
                    }
                    ThrowRock(angle, magnitude);
                    break;
                case TestMovement.InputMethod.HandStickThrottle:
                    break;
                case TestMovement.InputMethod.HandStickGesture:
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // Rock이 Boat와 충돌한 경우 distance 초기화
        if (collision.gameObject == rock) {
            distance = spawnDistance;
            //canThrow = false;  
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Collide")) {
            Transform explainImage = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/Explain");
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
