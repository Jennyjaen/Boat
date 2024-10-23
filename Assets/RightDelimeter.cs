/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Text;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class RightDelimiter : MonoBehaviour {
    public SerialControllerCustomDelimiter serialController;

    private byte[] sendArray;
    private FirstPersonMovement person;

    [HideInInspector]
    public int x;

    [HideInInspector]
    public int y;
    // Initialization
    [HideInInspector]
    public int stream;

    [HideInInspector]
    public int zerostream;

    [HideInInspector]
    public int sum;

    [HideInInspector]
    public int accum_x; //지금까지 총 누적된 거리
    [HideInInspector]
    public int accum_y; //지금까지 총 누적된 거리
    [HideInInspector]
    public bool vertical;

    void Start() {
        serialController = GameObject.Find("RSerial").GetComponent<SerialControllerCustomDelimiter>(); 

        person = GetComponentInParent<FirstPersonMovement>();
        if (person == null) {
            Debug.Log("Can not find person");
        }
        x = 0;
        y = 0;
        sum = 0; //y축에 대해서만 진행.
        stream = 0;
        zerostream = 0;
        accum_x = 0;
        accum_y = 0;
        vertical = true;
    }


    void printArray(byte[] array) {
        string output = "";  // 출력할 문자열을 저장할 변수

        for (int i = 0; i < array.Length; i++) {
            output += (int)(array[i] / 6) + " " + (int)(array[i] % 6) + " ";  // 배열의 값을 출력 문자열에 추가

            // groupSize 개씩 출력할 때마다 줄바꿈
            if ((i + 1) % 6 == 0) {
                output += "\n";  // groupSize마다 줄바꿈 추가
            }
        }

        // 최종 출력
        Debug.Log(output);
    }

    // Executed each frame
    void Update() {
        if (serialController == null) {
            Debug.Log("there is no right serial controller");
            return;
        }

        byte[] message = serialController.ReadSerialMessage();

        if (message == null) {
            x = 0;
            y = 0;
            return;
        }

        if (person != null) {
            sendArray = person.rarray; 
            //printArray(sendArray);
        }
        //Debug.Log(string.Join(",", sendArray));
        serialController.SendSerialMessage(sendArray);
        if (message.Length == 2) {
            y = (int)message[0];
            x = (int)message[1];
            if (y > 127) { y = 127 - y; }
            if (x > 127) { x = 127 - x; }

            accum_x += x;
            accum_y += y;
            if (y != 0) {
                
                if(sum * y < 0) {
                    sum = y;
                    stream = 1;
                }
                else {
                    stream++;
                    sum += y;
                }
                if(Mathf.Abs(sum) > 10) { zerostream = 0;}
                else { zerostream++; }
                
            }
            else {
                stream = 0;
                sum = 0;
                zerostream++;
            }
        }
        else {
            x = 0;
            y = 0;
        }

    }
}
