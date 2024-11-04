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
public class SampleCustomDelimiter : MonoBehaviour
{
    public SerialControllerCustomDelimiter serialController;
    public bool isLeft;

    private byte[] sendArray;
    private FirstPersonMovement person;
    // Initialization
    void Start()
    {
        if (isLeft) { serialController = GameObject.Find("LSerial").GetComponent<SerialControllerCustomDelimiter>(); }
        else { serialController = GameObject.Find("RSerial").GetComponent<SerialControllerCustomDelimiter>(); }
    
        //Debug.Log("is Left: " + isLeft);
        //Debug.Log(serialController == null);
        person = GetComponentInParent<FirstPersonMovement>();
        if(person == null) {
            Debug.Log("Can not find person");
        }
        Debug.Log("Press the SPACEBAR to execute some action");
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
    void Update()
    {
        if(serialController == null) {
            Debug.Log("find serial controller");
        }

        int[] message = serialController.ReadSerialMessage();
        //Debug.Log(message);
        if (message == null) {
            Debug.Log("no message");
            return;
        }

        if(person != null) {
            if (isLeft) {
                sendArray = person.larray;
            }
            else { sendArray = person.rarray; }
            //printArray(sendArray);
        }
        //Debug.Log(string.Join(",", sendArray));
        serialController.SendSerialMessage(sendArray);
        //Debug.Log("Sending information");
        
        StringBuilder sb = new StringBuilder();
        foreach (byte b in message)
            sb.AppendFormat("(#{0}={1})    ", b, (char)b);
        Debug.Log("Received some bytes, printing their ascii codes: " + sb);
    }
}
