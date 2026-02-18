using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;

public class CartFeedbackVR : MonoBehaviour
{
    [Header("References")]
    public CartMovement cartMovement;

    [Header("Haptic Feedback")]
    public float impulseHapticIntensity = 0.5f;
    public float impulseHapticDuration = 0.1f;
    public float brakeHapticIntensity = 0.8f;
    public float brakeHapticDuration = 0.2f;

    [Header("Additional Sounds")]
    public AudioSource engineAudio;
    public AudioClip impulseClip;
    public AudioClip brakeClip;

    void OnEnable()
    {
        if (cartMovement != null)
        {
            cartMovement.onImpulse += OnImpulse;
            cartMovement.onBrake += OnBrake;
        }

        LogInputDevices();
    }

    void OnDisable()
    {
        if (cartMovement != null)
        {
            cartMovement.onImpulse -= OnImpulse;
            cartMovement.onBrake -= OnBrake;
        }
    }

    private void LogInputDevices()
    {
        List<string> deviceNames = new List<string>();
        foreach (var device in InputSystem.devices)
        {
            deviceNames.Add(device.name + " (" + device.deviceId + ")");
        }
    }

    private void OnImpulse()
    {
        SendHapticToDevice("Right", impulseHapticIntensity, impulseHapticDuration);
        if (impulseClip != null && engineAudio != null)
        {
            engineAudio.PlayOneShot(impulseClip);
        }
    }

    private void OnBrake()
    {
        SendHapticToDevice("Left", brakeHapticIntensity, brakeHapticDuration);
        if (brakeClip != null && engineAudio != null)
        {
            engineAudio.PlayOneShot(brakeClip);
        }
    }

    private void SendHapticToDevice(string hand, float intensity, float duration)
    {
        foreach (var device in InputSystem.devices)
        {
            string name = device.name.ToLower();
            string desc = device.description.ToString().ToLower();
            bool isController = name.Contains("controller") || desc.Contains("controller") || name.Contains("touch") || desc.Contains("touch");
            bool isRight = name.Contains("right") || desc.Contains("right") || hand == "Right";
            bool isLeft = name.Contains("left") || desc.Contains("left") || hand == "Left";

            if (isController && ((hand == "Right" && isRight) || (hand == "Left" && isLeft)))
            {
                if (device is XRControllerWithRumble rumbleDevice)
                {
                    rumbleDevice.SendImpulse(intensity, duration);
                    return;
                }
            }
        }

        foreach (var device in InputSystem.devices)
        {
            if (device is XRControllerWithRumble rumbleDevice)
            {
                rumbleDevice.SendImpulse(intensity, duration);
                return;
            }
        }
    }
}