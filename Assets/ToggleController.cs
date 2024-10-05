using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour {
    public Toggle myToggle;
    public FirstPersonMovement firstPersonMovement;
    void Start() {
        myToggle.isOn = false;
        myToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn) {
        if (isOn) {
            firstPersonMovement.toggle = true;
        }
        else {
            firstPersonMovement.toggle = false;
        }
    }
}
