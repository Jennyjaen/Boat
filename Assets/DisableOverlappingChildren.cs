using System.Collections.Generic;
using UnityEngine;

public class DisableOverlappingChildren : MonoBehaviour {
    void Start() {
        // �ڽ� ������Ʈ�� Transform ��ġ�� ���ϱ� ���� ��ųʸ� ����
        Dictionary<Vector3, Transform> uniquePositions = new Dictionary<Vector3, Transform>();

        // �ڽ� ������Ʈ�� �ݺ��Ͽ� ��ġ�� ��ġ���� Ȯ��
        foreach (Transform child in transform) {
            Vector3 position = child.position;

            // ��ġ�� �̹� ��ųʸ��� �ִ� ���(= ��ġ�� ���), ���� child�� ��Ȱ��ȭ
            if (uniquePositions.ContainsKey(position)) {
                child.gameObject.SetActive(false);
                Debug.Log($"��Ȱ��ȭ�� ������Ʈ �̸�: {child.gameObject.name}");
            }
            else {
                // ��ġ�� ��ġ�� �ʴ´ٸ� ��ųʸ��� �߰�
                uniquePositions[position] = child;
            }
        }
    }
}
