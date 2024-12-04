using System.Collections.Generic;
   using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshSelector : MonoBehaviour
{

    private FirstPersonMovement person;
    public Mesh engeneboat;
    public Mesh woodBoat;
    private MeshFilter meshFilter;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        person = GetComponentInParent<FirstPersonMovement>();
        UpdateMesh();

    }

    private void OnValidate() {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        UpdateMesh();
    }
    private void UpdateMesh() {
        switch (person.inputMethod) {
            case FirstPersonMovement.InputMethod.HandStickThrottle:
            case FirstPersonMovement.InputMethod.GamePad:
                meshFilter.mesh = engeneboat;
                break;
            case FirstPersonMovement.InputMethod.HandStickGesture:
                meshFilter.mesh = woodBoat;
                break;
        }
    }
}
