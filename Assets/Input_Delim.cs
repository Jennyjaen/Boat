using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Input_Delim : MonoBehaviour {

    [HideInInspector]
    public IDelimeter lserial;
    [HideInInspector]
    public IDelimeter rserial;

    [HideInInspector]
    public float left_x;
    [HideInInspector]
    public float left_y;
    [HideInInspector]
    public float right_x;
    [HideInInspector]
    public float right_y;
    [HideInInspector]
    public bool r_reverse;
    [HideInInspector]
    public bool l_reverse;
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

    [SerializeField]
    private Text LText;
    [SerializeField]
    private Text RText;
    public enum Choice{
        FarUpDown,
        FootInput
    }
    public Choice selected; 


    // Start is called before the first frame update
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if(sceneName == "main") {
            lserial = transform.Find("LDelim").GetComponent<LeftDelimiter>();
            rserial = transform.Find("RDelim").GetComponent<RightDelimiter>();
        }
        else if(sceneName == "HapticTest") {
            lserial = transform.Find("LDelim").GetComponent<Test_LDelim>();
            rserial = transform.Find("RDelim").GetComponent<Test_RDelim>();
        }

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
        r_reverse = false;
        l_reverse = false;

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
                if(accum_lx < -1 * far_threshold) {
                    l_reverse = true;
                    if (LText != null) {
                        LText.gameObject.SetActive(true);
                    }
                }
                else {
                    l_reverse = false;
                    if (LText != null) {
                        LText.gameObject.SetActive(false);
                    }
                }
                if (accum_rx > far_threshold) {
                    r_reverse = true;
                    if (RText != null) {
                        RText.gameObject.SetActive(true);
                    }
                }
                else {
                    r_reverse = false;
                    if (RText != null) {
                        RText.gameObject.SetActive(false);
                    }
                }

                break;

            case Choice.FootInput: //해야하는 것: 특정 키보드가 눌렸는지
                if (Input.GetKey(KeyCode.B)) {
                    l_reverse = true;
                    r_reverse = true;
                }
                else {
                    l_reverse = false;
                    r_reverse = false;
                }
                break;
        }
    }
}
