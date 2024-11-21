using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalCheck : MonoBehaviour
{
    [HideInInspector]
    public HashSet<int> collidingChildIndices = new HashSet<int>();
    [HideInInspector]
    public bool on_land;
    // Start is called before the first frame update
    public Transform person;
    void Start()
    {
        on_land = false;
    }


    private void OnEnable() {
        // �ڽ��� �̺�Ʈ ����
        ChildColliderHandler.OnChildTriggerStay += HandleChildTriggerStay;
        ChildColliderHandler.OnChildTriggerExit += HandleChildTriggerExit;
    }

    private void OnDisable() {
        // �ڽ��� �̺�Ʈ ���� ����
        ChildColliderHandler.OnChildTriggerStay -= HandleChildTriggerStay;
        ChildColliderHandler.OnChildTriggerExit -= HandleChildTriggerExit;
    }

    // �ڽ��� TriggerStay �̺�Ʈ ó��
    private void HandleChildTriggerStay(Collider other, Transform child) {
        Transform[] children = GetComponentsInChildren<Transform>();

        int childIndex = System.Array.IndexOf(children, child);

        if (childIndex >= 0 && !collidingChildIndices.Contains(childIndex)) {
            collidingChildIndices.Add(childIndex);
        }
    }

    // �ڽ��� TriggerExit �̺�Ʈ ó��
    private void HandleChildTriggerExit(Collider other, Transform child) {
        // �� ��ü�� ��� �ڽ� Transform �迭 ��������
        Transform[] children = GetComponentsInChildren<Transform>();

        // �ڽ��� �ε��� ���
        int childIndex = System.Array.IndexOf(children, child);

        if (childIndex >= 0 && collidingChildIndices.Contains(childIndex)) {
            collidingChildIndices.Remove(childIndex);
        }
    }
    void Update()
    {
        transform.position = new Vector3(person.position.x, person.position.y - 0.08f, person.position.z);
        Quaternion yMinus90Rotation = Quaternion.Euler(0, -90, 0);
        Quaternion rot = person.rotation * yMinus90Rotation;
        transform.rotation = rot;

        if (collidingChildIndices.Count > 0) {
            string indices = string.Join(", ", collidingChildIndices);
            on_land = true;
        }
        else {
            on_land = false;
        }
    }

}
