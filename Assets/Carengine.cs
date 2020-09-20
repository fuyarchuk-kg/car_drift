using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carengine : MonoBehaviour
{
    public Transform path;
    private List<Transform> nodes;
    private int currentNode = 0;
    private float max_angle_turn = 40f;
    public List<WheelCollider> steering_wheels;
    public List<WheelCollider> throttle_wheels;

    public float speed;
    public float steering;
    public float driftIntensity = 3f;
    public float downforce = 1.0f;

    float newSteer;

    public Rigidbody rb;
    public Transform centerOfMass;  

    // Start is called before the first frame update
    void Start()
    {
        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();

        nodes = new List<Transform>();

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != path.transform)
                nodes.Add(pathTransform[i]);
        }


        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = centerOfMass.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        ApplySteer();
        Drive();
        CheckWaypointDistance();
    }

    private void ApplySteer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint( nodes[currentNode].position);

        newSteer = (relativeVector.x / relativeVector.magnitude) * max_angle_turn;

        if (newSteer > 30)
            DriftMove( newSteer );

        foreach (WheelCollider wheel in steering_wheels)
            wheel.steerAngle = newSteer;
    }

    private void Drive()
    {
        float currentSpeed = 2 * Mathf.PI * steering_wheels[0].radius * steering_wheels[0].rpm * 60 / 1000;

        foreach (WheelCollider wheel in throttle_wheels)
            wheel.motorTorque = 300f;
    }

    private void CheckWaypointDistance()
    {
        if(Vector3.Distance(transform.position, nodes[currentNode].position) < 0.5f)
        {
            if(currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }

            foreach (WheelCollider wheel in throttle_wheels)
                wheel.motorTorque = 500;
        }
    }

    private void DriftMove( float curvature )
    {
        foreach (WheelCollider wheel in throttle_wheels)
            wheel.motorTorque = 0;

        speed = transform.InverseTransformDirection(rb.velocity).z * 3.6f;

        steering = curvature * max_angle_turn;

        Vector3 driftForce = -transform.right;
        driftForce.y = 0.0f;
        driftForce.Normalize();

        if (steering != 0)
           driftForce *= rb.mass * speed / 7f * 2000f * steering / max_angle_turn;

        Vector3 driftTorque = transform.up * 10f * steering / max_angle_turn;

        rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
        rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);
    }
}
