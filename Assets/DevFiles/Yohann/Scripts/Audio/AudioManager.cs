using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Pool Settings")]
    [SerializeField] private int poolSize = 20;

    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup footstepMixerGroup;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1.0f;
    [SerializeField] private float sfxVolumeMultiplier = 1.0f;
    [SerializeField] private float footstepVolumeMultiplier = 2.0f; // Boost footsteps specifically

    private Queue<AudioSource> audioSourcePool;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();

            if (enableDebugLogs)
                Debug.Log("[AudioManager] AudioManager instance created and initialized.");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("[AudioManager] Duplicate AudioManager destroyed.");
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        audioSourcePool = new Queue<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObj = new GameObject($"PooledAudioSource_{i}");
            audioObj.transform.SetParent(transform);

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f; // 3D sound
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1f;
            source.maxDistance = 20f;

            audioSourcePool.Enqueue(source);
        }

        if (enableDebugLogs)
            Debug.Log($"[AudioManager] Audio pool initialized with {poolSize} AudioSources.");
    }

    // General sound play method
    // Updated PlaySound method
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        float finalVolume = volume * sfxVolumeMultiplier * masterVolume;
        if (enableDebugLogs)
            Debug.Log($"[AudioManager] PlaySound - Original: {volume}, Final: {finalVolume}");

        PlaySoundWithMixer(clip, position, finalVolume, pitch, sfxMixerGroup);
    }

    // Updated PlayFootstep method  
    public void PlayFootstep(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        float finalVolume = volume * footstepVolumeMultiplier * masterVolume;
        if (enableDebugLogs)
            Debug.Log($"[AudioManager] PlayFootstep - Original: {volume}, Multiplier: {footstepVolumeMultiplier}, Final: {finalVolume}");

        PlaySoundWithMixer(clip, position, finalVolume, pitch, footstepMixerGroup);
    }


    // Main method that handles mixer assignment
    public void PlaySoundWithMixer(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixerGroup = null)
    {
        if (clip == null)
        {
            if (enableDebugLogs)
                Debug.LogError("[AudioManager] Cannot play sound - AudioClip is null!");
            return;
        }

        if (audioSourcePool.Count == 0)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[AudioManager] No available AudioSources in pool!");
            return;
        }

        AudioSource source = audioSourcePool.Dequeue();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;

        // Assign mixer group
        source.outputAudioMixerGroup = mixerGroup ?? sfxMixerGroup;

        if (enableDebugLogs)
        {
            Debug.Log($"[AudioManager] Playing sound: {clip.name}");
            Debug.Log($"[AudioManager] AudioSource position: {source.transform.position}");
            Debug.Log($"[AudioManager] Volume: {volume}, Pitch: {pitch}");
            Debug.Log($"[AudioManager] Mixer Group: {(source.outputAudioMixerGroup ? source.outputAudioMixerGroup.name : "None")}");
            Debug.Log($"[AudioManager] Available sources in pool: {audioSourcePool.Count}");
        }

        source.Play();

        if (!source.isPlaying)
        {
            if (enableDebugLogs)
                Debug.LogError("[AudioManager] AudioSource failed to play!");
        }

        StartCoroutine(ReturnToPool(source, clip.length));
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioSourcePool != null && source != null)
        {
            // Clear the mixer group when returning to pool
            source.outputAudioMixerGroup = null;
            source.clip = null;
            audioSourcePool.Enqueue(source);

            if (enableDebugLogs)
                Debug.Log($"[AudioManager] AudioSource returned to pool. Pool size: {audioSourcePool.Count}");
        }
    }

    // Utility methods for controlling mixer volumes
    public void SetMixerVolume(AudioMixer mixer, string parameterName, float volume)
    {
        // Convert linear volume (0-1) to decibel (-80 to 0)
        float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat(parameterName, dbVolume);

        if (enableDebugLogs)
            Debug.Log($"[AudioManager] Set mixer parameter '{parameterName}' to {dbVolume}dB (linear: {volume})");
    }

    // Debug method to check pool status
    public void DebugPoolStatus()
    {
        Debug.Log($"[AudioManager] Pool Status - Available: {audioSourcePool.Count}/{poolSize}");
    }
}