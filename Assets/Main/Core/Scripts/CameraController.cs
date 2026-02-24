using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("Virtual Cameras")]
    [SerializeField]
    CinemachineVirtualCamera externalVCam = null;
    [SerializeField]
    CinemachineVirtualCamera noseVCam = null;
    [SerializeField]
    CinemachineVirtualCamera cockpitVCam = null;

    [Header("Head Look (Nose/Cockpit)")]
    [SerializeField]
    float lookSensitivity = 2f;
    [SerializeField]
    float maxLookAngle = 85f;

    [Header("UI")]
    [SerializeField]
    Text cameraLabel = null;

    int currentView;
    float headYaw;
    float headPitch;
    CinemachineVirtualCamera[] vCams;
    string[] viewNames;

    private void Start()
    {
        var camList = new System.Collections.Generic.List<CinemachineVirtualCamera>();
        var nameList = new System.Collections.Generic.List<string>();

        if (externalVCam != null) { camList.Add(externalVCam); nameList.Add("EXTERNAL"); }
        if (noseVCam != null) { camList.Add(noseVCam); nameList.Add("NOSE"); }
        if (cockpitVCam != null) { camList.Add(cockpitVCam); nameList.Add("COCKPIT"); }

        vCams = camList.ToArray();
        viewNames = nameList.ToArray();

        currentView = 0;
        ApplyCameraSwitch();
    }

    private void Update()
    {
        if (vCams.Length == 0) return;

        // Cycle cameras with C
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentView = (currentView + 1) % vCams.Length;
            headYaw = 0f;
            headPitch = 0f;
            ApplyCameraSwitch();
        }

        // Reset head look with V
        if (Input.GetKeyDown(KeyCode.V))
        {
            headYaw = 0f;
            headPitch = 0f;
        }

        // Head look for nose/cockpit when holding right mouse button
        CinemachineVirtualCamera activeVCam = vCams[currentView];
        if (activeVCam != externalVCam && Input.GetMouseButton(1))
        {
            headYaw += Input.GetAxis("Mouse X") * lookSensitivity;
            headPitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            headYaw = Mathf.Clamp(headYaw, -maxLookAngle, maxLookAngle);
            headPitch = Mathf.Clamp(headPitch, -maxLookAngle, maxLookAngle);
        }

        // Apply head rotation to nose/cockpit virtual cameras
        if (activeVCam != externalVCam)
        {
            activeVCam.transform.localRotation = Quaternion.Euler(headPitch, headYaw, 0f);
        }

        if (cameraLabel != null)
        {
            cameraLabel.text = "CAM: " + viewNames[currentView];
        }
    }

    void ApplyCameraSwitch()
    {
        // Cinemachine picks the highest priority VCam
        for (int i = 0; i < vCams.Length; i++)
        {
            vCams[i].Priority = (i == currentView) ? 20 : 10;
        }
    }
}