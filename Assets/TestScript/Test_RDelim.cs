using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_RDelim : MonoBehaviour, IDelimeter
{
    public SerialControllerCustomDelimiter serialController;

    private byte[] sendArray;
    private TestMovement person;


    [HideInInspector] public int x { get; set; }
    [HideInInspector] public int y { get; set; }
    [HideInInspector] public int stream { get; set; }
    [HideInInspector] public int zerostream { get; set; }
    [HideInInspector] public int zerostream_x { get; set; }
    [HideInInspector] public int zerostream_y { get; set; }
    [HideInInspector] public int sum_x { get; set; }
    [HideInInspector] public int sum_y { get; set; }
    

    private int accum_x; //���ݱ��� �� ������ �Ÿ�
    private int accum_y;

    [HideInInspector]
    public int save_x => accum_x; //���ݱ��� �� ������ �Ÿ�
    [HideInInspector]
    public int save_y => accum_y;


    void Start() {
        serialController = GameObject.Find("RSerial").GetComponent<SerialControllerCustomDelimiter>();

        person = GetComponentInParent<TestMovement>();
        if (person == null) {
            Debug.Log("Can not find person");
        }
        x = 0;
        y = 0;
        sum_x = 0;
        sum_y = 0;
        zerostream = 0;
        zerostream_x = 0;
        zerostream_y = 0;

        accum_x = 0;
        accum_y = 0;
    }


    void printArray(byte[] array) {
        string output = "";  // ����� ���ڿ��� ������ ����

        for (int i = 0; i < array.Length; i++) {
            output += (int)(array[i] / 6) + " " + (int)(array[i] % 6) + " ";  // �迭�� ���� ��� ���ڿ��� �߰�

            // groupSize ���� ����� ������ �ٹٲ�
            if ((i + 1) % 6 == 0) {
                output += "\n";  // groupSize���� �ٹٲ� �߰�
            }
        }

        // ���� ���
        Debug.Log(output);
    }

    // Executed each frame
    void Update() {
        if (serialController == null) {
            Debug.Log("there is no right serial controller");
            return;
        }

        int[] message = serialController.ReadSerialMessage();

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
            /*y = (int)message[0];
            x = (int)message[1];
            if (y > 127) { y = 127 - y; }
            if (x > 127) { x = 127 - x; }*/
            y = message[0];
            x = message[1];
            accum_x += x;
            accum_y += y;

            if (person.inputMethod == TestMovement.InputMethod.HandStickGesture) {
                if (x != 0 || y != 0) {
                    if (y != 0) {
                        if (sum_y * y < 0) {
                            sum_y = y;
                        }
                        else {
                            sum_y += y;
                        }
                        if (Mathf.Abs(sum_y) > 10) { zerostream_y = 0; }
                        else { zerostream_y++; }
                    }
                    else {
                        sum_y = 0;
                        zerostream_y++;
                    }
                    if (x != 0) {
                        if (sum_x * x < 0) {
                            sum_x = x;
                        }
                        else {
                            sum_x += x;
                        }
                        if (Mathf.Abs(sum_x) > 10) { zerostream_x = 0; }
                        else { zerostream_x++; }
                    }
                    else {
                        sum_x = 0;
                        zerostream_x++;
                    }
                }
                else {
                    sum_x = 0;
                    sum_y = 0;
                    zerostream_x++;
                    zerostream_y++;
                }

                zerostream = Mathf.Min(zerostream_x, zerostream_y);
            }
            else if (person.inputMethod == TestMovement.InputMethod.HandStickThrottle) {
                /*if (Mathf.Abs(accum_x) < 100 && Mathf.Abs(accum_y) < 100) {
                    zerostream++;
                }
                else {
                    zerostream = 0;
                }*/
                zerostream = 100; //������ ����Ʋ�� �þ� �����ε�, �ʿ��ұ�? ��� ����.
            }

        }
        else {
            x = 0;
            y = 0;
        }

    }
}