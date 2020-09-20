using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Input_handler : MonoBehaviour
{
    public float throttle;
    public float steer;
    public bool drifting;

    // Update is called once per frame
    void Update()
    {
        throttle = Input.GetAxis("Vertical");

        steer = Input.GetAxis("Horizontal");

        drifting = Input.GetKey(KeyCode.Space);
    }
}
