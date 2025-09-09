using System;
using UnityEngine;
using UnityEngine.InputSystem;

// INPUT MANAGER
// Handles all input events and state for VR controllers using Unity's Input System.
// It subscribes to input actions defined in a HolderData ScriptableObject, raising C# events for primary/secondary buttons, pinches, thumbsticks, and (optionally) grip/trigger actions.
// The script tracks the current state of thumbsticks, triggers, and grips, and provides utility methods to check thumbstick directions and dominant movement.
// Input can be enabled/disabled globally, and is automatically paused/resumed with the application.
// All event subscriptions are cleaned up on destroy to prevent memory leaks.

public class InputManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private HolderData holderData;
    [SerializeField] private bool enableDebugLogs = false;

    // Primary input events
    public event Action OnPrimaryButtonPressed;
    public event Action OnSecondaryButtonLeftPressed;
    public event Action OnSecondaryButtonRightPressed;

    // Hand input events
    public event Action OnPinchLeft;
    public event Action OnPinchRight;

    // Thumbstick events with detailed context
    public event Action<Vector2, InputHand> OnThumbstickInput;
    public event Action<Vector2> OnThumbstickInputAny; // For backward compatibility

    // Advanced input events
    public event Action<InputHand> OnGripPressed;
    public event Action<InputHand> OnGripReleased;
    public event Action<float, InputHand> OnTriggerChanged;

    // Input state queries
    public bool IsLeftGripPressed { get; private set; }
    public bool IsRightGripPressed { get; private set; }
    public float LeftTriggerValue { get; private set; }
    public float RightTriggerValue { get; private set; }
    public Vector2 LeftThumbstickValue { get; private set; }
    public Vector2 RightThumbstickValue { get; private set; }

    // Input enabled state
    public bool InputEnabled { get; private set; } = true;

    private bool isInitialized = false;

    public void Initialize(HolderData holderData)
    {
        if (isInitialized)
        {
            Debug.LogWarning("InputManager already initialized!");
            return;
        }

        this.holderData = holderData;

        if (holderData == null)
        {
            Debug.LogError("InputManager: HolderData is null!");
            return;
        }

        SetupInputHandlers();
        isInitialized = true;

        if (enableDebugLogs)
            Debug.Log("InputManager initialized successfully");
    }

    private void SetupInputHandlers()
    {
        // Primary buttons
        if (holderData.PrimaryButtonLeft?.action != null)
        {
            holderData.PrimaryButtonLeft.action.performed += OnPrimaryButtonLeftPerformed;
        }

        if (holderData.PrimaryButtonRight?.action != null)
        {
            holderData.PrimaryButtonRight.action.performed += OnPrimaryButtonRightPerformed;
        }

        // Secondary buttons
        if (holderData.SecondaryButtonLeft?.action != null)
        {
            holderData.SecondaryButtonLeft.action.performed += OnSecondaryLeftPerformed;
        }

        if (holderData.SecondaryButtonRight?.action != null)
        {
            holderData.SecondaryButtonRight.action.performed += OnSecondaryRightPerformed;
        }

        // Pinch/Select actions
        if (holderData.PinchLeft?.action != null)
        {
            holderData.PinchLeft.action.performed += OnPinchLeftPerformed;
        }

        if (holderData.PinchRight?.action != null)
        {
            holderData.PinchRight.action.performed += OnPinchRightPerformed;
        }

        // Thumbstick inputs
        if (holderData.ThumbstickLeft?.action != null)
        {
            holderData.ThumbstickLeft.action.started += OnThumbstickLeftStarted;
            holderData.ThumbstickLeft.action.performed += OnThumbstickLeftPerformed;
            holderData.ThumbstickLeft.action.canceled += OnThumbstickLeftCanceled;
        }

        if (holderData.ThumbstickRight?.action != null)
        {
            holderData.ThumbstickRight.action.started += OnThumbstickRightStarted;
            holderData.ThumbstickRight.action.performed += OnThumbstickRightPerformed;
            holderData.ThumbstickRight.action.canceled += OnThumbstickRightCanceled;
        }

        // Setup additional inputs if they exist in HolderData
        SetupAdvancedInputs();
    }

    private void SetupAdvancedInputs()
    {
        // This method can be extended when you have grip, trigger, etc. in HolderData
        // For now, it's a placeholder for future expansion
    }

    #region Input Event Handlers

    private void OnPrimaryButtonLeftPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (enableDebugLogs)
            Debug.Log("Primary Button Left Pressed");

        OnPrimaryButtonPressed?.Invoke();
    }

    private void OnPrimaryButtonRightPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (enableDebugLogs)
            Debug.Log("Primary Button Right Pressed");

        OnPrimaryButtonPressed?.Invoke();
    }

    private void OnSecondaryLeftPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (enableDebugLogs)
            Debug.Log("Secondary Button Left Pressed");

        OnSecondaryButtonLeftPressed?.Invoke();
    }

    private void OnSecondaryRightPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (enableDebugLogs)
            Debug.Log("Secondary Button Right Pressed");

        OnSecondaryButtonRightPressed?.Invoke();
    }

    private void OnPinchLeftPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (context.performed)
        {
            if (enableDebugLogs)
                Debug.Log("Pinch Left Performed");

            OnPinchLeft?.Invoke();
        }
    }

    private void OnPinchRightPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (context.performed)
        {
            if (enableDebugLogs)
                Debug.Log("Pinch Right Performed");

            OnPinchRight?.Invoke();
        }
    }

    private void OnThumbstickLeftStarted(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        Vector2 value = context.ReadValue<Vector2>();
        LeftThumbstickValue = value;

        if (enableDebugLogs)
            Debug.Log($"Left Thumbstick Started: {value}");

        OnThumbstickInput?.Invoke(value, InputHand.Left);
        OnThumbstickInputAny?.Invoke(value);
    }

    private void OnThumbstickLeftPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        Vector2 value = context.ReadValue<Vector2>();
        LeftThumbstickValue = value;

        if (enableDebugLogs)
            Debug.Log($"Left Thumbstick Performed: {value}");

        OnThumbstickInput?.Invoke(value, InputHand.Left);
        OnThumbstickInputAny?.Invoke(value);
    }

    private void OnThumbstickLeftCanceled(InputAction.CallbackContext context)
    {
        LeftThumbstickValue = Vector2.zero;
    }

    private void OnThumbstickRightStarted(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        Vector2 value = context.ReadValue<Vector2>();
        RightThumbstickValue = value;

        if (enableDebugLogs)
            Debug.Log($"Right Thumbstick Started: {value}");

        OnThumbstickInput?.Invoke(value, InputHand.Right);
        OnThumbstickInputAny?.Invoke(value);
    }

    private void OnThumbstickRightPerformed(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        Vector2 value = context.ReadValue<Vector2>();
        RightThumbstickValue = value;

        if (enableDebugLogs)
            Debug.Log($"Right Thumbstick Performed: {value}");

        OnThumbstickInput?.Invoke(value, InputHand.Right);
        OnThumbstickInputAny?.Invoke(value);
    }

    private void OnThumbstickRightCanceled(InputAction.CallbackContext context)
    {
        RightThumbstickValue = Vector2.zero;
    }

    #endregion

    #region Public Control Methods

    /// <summary>
    /// Enable or disable all input processing
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;

        if (enableDebugLogs)
            Debug.Log($"Input {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Temporarily disable input for a specified duration
    /// </summary>
    public void DisableInputTemporarily(float duration)
    {
        SetInputEnabled(false);
        Invoke(nameof(ReenableInput), duration);
    }

    private void ReenableInput()
    {
        SetInputEnabled(true);
    }

    /// <summary>
    /// Check if a specific thumbstick direction is being pressed
    /// </summary>
    public bool IsThumbstickDirectionPressed(InputHand hand, ThumbstickDirection direction, float threshold = 0.5f)
    {
        Vector2 value = hand == InputHand.Left ? LeftThumbstickValue : RightThumbstickValue;

        return direction switch
        {
            ThumbstickDirection.Up => value.y > threshold,
            ThumbstickDirection.Down => value.y < -threshold,
            ThumbstickDirection.Left => value.x < -threshold,
            ThumbstickDirection.Right => value.x > threshold,
            _ => false
        };
    }

    /// <summary>
    /// Get the dominant thumbstick direction for a hand
    /// </summary>
    public ThumbstickDirection GetDominantDirection(InputHand hand, float threshold = 0.5f)
    {
        Vector2 value = hand == InputHand.Left ? LeftThumbstickValue : RightThumbstickValue;

        if (value.magnitude < threshold) return ThumbstickDirection.None;

        if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
        {
            return value.x > 0 ? ThumbstickDirection.Right : ThumbstickDirection.Left;
        }
        else
        {
            return value.y > 0 ? ThumbstickDirection.Up : ThumbstickDirection.Down;
        }
    }

    #endregion

    #region Unity Lifecycle

    private void OnDestroy()
    {
        if (!isInitialized) return;
        if (holderData == null)
        {
            Debug.LogWarning("InputManager destroyed without valid HolderData.");
            return;
        }

        // Unsubscribe from all input actions
        if (holderData.PrimaryButtonLeft?.action != null)
            holderData.PrimaryButtonLeft.action.performed -= OnPrimaryButtonLeftPerformed;

        if (holderData.PrimaryButtonRight?.action != null)
            holderData.PrimaryButtonRight.action.performed -= OnPrimaryButtonRightPerformed;

        if (holderData.SecondaryButtonLeft?.action != null)
            holderData.SecondaryButtonLeft.action.performed -= OnSecondaryLeftPerformed;

        if (holderData.SecondaryButtonRight?.action != null)
            holderData.SecondaryButtonRight.action.performed -= OnSecondaryRightPerformed;

        if (holderData.PinchLeft?.action != null)
            holderData.PinchLeft.action.performed -= OnPinchLeftPerformed;

        if (holderData.PinchRight?.action != null)
            holderData.PinchRight.action.performed -= OnPinchRightPerformed;

        if (holderData.ThumbstickLeft?.action != null)
        {
            holderData.ThumbstickLeft.action.started -= OnThumbstickLeftStarted;
            holderData.ThumbstickLeft.action.performed -= OnThumbstickLeftPerformed;
            holderData.ThumbstickLeft.action.canceled -= OnThumbstickLeftCanceled;
        }

        if (holderData.ThumbstickRight?.action != null)
        {
            holderData.ThumbstickRight.action.started -= OnThumbstickRightStarted;
            holderData.ThumbstickRight.action.performed -= OnThumbstickRightPerformed;
            holderData.ThumbstickRight.action.canceled -= OnThumbstickRightCanceled;
        }

        if (enableDebugLogs)
            Debug.Log("InputManager destroyed and cleaned up");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Disable input when app is paused
        if (pauseStatus)
        {
            SetInputEnabled(false);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Re-enable input when app regains focus (if it was enabled before)
        if (hasFocus && isInitialized)
        {
            SetInputEnabled(true);
        }
    }

    #endregion
}