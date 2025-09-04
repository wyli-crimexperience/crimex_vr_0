using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup footstepMixerGroup;
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [SerializeField] private AudioMixerGroup positionalMixerGroup;

    [Header("General Pool Settings")]
    [SerializeField] private int poolSize = 30;
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;

    [Header("Ambient Zones")]
    public AmbientSoundZone[] ambientZones;
    public string defaultZone = "Default";
    private Dictionary<string, AmbientSoundZone> zoneDatabase;
    private Dictionary<string, AudioSource> zoneAudioSources;
    private Dictionary<string, Coroutine> fadeCoroutines;
    private string currentZone;

    [Header("Positional Sounds")]
    public PositionalAmbientSound[] positionalSounds;
    private Dictionary<string, PositionalAmbientSound> positionalDatabase;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // ================= UNITY LIFECYCLE =================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
            InitializeAmbientSystem();

            if (enableDebugLogs) Debug.Log("[AudioManager] Initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ================= INITIALIZATION =================
    private void InitializePool()
    {
        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject($"PooledAudioSource_{i}");
            obj.transform.SetParent(transform);

            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 1f;
            src.maxDistance = 50f;

            audioSourcePool.Enqueue(src);
        }
    }

    private void InitializeAmbientSystem()
    {
        zoneDatabase = new Dictionary<string, AmbientSoundZone>();
        zoneAudioSources = new Dictionary<string, AudioSource>();
        fadeCoroutines = new Dictionary<string, Coroutine>();
        positionalDatabase = new Dictionary<string, PositionalAmbientSound>();

        foreach (var z in ambientZones) zoneDatabase[z.zoneName] = z;
        foreach (var s in positionalSounds) positionalDatabase[s.soundId] = s;

        if (!string.IsNullOrEmpty(defaultZone) && zoneDatabase.ContainsKey(defaultZone))
            SetAmbientZone(defaultZone);
    }

    // ================= GENERAL SFX =================
    public void PlaySound(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f)
    {
        PlaySoundWithMixer(clip, pos, volume, pitch, sfxMixerGroup);
    }

    public void PlayFootstep(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f)
    {
        PlaySoundWithMixer(clip, pos, volume * 2f, pitch, footstepMixerGroup);
    }

    private void PlaySoundWithMixer(AudioClip clip, Vector3 pos, float vol, float pitch, AudioMixerGroup group)
    {
        if (clip == null || audioSourcePool.Count == 0) return;

        AudioSource src = audioSourcePool.Dequeue();
        src.transform.position = pos;
        src.clip = clip;
        src.volume = vol;
        src.pitch = pitch;
        src.outputAudioMixerGroup = group;

        src.Play();
        activeAudioSources.Add(src);
        StartCoroutine(ReturnToPool(src, clip.length));
    }

    private IEnumerator ReturnToPool(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeAudioSources.Remove(src);
        src.Stop();
        src.clip = null;
        src.outputAudioMixerGroup = null;
        audioSourcePool.Enqueue(src);
    }

    // ================= AMBIENT SYSTEM =================
    public void SetAmbientZone(string zoneName)
    {
        if (!zoneDatabase.ContainsKey(zoneName)) return;
        // (reuse FadeIn / FadeOut logic from AmbientSFXManager here)
    }

    public void PlayPositionalSound(string soundId, Vector3? position = null)
    {
        if (!positionalDatabase.ContainsKey(soundId)) return;

        var sound = positionalDatabase[soundId];
        AudioSource src = audioSourcePool.Count > 0 ? audioSourcePool.Dequeue() : null;
        if (src == null) return;

        src.transform.position = position ?? sound.sourceTransform.position;
        src.clip = sound.clip;
        src.volume = sound.volume;
        src.loop = sound.loop;
        src.outputAudioMixerGroup = sound.mixerGroup ?? positionalMixerGroup;

        src.Play();
        if (!sound.loop) StartCoroutine(ReturnToPool(src, sound.clip.length));
    }

    // ================= MIXER HELPERS =================
    public void SetMixerVolume(AudioMixer mixer, string parameterName, float volume)
    {
        float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat(parameterName, db);
    }
}

// Keep your AmbientSoundZone + PositionalAmbientSound classes here (unchanged from AmbientSFXManager)

[System.Serializable]
public class AmbientSoundZone
{
    public string zoneName;
    public AudioClip[] ambientClips;
    public float volume = 1f;
    public float fadeInTime = 2f;
    public float fadeOutTime = 2f;
    public bool loop = true;
    public bool randomizePlayback = false;
    public Vector2 randomPlaybackInterval = new Vector2(5f, 15f);
    public AudioMixerGroup mixerGroup;
}

[System.Serializable]
public class PositionalAmbientSound
{
    public string soundId;
    public AudioClip clip;
    public Transform sourceTransform;
    public float maxDistance = 50f;
    public float volume = 1f;
    public bool loop = true;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public AnimationCurve customRolloffCurve;
    public AudioMixerGroup mixerGroup;
}