using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[RequireComponent(typeof(Input_handler))]
[RequireComponent(typeof(Rigidbody))]

public class Car_control : MonoBehaviour
{
    public Input_handler in_data;
    public List<WheelCollider> throttle_wheels;
    public List<WheelCollider> steering_wheels;
    public float strength_coefficient = 20000f;
    public float max_turn_angle = 40f;
    public float speed;
    public bool drift;
    public bool allowDrift = true;
    public float steering;
    public float driftIntensity = 1f;
    public float downforce = 1.0f;

    public Transform centerOfMass;

    public AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

    public Rigidbody rb;

    //public StreamWriter file = new StreamWriter("drift_log.txt");

    // Start is called before the first frame update
    void Start()
    {
        in_data = GetComponent<Input_handler>();

        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = centerOfMass.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (WheelCollider wheel in throttle_wheels)
        {
            wheel.motorTorque = strength_coefficient * Time.deltaTime * in_data.throttle;
        }

        foreach (WheelCollider wheel in steering_wheels)
        {
            wheel.steerAngle = max_turn_angle * in_data.steer;
        }

        speed = transform.InverseTransformDirection(rb.velocity).z * 3.6f;

        steering = turnInputCurve.Evaluate(in_data.steer) * max_turn_angle; 

        drift = in_data.drifting && rb.velocity.sqrMagnitude > 100;

        if ( drift && allowDrift )
        {
            Vector3 driftForce = -transform.right;
            driftForce.y = 0.0f;
            driftForce.Normalize();

            if ( steering != 0 )
                driftForce *= rb.mass * speed / 7f * in_data.throttle * steering / max_turn_angle;
            Vector3 driftTorque = transform.up * 0.1f * steering / max_turn_angle;

            //file.WriteLine(steering + "   " + in_data.throttle + "   " + in_data.steer);

            rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
            rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);
        }

        // Downforce
       // rb.AddForce(-transform.up * speed * downforce);
    }
}