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
    [HideInInspector]
    public int zerostream;

    [HideInInspector]
    public int sum_l;
    [HideInInspector]
    public int sum_r;
    [HideInInspector]
    public int accum_lx { get; private set; }
    [HideInInspector]
    public int accum_ly { get; private set; }
    [HideInInspector]
    public int accum_rx { get; private set; }
    [HideInInspector]
    public int accum_ry { get; private set; }

    public int far_threshold = 500;

    public enum Choice{
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
        accum_lx = 0;
        accum_ly = 0;
        accum_rx = 0;
        accum_ry = 0;
        zerostream = 0;

        sum_l = 0;
        sum_r = 0;
        reverse = false;
    }

    // Update is called once per frame
    void Update()
    {
        left_y = lserial.y;
        left_x = lserial.x;
        right_x = rserial.x;
        right_y = rserial.y;
        sum_r = rserial.sum_y;
        sum_l = lserial.sum_y;
        accum_lx = lserial.save_x;
        accum_ly = lserial.save_y;
        accum_rx = rserial.save_x;
        accum_ry = rserial.save_y;

        zerostream = Mathf.Min(lserial.zerostream, rserial.zerostream);

        switch (selected) {
            case Choice.FarUpDown: //해야하는 것: x축에서 얼마나 떨어졌는지
                if(accum_lx < -1 * far_threshold && accum_rx > far_threshold) {
                    reverse = true;
                }
                else {
                    reverse = false;
                }

                break;

            case Choice.FootInput: //해야하는 것: 특정 키보드가 눌렸는지
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
