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
    public int zerostream;
    [HideInInspector]
    public int zerostream_x;
    [HideInInspector]
    public int zerostream_y;

    [HideInInspector]
    public int sum_x;
    [HideInInspector]
    public int sum_y;

    [HideInInspector]
    public int accum_x; //지금까지 총 누적된 거리
    [HideInInspector]
    public int accum_y; //지금까지 총 누적된 거리
    [HideInInspector]
    public bool vertical;

    public float movement_ratio;

    void Start() {
        serialController = GameObject.Find("LSerial").GetComponent<SerialControllerCustomDelimiter>();

        person = GetComponentInParent<FirstPersonMovement>();
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
        vertical = true;
        movement_ratio = 0.3f;
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
            accum_x += x;
            accum_y += y;
           
            /*만약에 vertical/ horizontal을 구분하려면: vertical로 움직이다가 horizontal움직이는 케이스를 처리해주어야 함.
             * 대각선으로 움직이는 것은 invalid해야하고, 방향 바꾸는 건 전환이 되어야 함.
            이 경우 x/ y와 sum_x/ sum_y를 전부 봐야함.
            결정적으로 가로로 움직였을 때 햅틱 피드백을 어떻게 줄 것인가 -> 어떻게 줘도 부자연스러움 : 이라는 생각으로 제외함.
             */
            if(x != 0 || y!= 0) {
                if(y!= 0) {
                    if (sum_y * y < 0) {
                        sum_y = y;
                        if (vertical) { //만약에 세로로 내려가다가 x가 +, - 왔다갔다 한거면 초기화 해주면 안됨. 따라서 이 조건에만 sum_x 교체해줌.
                            sum_x = x;
                        }
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
                        if (!vertical) {
                            sum_y = y;
                        }
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
            
            if(zerostream == 0) { // 유의미한 움직임이 있었음. 수직/ 수평 움직임 구분
                if(zerostream_x == 0 && zerostream_y == 0) { // 둘 다 움직임이 10 이상 있는 상태 = 이미 어느정도 움직인 상태. threshold 체크해주면서 diagonal인지 체크.
                    float ratio;
                    if (vertical) {
                        ratio = (float)sum_x / (float)sum_y;
                    }
                    else {
                        ratio = (float)sum_y / (float)sum_x;
                    }
                    if (Mathf.Abs(ratio) < movement_ratio) { }//둘 다 움직임이 어느정도 있지만, 움직이는 방향이 바뀌지는 않음.
                    else { //중간에 방향이 바뀌었을 가능성이 있음.

                    }
                }
                else if(zerostream_x == 0) { // 초기에 확인단계
                    vertical = false; //가로로 움직임.
                }
                else if(zerostream_y == 0) { // 초기에 확인 단계
                    vertical = true; //세로로 움직임.
                }
            }




        }
        else {
            x = 0;
            y = 0;
        }
        
    }
}
