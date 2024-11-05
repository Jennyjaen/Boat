using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class GamePadInput : MonoBehaviour
{

    private GamePadState state;
    private GamePadState prevState;
    Rigidbody rb;
    private Transform xrRig;

    private float rotationY = 0f;
    private bool colliding = false;

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
        rb = GetComponent<Rigidbody>();
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
            float lx = state.ThumbSticks.Left.X;
            float ly = state.ThumbSticks.Left.Y;
            float rx = state.ThumbSticks.Right.X;
            float ry = state.ThumbSticks.Right.Y;

            if (ly != 0) {
                Vector3 forwardMovement = -transform.right * ly * 2.5f * Time.deltaTime;
                float turnAmount = ly * 15f * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
                Vector3 targetPosition = rb.position + forwardMovement;
                if (!colliding) {
                    rb.MovePosition(targetPosition);
                }
                transform.rotation *= rotation;
            }
            else {
                if (lx < 0) {
                    rotationY += lx * 20f * Time.deltaTime;
                    rotationY = Mathf.Clamp(rotationY, -40f, 40f);
                }
            }
            if (ry != 0) {
                Vector3 forwardMovement = transform.right * ry * 2.5f * Time.deltaTime;
                float turnAmount = ry * 15f * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
                Vector3 targetPosition = rb.position + forwardMovement;
                if (!colliding) {
                    rb.MovePosition(targetPosition);
                }
                transform.rotation *= rotation;
            }
            else {
                if (rx > 0) {
                    rotationY += rx * 20f * Time.deltaTime;
                    rotationY = Mathf.Clamp(rotationY, -40f, 40f);
                }
            }
            if (xrRig != null) {
                xrRig.localRotation = Quaternion.Euler(0f, rotationY, 0f);
            }
        }
        else {
            Debug.Log("GamePad disconnected.");
        }

        prevState = state;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag != "Land" &&
        collision.gameObject.tag != "Grass" &&
        collision.gameObject.tag != "Water") {
            colliding = true;
        }
    }

    private void OnCollisionExit(Collision collision) {
        colliding = false;
    }
}

