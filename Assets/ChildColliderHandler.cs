using UnityEngine;

public class ChildColliderHandler : MonoBehaviour {
    // �θ�� �浹 ������ �����ϱ� ���� ��������Ʈ
    public delegate void TriggerEventHandler(Collider other, Transform child);
    public static event TriggerEventHandler OnChildTriggerStay;
    public static event TriggerEventHandler OnChildTriggerExit;

    private void OnTriggerStay(Collider other) {
        // �浹�� ��ü�� �±װ� "Land"���� Ȯ��
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
