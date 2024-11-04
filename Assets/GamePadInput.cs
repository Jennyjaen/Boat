using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class GamePadInput : MonoBehaviour
{

    private GamePadState state;
    private GamePadState prevState;
    Rigidbody rigidbody;
    private float speed = 4f;
    private Transform xrRig;

    public float rotationSpeed = 20f;
    private float rotationX = 0f;
    private float rotationY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        prevState = GamePad.GetState(PlayerIndex.One);
        if (prevState.IsConnected) {
            Debug.Log("GamePad connected.");
        }
        else {
            Debug.Log("GamePad not connected.");
        }
        rigidbody = GetComponent<Rigidbody>();
        xrRig = GameObject.Find("XR Rig/Camera Offset")?.transform;
        if(xrRig == null) {
            Debug.Log("can not find camera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        state = GamePad.GetState(PlayerIndex.One);

        if (state.IsConnected) {
            float LX = state.ThumbSticks.Left.X;
            float LY = state.ThumbSticks.Left.Y;
            Vector3 forwardMovement = transform.right * LY * -5f * Time.deltaTime;
            float turnAmount = LX * 30f * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);

            transform.position += forwardMovement;
            transform.rotation *= rotation;
            //Debug.Log($"LX: {LX}, LY: {LY}");

            float rx = state.ThumbSticks.Right.X;
            float ry = state.ThumbSticks.Right.Y;

            rotationX += ry * rotationSpeed * Time.deltaTime;
            rotationY += rx * rotationSpeed * Time.deltaTime;

            rotationX = Mathf.Clamp(rotationX, -40f, 40f);
            rotationY = Mathf.Clamp(rotationY, -40f, 40f);

            if (xrRig != null) {
                xrRig.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
            }
        }
        else {
            Debug.Log("GamePad disconnected.");
        }

        prevState = state;
    }
}

