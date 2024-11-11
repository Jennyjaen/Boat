using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclineBuo : MonoBehaviour
{
    // Start is called before the first frame update
    public float torqueAmplitude = 0.1f; // sin 함수의 진폭을 조정하는 값
    public float torqueFrequency = 1f; // sin 함수의 주파수를 조정하는 값
    private float torqueTime = 0f;

    public Transform plane;
    private float distance = 0.5f;
    private FirstPersonMovement target;
    private Rigidbody rb;
    void Start()
    {
        target = transform.GetComponent<FirstPersonMovement>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        if (target.waterincline) {
            ApplySinTorque();
            torqueTime += Time.deltaTime;
        }
        else {
            torqueTime = 0f;
        }
    }

    void MoveObject() {
        Vector3 desire = plane.position + Vector3.ProjectOnPlane(transform.position - plane.position, plane.up) + plane.up * 0.5f;
        Vector3 offset = desire - transform.position;
        rb.position += offset;
    }

    void ApplySinTorque() {
        float torqueValue = torqueAmplitude * Mathf.Sin(torqueFrequency * torqueTime);

        Vector3 localTorque = new Vector3(torqueValue, 0, 0);
        Vector3 worldTorque = transform.TransformDirection(localTorque);
        Debug.Log(worldTorque);
        rb.AddTorque(worldTorque, ForceMode.Force);
    }
}
