using UnityEngine;

public class TestCollision : MonoBehaviour {
    private Rigidbody rb;
    private TestMovement person;
    private GamePadInput GamePadInput;
    private HandThrottle HandThrottle;
    private HandGesture HandGesture;
    private bool insitu;

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
    }

    private void Update() {
        if (insitu) {

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
