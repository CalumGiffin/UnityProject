using UnityEngine;

public class RobotDriver : MonoBehaviour
{
    public Transform controller;
    public Transform xrOrigin;
    public Transform robotBase;
    public float scale = 1.0f;

    void LateUpdate()
    {
        Vector3 local = xrOrigin.InverseTransformPoint(controller.position);
        transform.position = robotBase.position + local * scale;
        transform.rotation = controller.rotation;
    }
}