using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;

/**
 * @file: CartVRFeedback.cs
 * @brief: Proporciona retroalimentación háptica al jugador cuando el jugador interactúa con el carrito en un entorno VR.
 *
 * Notas:
 * - La retroalimentación háptica se reproduce solo una vez mientras el dispositivo esté activo.
 * - El dispositivo debe estar en el mismo GameObject que este script.
 */
 
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

    /// <summary>
    /// Subscribes to cart movement events and logs input devices on enable, and unsubscribes on disable.
    /// </summary>
    void OnEnable()
    {
        if (cartMovement != null)
        {
            cartMovement.onImpulse += OnImpulse;
            cartMovement.onBrake += OnBrake;
        }

        LogInputDevices();
    }

    /// <summary>
    /// Unsubscribes from cart movement events to prevent memory leaks.
    /// </summary>
    void OnDisable()
    {
        if (cartMovement != null)
        {
            cartMovement.onImpulse -= OnImpulse;
            cartMovement.onBrake -= OnBrake;
        }
    }

    /// <summary> 
    /// Logs the names and IDs of all connected input devices for debugging purposes.
    /// </summary>
    private void LogInputDevices()
    {
        List<string> deviceNames = new List<string>();
        foreach (var device in InputSystem.devices)
        {
            deviceNames.Add(device.name + " (" + device.deviceId + ")");
        }
    }

    /// <summary>
    /// Handles the impulse event by sending haptic feedback to the right controller and playing the impulse sound if available.
    /// </summary>
    private void OnImpulse()
    {
        SendHapticToDevice("Right", impulseHapticIntensity, impulseHapticDuration);
        if (impulseClip != null && engineAudio != null)
        {
            engineAudio.PlayOneShot(impulseClip);
        }
    }

    /// <summary> 
    /// Handles the brake event by sending haptic feedback to the left controller and playing the brake sound if available. 
    /// </summary>
    private void OnBrake()
    {
        SendHapticToDevice("Left", brakeHapticIntensity, brakeHapticDuration);
        if (brakeClip != null && engineAudio != null)
        {
            engineAudio.PlayOneShot(brakeClip);
        }
    }

    /// <summary>
    /// Sends haptic feedback to the specified hand's controller. If no specific controller is found, it attempts to send feedback to any available controller.
    /// </summary>
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