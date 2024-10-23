using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Input_Delim : MonoBehaviour {

    [HideInInspector]
    public LeftDelimiter lserial;
    [HideInInspector]
    public RightDelimiter rserial;

    [HideInInspector]
    public float left_x;
    [HideInInspector]
    public float left_y;
    [HideInInspector]
    public float right_x;
    [HideInInspector]
    public float right_y;
    [HideInInspector]
    public bool reverse;

    public int far_threshold = 500;

    public enum Choice{
        LeftRight,
        FarUpDown,
        FootInput
    }
    public Choice selected;


    // Start is called before the first frame update
    void Start()
    {
        lserial = transform.Find("LDelim").GetComponent<LeftDelimiter>();
        rserial = transform.Find("RDelim").GetComponent<RightDelimiter>();
        left_x = 0;
        right_x = 0;
        left_y = 0;
        right_y = 0;
        reverse = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (selected) {
            case Choice.LeftRight: //�ؾ��ϴ� ��: y������ �����̴°���, x������ �����̴� ���� �����ؾ���.
                if(lserial.vertical && rserial.vertical) {
                    reverse = false;
                }
                else if(!(lserial.vertical || rserial.vertical)) {
                    reverse = true;
                }
                else {
                    Debug.Log("diagonal move");
                    reverse = false;
                }

                break;
            case Choice.FarUpDown: //�ؾ��ϴ� ��: x�࿡�� �󸶳� ����������
                if(lserial.accum_x < -1 * far_threshold && rserial.accum_x > far_threshold) {
                    reverse = true;
                }
                else {
                    reverse = false;
                }

                break;

            case Choice.FootInput: //�ؾ��ϴ� ��: Ư�� Ű���尡 ���ȴ���
                if (Input.GetKey(KeyCode.B)) {
                    reverse = true;
                }
                else {
                    reverse = false;
                }
                break;
        }
    }
}
