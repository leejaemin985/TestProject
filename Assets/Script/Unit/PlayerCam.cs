using System.Collections;
using UnityEngine;


public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Transform camAnglePos;
    [SerializeField] private Transform camAngle;
    [SerializeField] private Transform camPos;

    [SerializeField] private float pitchMin = -100f;
    [SerializeField] private float pitchMax = 100f;

    private float row = 0f;
    private float yaw = 0f;

    private float mouseSensitivity = 300f;

    private IEnumerator CamPositioningHandle = default;

    public void SetCam()
    {
        Camera cam = Camera.main;
        cam.cullingMask = ~0 & ~(1 << LayerMask.NameToLayer("LatencyInterpolated"));

        cam.transform.SetParent(camPos);
        cam.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        camAngle.SetParent(null);

        if (CamPositioningHandle != null) StopCoroutine(CamPositioningHandle);
        StartCoroutine(CamPositioningHandle = CamPositioning());
    }

    private IEnumerator CamPositioning()
    {
        while (true)
        {
            yield return null;
            camAngle.transform.position = camAnglePos.position;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            row += mouseX;
            yaw -= mouseY;
            yaw = Mathf.Clamp(yaw, pitchMin, pitchMax);

            camAngle.localRotation = Quaternion.Euler(yaw, row, 0);
        }
    }
}