using UnityEngine;

[CreateAssetMenu(fileName = "New NPCPersonality", menuName = "Gemini/NPC Personality")]
public class NPCPersonality : ScriptableObject
{
    [Header("AI Configuration")]
    [Tooltip("The core instruction or 'soul' of the NPC. Define its role, personality, knowledge, and rules for interaction.")]
    [TextArea(15, 25)]
    public string SystemContext;

    [Header("Voice Configuration")]
    [Tooltip("The Voice ID from your ElevenLabs account.")]
    public string ElevenLabsVoiceId = "21m00Tcm4TlvDq8ikWAM"; // Default: Rachel

    [Tooltip("Voice stability (0-1). Higher values are more consistent but less expressive.")]
    [Range(0f, 1f)] public float VoiceStability = 0.7f;

    [Tooltip("Voice similarity boost (0-1). Higher values make the voice more like the original.")]
    [Range(0f, 1f)] public float VoiceSimilarityBoost = 0.7f;

    [Header("Interaction Behavior (VAD)")]
    [Tooltip("The volume threshold to start considering audio as speech.")]
    [Range(0.001f, 0.1f)] public float VadThreshold = 0.01f;

    [Tooltip("How long the player must be silent (in seconds) to end their turn.")]
    public float SilenceTimeout = 2.0f;
}