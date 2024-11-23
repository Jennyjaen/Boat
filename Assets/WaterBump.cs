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
    private void OnTriggerEnter(Collider other) {
        // 충돌한 객체가 "WaterO" 태그를 가지고 있는지 확인
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
            Debug.Log($"start_bump: {start_bump} , rotationz: {rotationZ}");
            if (rotationZ >= -19) {
                if (!start_bump) { //갓 start_bump 시작
                    height = transform.position.y;
                }
                start_bump = true;
                if (before_ang > rotationZ) {
                    start_bump = false;
                    shouldLogRotation = false;
                }
            }
            before_ang = rotationZ;
        }
    }

}
