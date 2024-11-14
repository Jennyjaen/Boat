using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserTest : MonoBehaviour
{
    // Start is called before the first frame update
    public enum UT {
        Test2,
        Test3
    }
    public UT ut;

    public enum Environment {
        Collision,
        Grass,
        Waterfall,
        Water,
        Land,
        Moving,
        Rowing
    }

    public Environment env;
    private Environment previousEnv;
    public GameObject gameObjectA;
    void Start()
    {
        SetPositionBasedOnEnvironment();
        previousEnv = env;
    }

    // Update is called once per frame
    void Update()
    {
        if (env != previousEnv) {
            SetPositionBasedOnEnvironment();
            previousEnv = env; 
        }
    }

    private void SetPositionBasedOnEnvironment() {
        Transform targetParent = transform.Find(env.ToString());
        if (targetParent != null) {
            Transform startingPoint = targetParent.Find("StartPoint");
            if (startingPoint != null) {
                gameObjectA.transform.position = startingPoint.position;
            }
            else {
                Debug.LogWarning($"StartingPoint not found in {env} parent.");
            }
        }
        else {
            Debug.LogWarning($"No child named {env} found.");
        }
        }
    }
