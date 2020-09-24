using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carengine : MonoBehaviour
{
    List<float> tmp = new List<float>(new float[8]);

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

    public bool is_drifting = false;

    public GradientDescent model;

    public int count = 0;

    float newSteer;

    public Rigidbody rb;
    public Transform centerOfMass;  

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

    void Update()
    {
        if( !is_drifting )
            ApplySteer();

        Drive();
        CheckWaypointDistance();

        speed = -transform.InverseTransformDirection(rb.velocity).x;

        rb.AddForce(-transform.up * speed * downforce);
    }

    private void ApplySteer()
    {
         Vector3 relativeVector = transform.InverseTransformPoint( nodes[currentNode].position);

         newSteer = (relativeVector.x / relativeVector.magnitude) * max_angle_turn;

        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 6f && currentNode % 2 != 0 )
        {
            is_drifting = true;

            List<float> weights = model.Weights();

            tmp[7] = 1;
            tmp[6] = throttle_wheels[0].motorTorque;

            Vector3 car = transform.forward;

            for (int i = 0; i < 3; i++)
                tmp[i] = car[i];

            int next_node = currentNode++;

            if (next_node == nodes.Count)
                next_node = 0;

            Vector3 node_point = nodes[next_node].position;
            Vector3 curr_pos = transform.position;

            for (int i = 0; i < 3; i++)
                tmp[i + 3] = node_point[i] - curr_pos[i];

            float drift_angle = 0;

            for (int i = 0; i < weights.Count; i++)
                drift_angle += weights[i] * tmp[i];

            Drifting( drift_angle );
        }

         foreach (WheelCollider wheel in steering_wheels)
             wheel.steerAngle = newSteer;
    }

    private void Drive()
    {
        float currentSpeed = 2 * Mathf.PI * steering_wheels[0].radius * steering_wheels[0].rpm * 60 / 1000;

        count++;

        if (count > 15)
        {
            is_drifting = false;

            count = 0;
        }

        foreach (WheelCollider wheel in throttle_wheels)
            wheel.motorTorque = 400f;
    }

    private void CheckWaypointDistance()
    {
        if( Vector3.Distance(transform.position, nodes[currentNode].position) < 2f)
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

        speed = -transform.InverseTransformDirection(rb.velocity).x;

        steering = curvature * max_angle_turn;

        Vector3 driftForce = -transform.right;
        driftForce.y = 0.0f;
        driftForce.Normalize();

        if( steering != 0 )
           driftForce *= rb.mass * speed / 7f * 2000f * steering / max_angle_turn;

        Vector3 driftTorque = transform.up * 0.5f * steering / max_angle_turn;

        rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
        rb.AddTorque(driftTorque * driftIntensity * 0.5f, ForceMode.VelocityChange);

        model.GradientStep(tmp, Vector3.Distance(transform.position, nodes[currentNode].position));
    }

    private void Drifting( float steering )
    {
        speed = transform.InverseTransformDirection( rb.velocity ).x * 3.6f;

        Vector3 driftForce = -transform.right;
        driftForce.y = 0.0f;
        driftForce.Normalize();


        if ( steering != 0 )
            driftForce *= rb.mass * speed / 7f * throttle_wheels[0].motorTorque * steering / 30;
        Vector3 driftTorque = transform.up * 0.1f * steering / 30;


        rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
        rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);
    }
}


public class GradientDescent
{
    public List<float> _weights = new List<float>( new float[8] );

    private float coeff = 0.01f;

    public List<float> Weights()
    {
        return _weights;
    }

    public void GradientStep( List<float> w, float target )
    {
        float curr_result = 0;

        for (int i = 0; i < w.Count; i++)
            curr_result += _weights[i] * w[i];

        float error = curr_result - target;

        for (int i = 0; i < w.Count; i++)
            _weights[i] -= coeff * 2 * w[i] * error;
    }
}
