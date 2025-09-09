using System;
using UnityEngine;
// GameStateManager
// Manages the global state of the game scene.
// It tracks player permissions, action progress, and scene status using a serializable GameFlags class.
// The manager exposes properties for each flag, raising C# events when their values change for UI or logic updates.
// It provides methods to initialize, reset, and update state, as well as to check and write key actions (e.g., time of arrival, pulse).
// Scene lifecycle (start, complete, reset) is managed with dedicated methods and events.
// The manager supports saving/loading state, querying action availability, and calculating scene completion percentage.
// Debug utilities are included for logging and resetting state during development.

public class GameStateManager : MonoBehaviour
{
    [System.Serializable]
    public class GameFlags
    {
        [Header("Writing Permissions")]
        public bool canWriteNotepad;
        public bool canWriteForm;
        public bool canWriteEvidencePackSeal;

        [Header("Action States")]
        public bool hasCheckedTimeOfArrival;
        public bool hasCheckedPulse;
        public bool hasWrittenTimeOfArrival;
        public bool hasWrittenPulse;

        [Header("Game Values")]
        public int pulse;

        [Header("Scene Progress")]
        public bool sceneStarted;
        public bool sceneCompleted;
        public float sceneStartTime;
    }

    [Header("Game State")]
    [SerializeField] private GameFlags gameFlags = new GameFlags();
    [SerializeField] private bool enableDebugLogs = false;

    // Events for state changes
    public event Action<bool> OnCanWriteNotepadChanged;
    public event Action<bool> OnCanWriteFormChanged;
    public event Action<bool> OnCanWriteEvidencePackSealChanged;
    public event Action<bool> OnTimeOfArrivalChecked;
    public event Action<bool> OnPulseChecked;
    public event Action<bool> OnTimeOfArrivalWritten;
    public event Action<bool> OnPulseWritten;
    public event Action<int> OnPulseValueChanged;
    public event Action OnSceneStarted;
    public event Action OnSceneCompleted;

    // Properties with change notification
    public bool CanWriteNotepad
    {
        get => gameFlags.canWriteNotepad;
        set
        {
            if (gameFlags.canWriteNotepad != value)
            {
                gameFlags.canWriteNotepad = value;
                OnCanWriteNotepadChanged?.Invoke(value);
                LogStateChange($"CanWriteNotepad", value);
            }
        }
    }

    public bool CanWriteForm
    {
        get => gameFlags.canWriteForm;
        set
        {
            if (gameFlags.canWriteForm != value)
            {
                gameFlags.canWriteForm = value;
                OnCanWriteFormChanged?.Invoke(value);
                LogStateChange($"CanWriteForm", value);
            }
        }
    }

    public bool CanWriteEvidencePackSeal
    {
        get => gameFlags.canWriteEvidencePackSeal;
        set
        {
            if (gameFlags.canWriteEvidencePackSeal != value)
            {
                gameFlags.canWriteEvidencePackSeal = value;
                OnCanWriteEvidencePackSealChanged?.Invoke(value);
                LogStateChange($"CanWriteEvidencePackSeal", value);
            }
        }
    }

    public bool HasCheckedTimeOfArrival
    {
        get => gameFlags.hasCheckedTimeOfArrival;
        set
        {
            if (gameFlags.hasCheckedTimeOfArrival != value)
            {
                gameFlags.hasCheckedTimeOfArrival = value;
                OnTimeOfArrivalChecked?.Invoke(value);
                LogStateChange($"HasCheckedTimeOfArrival", value);
            }
        }
    }

    public bool HasCheckedPulse
    {
        get => gameFlags.hasCheckedPulse;
        set
        {
            if (gameFlags.hasCheckedPulse != value)
            {
                gameFlags.hasCheckedPulse = value;
                OnPulseChecked?.Invoke(value);
                LogStateChange($"HasCheckedPulse", value);
            }
        }
    }

    public bool HasWrittenTimeOfArrival
    {
        get => gameFlags.hasWrittenTimeOfArrival;
        set
        {
            if (gameFlags.hasWrittenTimeOfArrival != value)
            {
                gameFlags.hasWrittenTimeOfArrival = value;
                OnTimeOfArrivalWritten?.Invoke(value);
                LogStateChange($"HasWrittenTimeOfArrival", value);
            }
        }
    }

    public bool HasWrittenPulse
    {
        get => gameFlags.hasWrittenPulse;
        set
        {
            if (gameFlags.hasWrittenPulse != value)
            {
                gameFlags.hasWrittenPulse = value;
                OnPulseWritten?.Invoke(value);
                LogStateChange($"HasWrittenPulse", value);
            }
        }
    }

    public int Pulse
    {
        get => gameFlags.pulse;
        set
        {
            if (gameFlags.pulse != value)
            {
                gameFlags.pulse = value;
                OnPulseValueChanged?.Invoke(value);
                LogStateChange($"Pulse", value);
            }
        }
    }

    public bool SceneStarted => gameFlags.sceneStarted;
    public bool SceneCompleted => gameFlags.sceneCompleted;
    public float SceneStartTime => gameFlags.sceneStartTime;
    public float SceneElapsedTime => gameFlags.sceneStarted ? Time.time - gameFlags.sceneStartTime : 0f;

    private ThoughtManager thoughtManager;
    private TimelineManager timelineManager;

    public void Initialize(ThoughtManager thoughtManager, TimelineManager timelineManager)
    {
        this.thoughtManager = thoughtManager;
        this.timelineManager = timelineManager;

        InitializeFlags();

        if (enableDebugLogs)
            Debug.Log("GameStateManager initialized");
    }

    public void InitializeFlags()
    {
        gameFlags.canWriteNotepad = false;
        gameFlags.canWriteForm = false;
        gameFlags.canWriteEvidencePackSeal = false;
        gameFlags.hasCheckedTimeOfArrival = false;
        gameFlags.hasCheckedPulse = false;
        gameFlags.hasWrittenTimeOfArrival = false;
        gameFlags.hasWrittenPulse = false;
        gameFlags.pulse = 0;
        gameFlags.sceneStarted = false;
        gameFlags.sceneCompleted = false;
        gameFlags.sceneStartTime = 0f;

        if (enableDebugLogs)
            Debug.Log("Game flags initialized to default values");
    }

    #region Scene Management

    public void StartScene()
    {
        if (!gameFlags.sceneStarted)
        {
            gameFlags.sceneStarted = true;
            gameFlags.sceneStartTime = Time.time;
            OnSceneStarted?.Invoke();

            if (enableDebugLogs)
                Debug.Log($"Scene started at {gameFlags.sceneStartTime}");
        }
    }

    public void CompleteScene()
    {
        if (!gameFlags.sceneCompleted)
        {
            gameFlags.sceneCompleted = true;
            OnSceneCompleted?.Invoke();

            if (enableDebugLogs)
                Debug.Log($"Scene completed after {SceneElapsedTime} seconds");
        }
    }

    public void ResetScene()
    {
        InitializeFlags();

        if (enableDebugLogs)
            Debug.Log("Scene reset");
    }

    #endregion

    #region Game Logic Methods

    public void CheckWristwatch(GameObject sender)
    {
        if (!HasCheckedTimeOfArrival && timelineManager != null)
        {
            // TODO: this is only scene 1. make it adapt
            timelineManager.SetEventNow(TimelineEvent.FirstResponderArrived,
                timelineManager.GetEventTime(TimelineEvent.Incident).Value);
            HasCheckedTimeOfArrival = true;
        }

        if (thoughtManager != null)
        {
            thoughtManager.ShowThought(sender, "Current time noted...");
        }
    }

    public void CheckPulse(GameObject sender)
    {
        HasCheckedPulse = true;

        string thoughtText = Pulse == 0 ? "They have no more pulse..." : $"Pulse: {Pulse} BPM";

        if (thoughtManager != null)
        {
            thoughtManager.ShowThought(sender, thoughtText);
        }
    }

    public void WriteTimeOfArrival()
    {
        if (HasCheckedTimeOfArrival && !HasWrittenTimeOfArrival)
        {
            HasWrittenTimeOfArrival = true;

            if (enableDebugLogs)
                Debug.Log("Time of arrival written to document");
        }
    }

    public void WritePulse()
    {
        if (HasCheckedPulse && !HasWrittenPulse)
        {
            HasWrittenPulse = true;

            if (enableDebugLogs)
                Debug.Log($"Pulse ({Pulse}) written to document");
        }
    }

    #endregion

    #region State Queries

    public bool CanPerformAction(GameAction action)
    {
        return action switch
        {
            GameAction.WriteNotepad => CanWriteNotepad,
            GameAction.WriteForm => CanWriteForm,
            GameAction.WriteEvidencePackSeal => CanWriteEvidencePackSeal,
            GameAction.WriteTimeOfArrival => CanWriteNotepad && HasCheckedTimeOfArrival && !HasWrittenTimeOfArrival,
            GameAction.WritePulse => CanWriteNotepad && HasCheckedPulse && !HasWrittenPulse,
            _ => false
        };
    }

    public float GetCompletionPercentage()
    {
        int totalActions = 7; // Total checkable actions
        int completedActions = 0;

        if (HasCheckedTimeOfArrival) completedActions++;
        if (HasCheckedPulse) completedActions++;
        if (HasWrittenTimeOfArrival) completedActions++;
        if (HasWrittenPulse) completedActions++;

        // Add other completion criteria as needed

        return (float)completedActions / totalActions;
    }

    public bool IsSceneReadyToComplete()
    {
        // Define scene completion criteria
        return HasCheckedTimeOfArrival &&
               HasCheckedPulse &&
               HasWrittenTimeOfArrival &&
               HasWrittenPulse;
        // Add other criteria as needed
    }

    #endregion

    #region Save/Load State

    [System.Serializable]
    public class GameStateSaveData
    {
        public GameFlags flags;
        public float saveTime;
    }

    public GameStateSaveData GetSaveData()
    {
        return new GameStateSaveData
        {
            flags = gameFlags,
            saveTime = Time.time
        };
    }

    public void LoadSaveData(GameStateSaveData saveData)
    {
        if (saveData != null)
        {
            gameFlags = saveData.flags;

            // Trigger events for loaded state
            OnCanWriteNotepadChanged?.Invoke(CanWriteNotepad);
            OnCanWriteFormChanged?.Invoke(CanWriteForm);
            OnCanWriteEvidencePackSealChanged?.Invoke(CanWriteEvidencePackSeal);
            OnTimeOfArrivalChecked?.Invoke(HasCheckedTimeOfArrival);
            OnPulseChecked?.Invoke(HasCheckedPulse);
            OnTimeOfArrivalWritten?.Invoke(HasWrittenTimeOfArrival);
            OnPulseWritten?.Invoke(HasWrittenPulse);
            OnPulseValueChanged?.Invoke(Pulse);

            if (enableDebugLogs)
                Debug.Log($"Game state loaded from save data (saved at: {saveData.saveTime})");
        }
    }

    #endregion

    #region Debug and Utilities

    private void LogStateChange(string propertyName, object value)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"GameState: {propertyName} changed to {value}");
        }
    }

    [ContextMenu("Log Current State")]
    public void LogCurrentState()
    {
        Debug.Log($"=== GAME STATE ===\n" +
                  $"Writing Permissions:\n" +
                  $"  - CanWriteNotepad: {CanWriteNotepad}\n" +
                  $"  - CanWriteForm: {CanWriteForm}\n" +
                  $"  - CanWriteEvidencePackSeal: {CanWriteEvidencePackSeal}\n" +
                  $"Action States:\n" +
                  $"  - HasCheckedTimeOfArrival: {HasCheckedTimeOfArrival}\n" +
                  $"  - HasCheckedPulse: {HasCheckedPulse}\n" +
                  $"  - HasWrittenTimeOfArrival: {HasWrittenTimeOfArrival}\n" +
                  $"  - HasWrittenPulse: {HasWrittenPulse}\n" +
                  $"Game Values:\n" +
                  $"  - Pulse: {Pulse}\n" +
                  $"Scene Info:\n" +
                  $"  - Started: {SceneStarted}\n" +
                  $"  - Completed: {SceneCompleted}\n" +
                  $"  - Elapsed Time: {SceneElapsedTime:F2}s\n" +
                  $"  - Completion: {GetCompletionPercentage():P}");
    }

    [ContextMenu("Reset All Flags")]
    public void ResetAllFlags()
    {
        InitializeFlags();
    }

    #endregion
}