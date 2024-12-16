using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController2 : MonoBehaviour
{
    private float horizontalInput, verticalInput; //players inputs for rotation and speeding
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;
    private bool isInTriggerZone = false;  // Variable to check if in trigger zone
    Rigidbody rigidBody;
    public float centreOfGravityOffset = -1f;
    // Settings
    public float motorForce = 150f; // Normal motor power
    public float breakForce = 300f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 100f;  // Normal maximum speed
    public float reducedMaxSpeed = 50f;  // Maximum speed in trigger zone

    private float currentMaxSpeed; // Maximum speed will be adjusted dynamically
                                   // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    public void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        // Set initial maximum speed
        currentMaxSpeed = maxSpeed;
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        // Maximum speed control
        LimitSpeed();
    }

    private void GetInput()
    {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");
        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");
        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        // Apply motor torque
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        // Apply braking
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    //updates wheels position and rotation
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    // Maximum speed control
    private void LimitSpeed()
    {
        if (rigidBody.linearVelocity.magnitude > currentMaxSpeed / 3.6f) // km/h to m/s conversion
        {
            rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * currentMaxSpeed / 3.6f;
        }
    }

    // Function called when entering trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpeedLimitZone"))  // Checking the tag of the trigger zone
        {
            isInTriggerZone = true;  // Set flag when entering the trigger zone
            currentMaxSpeed = reducedMaxSpeed; // Reduce maximum speed
        }
    }

    // Function called when exiting trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpeedLimitZone"))  // Checking the tag of the trigger zone
        {
            isInTriggerZone = false;  // Reset flag when exiting the trigger zone
            currentMaxSpeed = maxSpeed; // Reset maximum speed to original
        }
    }
}

