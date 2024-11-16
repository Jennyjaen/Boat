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

    [HideInInspector]
    public float rotation_m = 15f;
    private bool colliding = false;
    private GameObject lPaddle;
    private GameObject rPaddle;

    private Vector3 leftPos;
    private Quaternion leftRot;
    private Vector3 leftPos_back;
    private Quaternion leftRot_back;

    private Vector3 rightPos;
    private Quaternion rightRot;
    private Vector3 rightPos_back;
    private Quaternion rightRot_back;
    private float sum_left = 0f;
    private float sum_right = 0f;

    private float last_ly = 0;
    private float last_ry = 0;
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
        lPaddle = GameObject.Find("LPaddle");
        rPaddle = GameObject.Find("RPaddle");
        leftPos = lPaddle.transform.localPosition;
        leftRot = lPaddle.transform.localRotation;
        leftPos_back = new Vector3(-0.80f, 0.52f, -0.96f);
        leftRot_back = new Quaternion(0.44040f, -0.85550f, 0.27184f, -0.01661f);
        rightPos = rPaddle.transform.localPosition;
        rightRot = rPaddle.transform.localRotation;
        rightPos_back = new Vector3(-0.79f, 0.52f, 0.88f);
        rightRot_back = new Quaternion(0.27185f, 0.01661f, 0.44040f, 0.85550f);
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
                float turnAmount = ly * rotation_m * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
                Vector3 targetPosition = rb.position + forwardMovement;
                if (!colliding) {
                    rb.MovePosition(targetPosition);
                }
                transform.rotation *= rotation;

                
                sum_left += (ly * Time.deltaTime * 30);
                if (ly > 0) {
                    if(last_ly <= 0) { //후진하다 전진하는 케이스
                        lPaddle.transform.localPosition = leftPos;
                        lPaddle.transform.localRotation = leftRot;
                    }
                    else {
                        if(sum_left > 81) {
                            sum_left = 0;
                            lPaddle.transform.localPosition = leftPos;
                            lPaddle.transform.localRotation = leftRot;
                        }
                    }
                }
                else if(ly < 0) {
                    if (last_ly >= 0) { //전진하다 후진하는 케이스
                        lPaddle.transform.localPosition = leftPos_back;
                        lPaddle.transform.localRotation = leftRot_back;
                    }
                    else {
                        if (sum_left < -1) {
                            sum_left = 80;
                            lPaddle.transform.localPosition = leftPos_back;
                            lPaddle.transform.localRotation = leftRot_back;
                        }
                    }
                }
                lPaddle.transform.RotateAround(lPaddle.transform.GetChild(0).position, rb.transform.forward, (ly * Time.deltaTime * 30));

                last_ly = ly;
            }
            /*else {
                if (lx < 0) {
                    rotationY += lx * 20f * Time.deltaTime;
                    rotationY = Mathf.Clamp(rotationY, -40f, 40f);
                }
            }*/
            if (ry != 0) {
                ry = -1 * ry;
                Vector3 forwardMovement = -transform.right * ry * 2.5f * Time.deltaTime;
                float turnAmount = -ry * rotation_m * Time.deltaTime;
                Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);
                Vector3 targetPosition = rb.position + forwardMovement;
                if (!colliding) {
                    rb.MovePosition(targetPosition);
                }
                transform.rotation *= rotation;

                sum_right += (ry * Time.deltaTime * 30);
                if (ry > 0) {
                    if (last_ry <= 0) { //후진하다 전진하는 케이스
                        rPaddle.transform.localPosition = rightPos;
                        rPaddle.transform.localRotation = rightRot;
                    }
                    else {
                        if (sum_right > 81) {
                            sum_right = 0;
                            rPaddle.transform.localPosition = rightPos;
                            rPaddle.transform.localRotation = rightRot;
                        }
                    }
                }
                else if (ry < 0) {
                    if (last_ry >= 0) { //전진하다 후진하는 케이스
                        rPaddle.transform.localPosition = rightPos_back;
                        rPaddle.transform.localRotation = rightRot_back;
                    }
                    else {
                        if (sum_right < -1) {
                            sum_right = 80;
                            rPaddle.transform.localPosition = rightPos_back;
                            rPaddle.transform.localRotation = rightRot_back;
                        }
                    }
                }
                rPaddle.transform.RotateAround(rPaddle.transform.GetChild(0).position, rb.transform.forward, (ry * Time.deltaTime * 30));

                last_ry = ry;
            }
            /*else {
                if (rx > 0) {
                    rotationY += rx * 20f * Time.deltaTime;
                    rotationY = Mathf.Clamp(rotationY, -40f, 40f);
                }
            }
            if (xrRig != null) {
                xrRig.localRotation = Quaternion.Euler(0f, rotationY, 0f);
            }*/
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

