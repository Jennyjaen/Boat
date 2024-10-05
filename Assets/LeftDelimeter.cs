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
public class LeftDelimiter : MonoBehaviour {
    public SerialControllerCustomDelimiter serialController;

    private byte[] sendArray;
    private FirstPersonMovement person;
    // Initialization
    [HideInInspector]
    public int x;
    [HideInInspector]
    public int y;

    [HideInInspector]
    public int stream;

    [HideInInspector]
    public int sum;

    void Start() {
        serialController = GameObject.Find("LSerial").GetComponent<SerialControllerCustomDelimiter>();

        person = GetComponentInParent<FirstPersonMovement>();
        if (person == null) {
            Debug.Log("Can not find person");
        }

        x = 0;
        y = 0;
        sum = 0;
        stream = 0;
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
            Debug.Log("find serial controller");
            return;
        }

        byte[] message = serialController.ReadSerialMessage();
        
        if (message == null) {
            //Debug.Log("no message");
            x = 0;
            y = 0;
            return;
        }

        if (person != null) {
            sendArray = person.larray;
            //printArray(sendArray);
        }
        
        serialController.SendSerialMessage(sendArray);

        if(message.Length == 2) {
            y = (int)message[0];
            x = (int)message[1];
            if (y > 127) { y = 127 - y; }
            if (x > 127) { x = 127 - x; }
            if (y != 0) {
                stream++;
                sum += y;
            }
            else {
                if (stream != 0) { Debug.Log(stream); }
                if (sum != 0) { Debug.Log(sum); }
                stream = 0;
                sum = 0;
            }
        }
        else {
            x = 0;
            y = 0;
        }
        
    }
}
