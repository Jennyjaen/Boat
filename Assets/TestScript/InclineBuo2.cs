using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclineBuo2 : MonoBehaviour
{

    public Transform targetObject; // 부력 적용할 오브젝트
    public float buoyancyStrength = 3.5f; // 부력 강도
    private Rigidbody rb;

    // 파도 관련 설정
    public float waveFrequency = 0.5f; // 파도 주기
    public float waveAmplitude = 2f; // 파도 높이
    public float waveSpeed = 1.0f;     // 파도 이동 속도
    private TestMovement parentScript;
    public Transform plane1;
    public Transform plane2;
    private int count = 0;
    private int frequency = 80;
    // Start is called before the first frame update
    void Start()
    {
        if (targetObject != null) {
            rb = targetObject.GetComponent<Rigidbody>();
            parentScript = targetObject.GetComponent<TestMovement>();
        }
    }

    void FixedUpdate() {
        // 부모의 waterIncline이 true일 때만 부력 적용
        if (rb != null && parentScript != null && parentScript.waterincline) {
            // 가까운 plane을 선택하고, 해당 plane의 파도 높이 계산
            Transform closestPlane = GetClosestPlane(targetObject.position, plane1, plane2);
            float waveHeight = GetWaveHeightOnPlane(targetObject.position, closestPlane, Time.time);

            if (count % frequency < (frequency / 2)) {
                if (targetObject.position.z >= transform.position.z) { // 양쪽에 번갈아가며 부력을 주기 위해 추가.
                    ApplyBuoyancy(rb, targetObject.position, waveHeight, buoyancyStrength);
                    //Debug.Log($"Left: {count}");
                }
            }
            else {
                if (targetObject.position.z < transform.position.z) {
                    ApplyBuoyancy(rb, targetObject.position, waveHeight, buoyancyStrength);
                    //Debug.Log($"Right: {count}");
                }
            }
            count++;


        }
    }


    Transform GetClosestPlane(Vector3 position, Transform plane1, Transform plane2) {
        float distanceToPlane1 = Vector3.Distance(position, plane1.position);
        float distanceToPlane2 = Vector3.Distance(position, plane2.position);
        return (distanceToPlane1 < distanceToPlane2) ? plane1 : plane2;
    }


    // 특정 plane에서의 파도 높이를 계산하는 함수
    float GetWaveHeightOnPlane(Vector3 position, Transform plane, float time) {
        //Debug.Log(time);
        // 선택된 plane의 로컬 좌표로 변환
        Vector3 localPosition = plane.InverseTransformPoint(position);
        float localWaveHeight = Mathf.Sin(localPosition.x * waveFrequency + time * waveSpeed) * waveAmplitude;
        Vector3 worldWavePosition = plane.TransformPoint(new Vector3(localPosition.x, localWaveHeight, localPosition.z));
        return worldWavePosition.y;
    }

    void ApplyBuoyancy(Rigidbody rb, Vector3 position, float waveHeight, float buoyancyStrength) {
        float objectHeight = position.y;
        //Debug.Log(objectHeight);
        float displacement = Mathf.Abs(waveHeight - objectHeight);
        if (displacement > 0) {
            //Debug.Log(objectHeight);
            Vector3 buoyancyForce = Vector3.up * displacement * buoyancyStrength;
            buoyancyForce *= (position.y * 0.4f);
            if (rb.position.y > 0.8f) {
                if (buoyancyForce.y < 0.6f) {
                    buoyancyForce.y = 0.6f;
                }
            }
            else if (rb.position.y > 0.6f) {
                if (buoyancyForce.y < 0.4f) {
                    buoyancyForce.y = 0.4f;
                }
            }
            else if (rb.position.y < 0.3f) {
                if (buoyancyForce.y > 0.3f) {
                    buoyancyForce.y = 0.3f;
                }
            }
            else {

                if (buoyancyForce.y < 0.1f) {
                    buoyancyForce.y = 0.1f;
                }
            }

            //Debug.Log($"height : {rb.transform.position.y} , and buoyancy: {buoyancyForce.y}");
            rb.AddForceAtPosition(buoyancyForce, transform.position);
            //Debug.Log($"target object: {position}, buoyancy object: {transform.position}");
        }
    }
}
