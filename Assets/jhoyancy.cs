using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jhoyancy : MonoBehaviour
{
    public Transform targetObject; // �η� ������ ������Ʈ
    public float buoyancyStrength = 3.5f; // �η� ����
    private Rigidbody rb;

    // �ĵ� ���� ����
    public float waveFrequency = 0.5f; // �ĵ� �ֱ�
    public float waveAmplitude = 2f; // �ĵ� ����
    public float waveSpeed = 1.0f;     // �ĵ� �̵� �ӵ�
    private FirstPersonMovement parentScript;
    public float phaseOffset = 1.0f;   // ���� ������
    public Transform plane1;
    public Transform plane2;
    private int count = 0;
    public int frequency = 50;
    // Start is called before the first frame update
    void Start()
    {
        if (targetObject != null) {
            rb = targetObject.GetComponent<Rigidbody>();
            parentScript = targetObject.GetComponent<FirstPersonMovement>();
        }
    }

    void FixedUpdate() {
        // �θ��� waterIncline�� true�� ���� �η� ����
        if (rb != null && parentScript != null && parentScript.waterincline) {
            // ����� plane�� �����ϰ�, �ش� plane�� �ĵ� ���� ���
            Transform closestPlane = GetClosestPlane(targetObject.position, plane1, plane2);
            float waveHeight = GetWaveHeightOnPlane(targetObject.position, closestPlane, Time.time);

            if(count % frequency < (frequency / 2)) {
                Debug.Log("Left");
                if(targetObject.position.z > transform.position.z) { // ���ʿ� �����ư��� �η��� �ֱ� ���� �߰�.
                ApplyBuoyancy(rb, targetObject.position, waveHeight, buoyancyStrength);
               }
            }
            else {
                Debug.Log("Right");
                if (targetObject.position.z < transform.position.z) {
                ApplyBuoyancy(rb, targetObject.position, waveHeight, buoyancyStrength);
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


    // Ư�� plane������ �ĵ� ���̸� ����ϴ� �Լ�
    float GetWaveHeightOnPlane(Vector3 position, Transform plane, float time) {
        // ���õ� plane�� ���� ��ǥ�� ��ȯ
        Vector3 localPosition = plane.InverseTransformPoint(position);
        float offset = (targetObject.position.z > position.z) ? phaseOffset : -phaseOffset;
        float localWaveHeight = Mathf.Sin(localPosition.x * waveFrequency + time * waveSpeed + offset) * waveAmplitude;
        Vector3 worldWavePosition = plane.TransformPoint(new Vector3(localPosition.x, localWaveHeight, localPosition.z));
        return worldWavePosition.y;
    }

    void ApplyBuoyancy(Rigidbody rb, Vector3 position, float waveHeight, float buoyancyStrength) {
        float objectHeight = position.y;
        float displacement = Mathf.Abs(waveHeight - objectHeight);
        if (displacement > 0) {
            Vector3 buoyancyForce = Vector3.up * displacement * buoyancyStrength;
            //rb.AddForce(buoyancyForce, ForceMode.Acceleration);
            rb.AddForceAtPosition(buoyancyForce, transform.position, ForceMode.Acceleration);
        }
    }
}