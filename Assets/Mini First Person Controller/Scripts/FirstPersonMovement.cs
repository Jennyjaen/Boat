using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public float turnSpeed = 5f;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rigidbody;
    private GameObject lPaddle;
    private GameObject rPaddle;

    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();



    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
    }

    void FixedUpdate()
    {
        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        // Get targetVelocity from input.
        Vector2 targetVelocity =new Vector2( Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        // Apply movement.
        rigidbody.AddTorque(0f, h* targetMovingSpeed, 0f);
        rigidbody.AddForce(transform.forward * v * targetMovingSpeed);
        //rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);
    }

    void Update()
    {
        //lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position , new Vector3(0f, 0f, 1f), 30f*Time.deltaTime);

            rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, Vector3.right , 30f * Time.deltaTime);
            lPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, Vector3.right, 30f * Time.deltaTime);
    }
}