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
        // 자식의 이벤트 구독
        ChildColliderHandler.OnChildTriggerStay += HandleChildTriggerStay;
        ChildColliderHandler.OnChildTriggerExit += HandleChildTriggerExit;
    }

    private void OnDisable() {
        // 자식의 이벤트 구독 해제
        ChildColliderHandler.OnChildTriggerStay -= HandleChildTriggerStay;
        ChildColliderHandler.OnChildTriggerExit -= HandleChildTriggerExit;
    }

    // 자식의 TriggerStay 이벤트 처리
    private void HandleChildTriggerStay(Collider other, Transform child) {
        Transform[] children = GetComponentsInChildren<Transform>();

        int childIndex = System.Array.IndexOf(children, child);

        if (childIndex >= 0 && !collidingChildIndices.Contains(childIndex)) {
            collidingChildIndices.Add(childIndex);
        }
    }

    // 자식의 TriggerExit 이벤트 처리
    private void HandleChildTriggerExit(Collider other, Transform child) {
        // 이 객체의 모든 자식 Transform 배열 가져오기
        Transform[] children = GetComponentsInChildren<Transform>();

        // 자식의 인덱스 계산
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
