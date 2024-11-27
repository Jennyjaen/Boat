using System.Collections;
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
    Transform testPoint;
    public float radius = 10f;
    private bool colliding;

    private float lastCollision;
    private float cooldown = 1f;
    public float col_angle = 0f;
    private void Start() {
        // Rigidbody 컴포넌트 가져오기
    }

    void OnEnable() {
        rb = GetComponent<Rigidbody>();

        if (rb == null) {
            Debug.LogError("Rigidbody가 이 오브젝트에 없습니다.");
        }
        input_d = transform.Find("Input").GetComponent<Input_Delim>();
        person = GetComponent<TestMovement>();
        GamePadInput = GetComponent<GamePadInput>();
        HandThrottle = GetComponent<HandThrottle>();
        HandGesture = GetComponent<HandGesture>();

        insitu = false;
        rock = GameObject.Find("Collision/Circle");
        distance = 0;
        explainImage = transform.Find("XR Rig/Camera Offset/Main Camera/Canvas/Explain");
        GameObject testEnvironment = GameObject.Find("TestEnvironment");
        testPoint = testEnvironment.transform.Find("Collision/TestPoint");
        colliding = false;
        lastCollision = 0f;

        if (explainImage != null) {
            explainImage.gameObject.SetActive(true);
        }

        //ArrangeChildObjectsInCircle();
        if (testPoint != null) {
            transform.position = testPoint.position;
            transform.rotation = Quaternion.Euler(0, 90, 0);

            if (rb != null) {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
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

    float CalculateAngle(float x, float y) {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        if (angle < 0) {
            angle += 360f;
        }
        col_angle = angle;
        if(col_angle < 0) {
            col_angle = 360;
        }
        return angle;
    }

    public void ThrowBoat(float angle, float relative) {
        float angleRad = angle * Mathf.Deg2Rad;

        Vector3 spawnPosition = new Vector3(
            Mathf.Cos(angleRad) * distance,
            0f,
            Mathf.Sin(angleRad) * distance
        ) + testPoint.position;

        transform.position = spawnPosition;

        Vector3 direction = (transform.position - testPoint.position).normalized;
        if (rb != null) {
            rb.velocity = direction * relative;
            //Debug.Log(rb.velocity);
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
        Vector3 newPosition = testPoint.position;
        newPosition.z += 25f;
        transform.position = newPosition;

        explainImage.gameObject.SetActive(false);
        person.testcol = false;
        switch (person.inputMethod) {
            case TestMovement.InputMethod.GamePad:
                GamePadInput.enabled = true;
                break;
            case TestMovement.InputMethod.HandStickThrottle:
                HandThrottle.enabled = true;
                break;
        }
        this.enabled = false;
    }

    private void Update() {
        if (insitu && !colliding) {
            switch (person.inputMethod) {
                case TestMovement.InputMethod.GamePad:
                    GamePadState state = GamePad.GetState(PlayerIndex.One);

                    float lx = state.ThumbSticks.Left.X; 
                    float ly = state.ThumbSticks.Left.Y;  
                    float rx = state.ThumbSticks.Right.X;
                    float ry = state.ThumbSticks.Right.Y;

                    //Debug.Log($"lx: {lx} rx: {rx}");
                    if(lx == -1 && rx == 1) {
                        EndScenario();
                        break;
                    }
                    float angle = CalculateAngle(lx + rx, ly -ry);

                    float max_i = GetMagnitude(angle);
                    float magnitude = Mathf.Sqrt((lx + rx) * (lx + rx) + (ly - ry) * (ly - ry)) / max_i;

                    if (lx == 0 && ly == 0 && rx == 0 && ry == 0) {
                        distance = 0;
                    }
                    else {
                        distance += magnitude * Time.deltaTime * 5f;
                        //distance = Mathf.Max(distance, 0.5f);
                    }
                    ThrowBoat(angle, magnitude);
                    break;
                case TestMovement.InputMethod.HandStickThrottle:
                    int accum_x = input_d.accum_lx;
                    lx = accum_x / 400f;
                    int accum_y = input_d.accum_ly;
                    ly = accum_y / 400f;
                    lx = Mathf.Abs(lx) < 0.15f ? 0 : Mathf.Clamp(lx, -1f, 1f);
                    ly = Mathf.Abs(ly) < 0.15f ? 0 : Mathf.Clamp(ly, -1f, 1f);
                    rx = input_d.accum_rx / 400f;
                    ry = input_d.accum_ry / 400f;
                    rx = Mathf.Abs(rx) < 0.15f ? 0 : Mathf.Clamp(rx, -1f, 1f);
                    ry = Mathf.Abs(ry) < 0.15f ? 0 : Mathf.Clamp(ry, -1f, 1f);

                    if (lx < -0.8f && rx > 0.8f) {
                        //Debug.Log("end?");
                        EndScenario();
                        break;
                    }
                    //Debug.Log($"lx: {lx} ly: {ly} rx: {rx} ry: {ry}");
                    angle = CalculateAngle(lx + rx, -ly- ry);

                    max_i = GetMagnitude(angle);
                    magnitude = Mathf.Sqrt((lx + rx) * (lx + rx) + (ly + ry) * (ly + ry)) / max_i;
                    //Debug.Log(magnitude);
                     
                    if (lx == 0 && ly == 0 && rx == 0 && ry == 0) {
                        distance = 0;
                    }
                    else {
                        distance += magnitude * Time.deltaTime * 5f;
                    }
                    ThrowBoat(angle, magnitude);
                    break;
                case TestMovement.InputMethod.HandStickGesture:
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag("Collide")) {
            StartCoroutine(HandleCollision());
            colliding = true;
        }
        
    }
    private IEnumerator HandleCollision() {
        //rb.constraints = RigidbodyConstraints.FreezePosition;
        if(Time.time - lastCollision > cooldown) { lastCollision = Time.time; }
        else { yield break; }
        yield return new WaitForSeconds(1f);

        // Rock이 Boat와 충돌한 경우 distance 초기화
        distance = 0;
        transform.position = testPoint.position;
        transform.rotation = Quaternion.Euler(0, 90, 0);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        colliding = false;
        //rb.constraints = RigidbodyConstraints.None;
        // canThrow = false;  
    }


    void ArrangeChildObjectsInCircle() {
        if (rock == null || testPoint == null) {
            Debug.LogError("rock 또는 testPoint가 설정되지 않았습니다!");
            return;
        }
        int childCount = rock.transform.childCount;
        if (childCount == 0) {
            Debug.LogWarning("rock에 자식 오브젝트가 없습니다!");
            return;
        }

        float angleStep = 360f / childCount;

        for (int i = 0; i < childCount; i++) {
            Transform child = rock.transform.GetChild(i);

            float angle = angleStep * i;

            float radian = angle * Mathf.Deg2Rad;

            float x = testPoint.position.x + Mathf.Cos(radian) * radius;
            float z = testPoint.position.z + Mathf.Sin(radian) * radius;

            GameObject testEnvironment = GameObject.Find("TestEnvironment");
            Transform col = testEnvironment.transform.Find("Collision");
            Vector3 newPosition = new Vector3(x, testPoint.position.y, z);
            child.position = newPosition;
            Debug.Log(col.transform.InverseTransformPoint(newPosition));
            child.LookAt(testPoint.position);
        }
    }
}
