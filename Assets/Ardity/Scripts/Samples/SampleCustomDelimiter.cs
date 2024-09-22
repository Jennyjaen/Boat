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

    // Executed each frame
    void Update()
    {
        if(serialController == null) {
            Debug.Log("find serial controller");
        }
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------

        // If you press one of these keys send it to the serial device. A
        // sample serial device that accepts this input is given in the README.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Sending some action");
            // Sends a 65 (ascii for 'A') followed by an space (ascii 32, which 
            // is configured in the controller of our scene as the separator).
            serialController.SendSerialMessage(new byte[] { 65, 32 });
        }


        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        byte[] message = serialController.ReadSerialMessage();
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
        }
        //Debug.Log(string.Join(",", sendArray));
        serialController.SendSerialMessage(sendArray);
        //Debug.Log("Sending information");
        /*
        StringBuilder sb = new StringBuilder();
        foreach (byte b in message)
            sb.AppendFormat("(#{0}={1})    ", b, (char)b);
        Debug.Log("Received some bytes, printing their ascii codes: " + sb);*/
    }
}
