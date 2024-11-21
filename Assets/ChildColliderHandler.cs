using UnityEngine;

public class ChildColliderHandler : MonoBehaviour {
    // 부모로 충돌 정보를 전달하기 위한 델리게이트
    public delegate void TriggerEventHandler(Collider other, Transform child);
    public static event TriggerEventHandler OnChildTriggerStay;
    public static event TriggerEventHandler OnChildTriggerExit;

    private void OnTriggerStay(Collider other) {
        // 충돌한 객체가 태그가 "Land"인지 확인
        //Debug.Log("ho " + other.name);
        if (other.CompareTag("Land")) {
            //Debug.Log("hey!");
            OnChildTriggerStay?.Invoke(other, transform);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Land")) {
            OnChildTriggerExit?.Invoke(other, transform);
        }
    }
}
