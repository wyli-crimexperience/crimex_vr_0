// NPCAnimatorController.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimatorController : MonoBehaviour
{
    [Tooltip("Reference to the SpeechManager on the root NPC object.")]
    [SerializeField] private SpeechManager speechManager;

    private Animator animator;

    // Using a hash is more performant than using a string in Update()
    private readonly int isTalkingHash = Animator.StringToHash("IsTalking");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (speechManager == null)
        {
            // Try to find it on the parent if not assigned
            speechManager = GetComponentInParent<SpeechManager>();
            if (speechManager == null)
            {
                Debug.LogError("NPCAnimatorController: SpeechManager not found! Please assign it in the Inspector.", this);
                this.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (speechManager == null) return;

        // Get the current speaking state from the single source of truth: the SpeechManager.
        bool isCurrentlySpeaking = speechManager.IsSpeaking();

        // Get the current state from the animator.
        bool isAnimatorTalking = animator.GetBool(isTalkingHash);

        // Only update the animator if the state has changed.
        if (isCurrentlySpeaking != isAnimatorTalking)
        {
            animator.SetBool(isTalkingHash, isCurrentlySpeaking);
        }
    }
}