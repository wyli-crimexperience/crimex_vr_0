// NPCPersonality.cs (Upgraded for v1beta1 Features)
using UnityEngine;

[CreateAssetMenu(fileName = "New NPCPersonality", menuName = "Gemini/NPC Personality")]
public class NPCPersonality : ScriptableObject
{
    [Header("AI Configuration")]
    [Tooltip("The core instruction or 'soul' of the NPC. Define its role, personality, knowledge, and rules for interaction.")]
    [TextArea(15, 25)]
    public string SystemContext;

    [Header("ElevenLabs Voice (Primary)")]
    [Tooltip("The Voice ID from your ElevenLabs account.")]
    public string ElevenLabsVoiceId = "21m00Tcm4TlvDq8ikWAM";
    [Range(0f, 1f)] public float VoiceStability = 0.7f;
    [Range(0f, 1f)] public float VoiceSimilarityBoost = 0.7f;

    [Header("Google Cloud TTS (Fallback - v1beta1)")]
    [Tooltip("The voice name for Google Cloud TTS (e.g., 'en-US-Standard-C', 'en-US-Wavenet-F', 'en-US-Chirp3-HD-Achernar').")]
    public string GoogleTTS_VoiceName = "en-US-Chirp3-HD-Achernar";

    [Tooltip("Optional audio profiles for Google TTS (e.g., 'small-bluetooth-speaker-class-device'). Leave empty for default.")]
    public string[] GoogleTTS_EffectsProfileIds;

    [Tooltip("Controls the pitch of the synthesized voice. 0 is normal. Range: -20.0 to 20.0.")]
    [Range(-20f, 20f)] public float GoogleTTS_Pitch = 0f;

    [Tooltip("Controls the speaking rate. 1.0 is normal. Range: 0.25 to 4.0.")]
    [Range(0.25f, 4f)] public float GoogleTTS_SpeakingRate = 1.0f;

    [Header("Interaction Behavior (VAD)")]
    [Tooltip("The volume threshold to start considering audio as speech.")]
    [Range(0.001f, 0.1f)] public float VadThreshold = 0.01f;
    [Tooltip("How long the player must be silent (in seconds) to end their turn.")]
    public float SilenceTimeout = 2.0f;
}