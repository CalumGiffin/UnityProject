using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class HapticController : MonoBehaviour
{
    public XRNode hand = XRNode.RightHand;
    InputDevice device;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(hand);
    }

    public void Pulse(float amplitude, float duration)
    {
        if (device.isValid)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
}
