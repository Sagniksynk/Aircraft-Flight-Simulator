using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Header("Engine")]
    [SerializeField]
    float engineStartUpTime = 3f;
    [SerializeField]
    float engineShutDownTime = 6f;

    [Header("Propeller")]
    [SerializeField]
    Transform propeller = null;
    [SerializeField]
    float propellerMaxRPM = 2700f;
    [SerializeField]
    float propellerIdleRPM = 650f;
    [SerializeField]
    float propellerSpoolUpTime = 4f;
    [SerializeField]
    float propellerSpoolDownTime = 8f;

    [Header("Throttle")]
    [SerializeField]
    float throttleSpoolUpTime = 3f;
    [SerializeField]
    float throttleSpoolDownTime = 2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [SerializeField]
    Text displayText = null;

    float thrustPercent;
    float targetThrustPercent;
    float brakesTorque;
    float currentPropellerRPM;
    bool engineRunning;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        engineRunning = false;
        currentPropellerRPM = 0f;
    }

    private void Update()
    {
        // Engine start/stop toggle
        if (Input.GetKeyDown(KeyCode.E))
        {
            engineRunning = !engineRunning;
            if (!engineRunning)
            {
                targetThrustPercent = 0f;
            }
        }

        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        // Only allow throttle input when engine is running
        if (engineRunning && Input.GetKeyDown(KeyCode.Space))
        {
            targetThrustPercent = targetThrustPercent > 0 ? 0 : 1f;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }

        // Smoothly ramp throttle toward target
        float throttleSpeed = thrustPercent < targetThrustPercent
            ? 1f / throttleSpoolUpTime
            : 1f / throttleSpoolDownTime;
        thrustPercent = Mathf.MoveTowards(thrustPercent, targetThrustPercent, throttleSpeed * Time.deltaTime);

        UpdateHUD();
        RotatePropeller();
    }

    void UpdateHUD()
    {
        if (displayText == null) return;

        float speedKnots = rb.linearVelocity.magnitude * 1.944f;
        float altFeet = transform.position.y * 3.281f;

        displayText.text = "SPD: " + ((int)speedKnots).ToString("D3") + " kts\n";
        displayText.text += "ALT: " + ((int)altFeet).ToString("D5") + " ft\n";
        displayText.text += "THR: " + (int)(thrustPercent * 100) + "%\n";
        displayText.text += "FLP: " + (int)(Flap * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "BRK: ON\n" : "BRK: OFF\n";
        displayText.text += engineRunning ? "ENG: ON\n" : "ENG: OFF\n";
        displayText.text += "---CONTROLS---\n";
        displayText.text += "E:Engine  Space:Throttle\n";
        displayText.text += "F:Flap  B:Brake\n";
        displayText.text += "X:Drop  P:Photo  C:Cam";
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
    }

    void RotatePropeller()
    {
        if (propeller == null) return;

        float targetRPM;

        if (engineRunning)
        {
            targetRPM = Mathf.Lerp(propellerIdleRPM, propellerMaxRPM, thrustPercent);
        }
        else
        {
            targetRPM = 0f;
        }

        float rpmSpeed;
        if (currentPropellerRPM < targetRPM)
        {
            rpmSpeed = currentPropellerRPM < propellerIdleRPM
                ? propellerIdleRPM / engineStartUpTime
                : propellerMaxRPM / propellerSpoolUpTime;
        }
        else
        {
            rpmSpeed = engineRunning
                ? propellerMaxRPM / propellerSpoolDownTime
                : propellerMaxRPM / engineShutDownTime;
        }

        currentPropellerRPM = Mathf.MoveTowards(currentPropellerRPM, targetRPM, rpmSpeed * Time.deltaTime);

        float degreesPerSecond = currentPropellerRPM * 360f / 60f;
        propeller.Rotate(Vector3.forward, degreesPerSecond * Time.deltaTime);
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }
}