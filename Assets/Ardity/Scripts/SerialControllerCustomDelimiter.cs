/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Threading;
using System.Collections.Generic;

/**
 * While 'SerialController' only allows reading/sending text data that is
 * terminated by new-lines, this class allows reading/sending messages 
 * using a binary protocol where each message is separated from the next by 
 * a 1-char delimiter.
 */
public class SerialControllerCustomDelimiter : MonoBehaviour
{
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM3";

    [Tooltip("Baud rate that the serial device is using to transmit data.")]
    public int baudRate = 9600;

    [Tooltip("Reference to an scene object that will receive the events of connection, " +
             "disconnection and the messages from the serial device.")]
    public GameObject messageListener;

    [Tooltip("After an error in the serial communication, or an unsuccessful " +
             "connect, how many milliseconds we should wait.")]
    public int reconnectionDelay = 1000;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public int maxUnreadMessages = 100;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public byte separator = 255;

    // Internal reference to the Thread and the object that runs in it.
    protected Thread thread;
    protected SerialThreadBinaryDelimited serialThread;


    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    void OnEnable()
    {
        serialThread = new SerialThreadBinaryDelimited(portName,
                                                       baudRate,
                                                       reconnectionDelay,
                                                       maxUnreadMessages,
                                                       separator);
        thread = new Thread(new ThreadStart(serialThread.RunForever));
        thread.Start();
    }

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is deactivated.
    // It stops and destroys the thread that was reading from the serial device.
    // ------------------------------------------------------------------------
    void OnDisable()
    {
        // If there is a user-defined tear-down function, execute it before
        // closing the underlying COM port.
        if (userDefinedTearDownFunction != null)
            userDefinedTearDownFunction();

        // The serialThread reference should never be null at this point,
        // unless an Exception happened in the OnEnable(), in which case I've
        // no idea what face Unity will make.
        if (serialThread != null)
        {
            serialThread.RequestStop();
            serialThread = null;
        }

        // This reference shouldn't be null at this point anyway.
        if (thread != null)
        {
            thread.Join();
            thread = null;
        }
    }

    // ------------------------------------------------------------------------
    // Polls messages from the queue that the SerialThread object keeps. Once a
    // message has been polled it is removed from the queue. There are some
    // special messages that mark the start/end of the communication with the
    // device.
    // ------------------------------------------------------------------------
    void Update()
    {
        // If the user prefers to poll the messages instead of receiving them
        // via SendMessage, then the message listener should be null.
        if (messageListener == null)
            return;
        //Debug.Log("this works");
        // Read the next message from the queue
        //byte[] message = ReadSerialMessage();
        //if (message == null)
        //    return;

        // Check if the message is plain data or a connect/disconnect event.
        //messageListener.SendMessage("OnMessageArrived", message);
    }

    // ------------------------------------------------------------------------
    // Returns a new unread message from the serial device. You only need to
    // call this if you don't provide a message listener.
    // ------------------------------------------------------------------------
    public int[] ReadSerialMessage()
    {
        List<byte[]> messages = new List<byte[]>();

        // serialThread에서 메시지를 null이 반환될 때까지 계속 읽어서 messages에 추가
        object messageObject;
        while ((messageObject = serialThread.ReadMessage()) != null) {
            if (messageObject is byte[]) {
                messages.Add((byte[])messageObject);
            }
            else {
                Debug.LogWarning("Expected byte[], but got: " + messageObject.GetType());
            }
        }

        if (messages.Count == 0) {
            //Debug.Log("it is null");
            return null;
        }

        int messageLength = messages[0].Length;
        int[] summedMessage = new int[messageLength];

        for (int i = 0; i < messageLength; i++) {
            int sum = 0;
            foreach (var message in messages) {
                int value = message[i];
                if (value > 127) {
                    value = 127 - value;
                }
                sum += value;
            }
            summedMessage[i] = sum;
        }

        //Debug.Log( string.Join(", ", summedMessage));
        return summedMessage; 
    }

    // ------------------------------------------------------------------------
    // Puts a message in the outgoing queue. The thread object will send the
    // message to the serial device when it considers it's appropriate.
    // ------------------------------------------------------------------------
    public void SendSerialMessage(byte[] message)
    {
        serialThread.SendMessage(message);
    }

    // ------------------------------------------------------------------------
    // Executes a user-defined function before Unity closes the COM port, so
    // the user can send some tear-down message to the hardware reliably.
    // ------------------------------------------------------------------------
    public delegate void TearDownFunction();
    private TearDownFunction userDefinedTearDownFunction;
    public void SetTearDownFunction(TearDownFunction userFunction)
    {
        this.userDefinedTearDownFunction = userFunction;
    }

}
