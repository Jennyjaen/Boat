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
    }

    // Update is called once per frame
    void Update()
    {
        state = GamePad.GetState(PlayerIndex.One);

        if (state.IsConnected) {
            float LX = state.ThumbSticks.Left.X;
            float LY = state.ThumbSticks.Left.Y;
            float moveDirection = state.ThumbSticks.Left.Y; 
            Vector3 forwardMovement = transform.right * moveDirection * -5f * Time.deltaTime;

            // LX 값을 회전에 활용
            float turnDirection = state.ThumbSticks.Left.X;
            float turnAmount = turnDirection * 30f * Time.deltaTime;
            Quaternion rotation = Quaternion.Euler(0, turnAmount, 0);

            // 위치와 회전 적용
            transform.position += forwardMovement;
            transform.rotation *= rotation;
            //Debug.Log($"LX: {LX}, LY: {LY}");
        }
        else {
            Debug.Log("GamePad disconnected.");
        }

        prevState = state;
    }
}

