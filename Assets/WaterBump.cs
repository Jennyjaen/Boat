using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBump : MonoBehaviour
{
    private bool shouldLogRotation = false;
    private float before_ang = 0;
    [HideInInspector]
    public bool start_bump= false;
    [HideInInspector]
    public float height = 0;
    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other) {
        // �浹�� ��ü�� "WaterO" �±׸� ������ �ִ��� Ȯ��
        if (other.CompareTag("WaterO")) {
            float rotationZ = transform.rotation.eulerAngles.z;
            if (rotationZ >=330) {
                shouldLogRotation = true;
            }
        }
        if (other.CompareTag("WaterE")) {
            shouldLogRotation = false;
            start_bump = false;
        }
    }

    private void Update() {
        if (shouldLogRotation ) {
            float rotationZ = transform.rotation.eulerAngles.z;
            if (rotationZ > 180) {
                rotationZ -= 360;
            }
            if (rotationZ >= -19.5) {
                if (!start_bump) { //�� start_bump ����
                    height = transform.position.y;
                    Debug.Log(rb.velocity.y);
                }
                start_bump = true;
                if (before_ang > rotationZ + 0.01f) {
                    start_bump = false;
                    shouldLogRotation = false;
                }
            }
            before_ang = rotationZ;
        }
    }

}
