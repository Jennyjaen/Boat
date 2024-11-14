using System.Collections.Generic;
using UnityEngine;

public class DisableOverlappingChildren : MonoBehaviour {
    void Start() {
        // 자식 오브젝트의 Transform 위치를 비교하기 위한 딕셔너리 생성
        Dictionary<Vector3, Transform> uniquePositions = new Dictionary<Vector3, Transform>();

        // 자식 오브젝트를 반복하여 위치가 겹치는지 확인
        foreach (Transform child in transform) {
            Vector3 position = child.position;

            // 위치가 이미 딕셔너리에 있는 경우(= 겹치는 경우), 현재 child를 비활성화
            if (uniquePositions.ContainsKey(position)) {
                child.gameObject.SetActive(false);
                Debug.Log($"비활성화된 오브젝트 이름: {child.gameObject.name}");
            }
            else {
                // 위치가 겹치지 않는다면 딕셔너리에 추가
                uniquePositions[position] = child;
            }
        }
    }
}
