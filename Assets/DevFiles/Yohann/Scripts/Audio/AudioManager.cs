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
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private float cullingDistance = 100f;

    [Header("VR Specific")]
    [SerializeField] private bool useVRSpatialBlend = true;
    [SerializeField] private float vrSpatialBlendStrength = 1f;
    [SerializeField] private bool enableReverbZones = true;

    [Header("Ambient Zones")]
    public AmbientSoundZone[] ambientZones;
    public string defaultZone = "Default";

    [Header("Positional Sounds")]
    public PositionalAmbientSound[] positionalSounds;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Private variables
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;
    private Dictionary<string, AmbientSoundZone> zoneDatabase;
    private Dictionary<string, AudioSource> zoneAudioSources;
    private Dictionary<string, Coroutine> fadeCoroutines;
    private Dictionary<string, PositionalAmbientSound> positionalDatabase;
    private string currentZone;
    private Transform playerTransform;
    private Camera vrCamera;

    // Events
    public event Action<string> OnZoneChanged;
    public event Action<string> OnPositionalSoundTriggered;

    // ================= UNITY LIFECYCLE =================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
            if (enableDebugLogs) Debug.Log("[AudioManager] Initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupVRReferences();
        StartCoroutine(UpdateAmbientSounds());
    }

    // ================= INITIALIZATION =================
    private void InitializeManager()
    {
        InitializePool();
        InitializeAmbientSystem();
    }

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
            src.spatialBlend = useVRSpatialBlend ? vrSpatialBlendStrength : 1f;
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

        // Populate databases
        foreach (var zone in ambientZones)
        {
            if (!zoneDatabase.ContainsKey(zone.zoneName))
                zoneDatabase[zone.zoneName] = zone;
        }

        foreach (var sound in positionalSounds)
        {
            if (!positionalDatabase.ContainsKey(sound.soundId))
                positionalDatabase[sound.soundId] = sound;
        }

        // Set default zone
        if (!string.IsNullOrEmpty(defaultZone) && zoneDatabase.ContainsKey(defaultZone))
            SetAmbientZone(defaultZone);
    }

    private void SetupVRReferences()
    {
        // Find VR camera (usually tagged as MainCamera in VR)
        GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
        if (mainCameraObject != null)
        {
            vrCamera = mainCameraObject.GetComponent<Camera>();
        }

        if (vrCamera == null)
        {
            vrCamera = FindFirstObjectByType<Camera>();
        }

        if (vrCamera != null)
        {
            playerTransform = vrCamera.transform;
        }
    }

    // ================= GENERAL SFX =================
    public void PlaySound(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f)
    {
        PlaySoundWithMixer(clip, pos, volume, pitch, sfxMixerGroup);
    }

    public void PlayFootstep(AudioClip clip, Vector3 pos, float volume = 1f, float pitch = 1f)
    {
        PlaySoundWithMixer(clip, pos, volume, pitch, footstepMixerGroup);
    }

    private void PlaySoundWithMixer(AudioClip clip, Vector3 pos, float vol, float pitch, AudioMixerGroup group)
    {
        if (clip == null) return;

        AudioSource src = GetPooledAudioSource();
        if (src == null) return;

        src.transform.position = pos;
        src.clip = clip;
        src.volume = vol;
        src.pitch = pitch;
        src.outputAudioMixerGroup = group;
        src.spatialBlend = useVRSpatialBlend ? vrSpatialBlendStrength : 1f;

        src.Play();
        activeAudioSources.Add(src);
        StartCoroutine(ReturnToPool(src, clip.length));
    }

    // ================= AMBIENT ZONE SYSTEM =================
    public void SetAmbientZone(string zoneName, bool forceChange = false)
    {
        if (currentZone == zoneName && !forceChange) return;

        if (!zoneDatabase.ContainsKey(zoneName))
        {
            Debug.LogWarning($"Ambient zone '{zoneName}' not found!");
            return;
        }

        string previousZone = currentZone;
        currentZone = zoneName;

        // Fade out previous zone
        if (!string.IsNullOrEmpty(previousZone) && zoneDatabase.ContainsKey(previousZone))
        {
            FadeOutZone(previousZone);
        }

        // Fade in new zone
        FadeInZone(zoneName);

        OnZoneChanged?.Invoke(zoneName);
    }

    public void SetAmbientZoneFast(string zoneName, float crossfadeTime = 0.2f)
    {
        if (currentZone == zoneName) return;

        if (!zoneDatabase.ContainsKey(zoneName))
        {
            Debug.LogWarning($"Ambient zone '{zoneName}' not found!");
            return;
        }

        string previousZone = currentZone;
        currentZone = zoneName;

        StartCoroutine(FastCrossfadeCoroutine(previousZone, zoneName, crossfadeTime));
        OnZoneChanged?.Invoke(zoneName);
    }

    public void SetAmbientZoneInstant(string zoneName)
    {
        if (!zoneDatabase.ContainsKey(zoneName))
        {
            Debug.LogWarning($"Ambient zone '{zoneName}' not found!");
            return;
        }

        // Stop all current zone audio immediately
        foreach (var zoneAudioSource in zoneAudioSources.Values)
        {
            zoneAudioSource.Stop();
        }

        // Stop all fade coroutines
        foreach (var coroutine in fadeCoroutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        fadeCoroutines.Clear();

        currentZone = zoneName;
        AmbientSoundZone zone = zoneDatabase[zoneName];
        AudioSource targetAudioSource = GetZoneAudioSource(zoneName);

        // Set up new zone immediately
        if (zone.ambientClips.Length > 0)
        {
            targetAudioSource.clip = zone.randomizePlayback ?
                zone.ambientClips[UnityEngine.Random.Range(0, zone.ambientClips.Length)] :
                zone.ambientClips[0];

            targetAudioSource.loop = zone.loop;
            targetAudioSource.outputAudioMixerGroup = zone.mixerGroup ?? ambientMixerGroup;
            targetAudioSource.volume = zone.volume;
            targetAudioSource.Play();
        }

        OnZoneChanged?.Invoke(zoneName);
    }

    private IEnumerator FastCrossfadeCoroutine(string fromZone, string toZone, float crossfadeTime)
    {
        AudioSource fromSource = null;
        AudioSource toSource = GetZoneAudioSource(toZone);

        if (!string.IsNullOrEmpty(fromZone) && zoneAudioSources.ContainsKey(fromZone))
        {
            fromSource = zoneAudioSources[fromZone];
        }

        // Setup new zone
        AmbientSoundZone toZoneData = zoneDatabase[toZone];
        if (toZoneData.ambientClips.Length > 0)
        {
            toSource.clip = toZoneData.randomizePlayback ?
                toZoneData.ambientClips[UnityEngine.Random.Range(0, toZoneData.ambientClips.Length)] :
                toZoneData.ambientClips[0];

            toSource.loop = toZoneData.loop;
            toSource.outputAudioMixerGroup = toZoneData.mixerGroup ?? ambientMixerGroup;
            toSource.volume = 0f;
            toSource.Play();
        }

        // Crossfade
        float startFromVolume = fromSource?.volume ?? 0f;
        float targetToVolume = toZoneData.volume;
        float elapsedTime = 0f;

        while (elapsedTime < crossfadeTime)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / crossfadeTime;

            if (fromSource != null)
                fromSource.volume = Mathf.Lerp(startFromVolume, 0f, normalizedTime);

            toSource.volume = Mathf.Lerp(0f, targetToVolume, normalizedTime);
            yield return null;
        }

        // Ensure final volumes
        if (fromSource != null)
        {
            fromSource.volume = 0f;
            fromSource.Stop();
        }
        toSource.volume = targetToVolume;
    }

    private void FadeInZone(string zoneName)
    {
        if (!zoneDatabase.ContainsKey(zoneName)) return;

        AmbientSoundZone zone = zoneDatabase[zoneName];

        if (fadeCoroutines.ContainsKey(zoneName) && fadeCoroutines[zoneName] != null)
        {
            StopCoroutine(fadeCoroutines[zoneName]);
            fadeCoroutines.Remove(zoneName);
        }

        fadeCoroutines[zoneName] = StartCoroutine(FadeZoneCoroutine(zone, true));
    }

    private void FadeOutZone(string zoneName)
    {
        if (!zoneDatabase.ContainsKey(zoneName)) return;

        AmbientSoundZone zone = zoneDatabase[zoneName];

        if (fadeCoroutines.ContainsKey(zoneName) && fadeCoroutines[zoneName] != null)
        {
            StopCoroutine(fadeCoroutines[zoneName]);
            fadeCoroutines.Remove(zoneName);
        }

        fadeCoroutines[zoneName] = StartCoroutine(FadeZoneCoroutine(zone, false));
    }

    private IEnumerator FadeZoneCoroutine(AmbientSoundZone zone, bool fadeIn)
    {
        AudioSource audioSource = GetZoneAudioSource(zone.zoneName);

        if (fadeIn)
        {
            if (zone.ambientClips.Length > 0)
            {
                audioSource.clip = zone.randomizePlayback ?
                    zone.ambientClips[UnityEngine.Random.Range(0, zone.ambientClips.Length)] :
                    zone.ambientClips[0];

                audioSource.loop = zone.loop;
                audioSource.outputAudioMixerGroup = zone.mixerGroup ?? ambientMixerGroup;
                audioSource.volume = 0f;
                audioSource.Play();
            }

            float targetVolume = zone.volume;
            float fadeTime = zone.fadeInTime;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeTime;
                audioSource.volume = Mathf.Lerp(0f, targetVolume, normalizedTime);
                yield return null;
            }

            audioSource.volume = targetVolume;

            if (zone.randomizePlayback && zone.ambientClips.Length > 1)
            {
                StartCoroutine(RandomizedPlaybackCoroutine(zone));
            }
        }
        else
        {
            float startVolume = audioSource.volume;
            float fadeTime = zone.fadeOutTime;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
                yield return null;
            }

            audioSource.volume = 0f;
            audioSource.Stop();
        }

        if (fadeCoroutines.ContainsKey(zone.zoneName))
        {
            fadeCoroutines.Remove(zone.zoneName);
        }
    }

    private IEnumerator RandomizedPlaybackCoroutine(AmbientSoundZone zone)
    {
        while (currentZone == zone.zoneName)
        {
            float waitTime = UnityEngine.Random.Range(zone.randomPlaybackInterval.x, zone.randomPlaybackInterval.y);
            yield return new WaitForSeconds(waitTime);

            if (currentZone == zone.zoneName && zone.ambientClips.Length > 0)
            {
                AudioSource audioSource = GetZoneAudioSource(zone.zoneName);
                AudioClip newClip = zone.ambientClips[UnityEngine.Random.Range(0, zone.ambientClips.Length)];

                if (newClip != audioSource.clip)
                {
                    audioSource.clip = newClip;
                    audioSource.Play();
                }
            }
        }
    }

    private AudioSource GetZoneAudioSource(string zoneName)
    {
        if (!zoneAudioSources.ContainsKey(zoneName))
        {
            GameObject zoneObject = new GameObject($"Zone_{zoneName}");
            zoneObject.transform.SetParent(transform);
            AudioSource zoneAudio = zoneObject.AddComponent<AudioSource>();

            zoneAudio.spatialBlend = 0f; // 2D for ambient zones
            zoneAudio.playOnAwake = false;

            zoneAudioSources[zoneName] = zoneAudio;
        }

        return zoneAudioSources[zoneName];
    }

    // ================= POSITIONAL SOUNDS =================
    public void PlayPositionalSound(string soundId, Vector3? position = null)
    {
        if (!positionalDatabase.ContainsKey(soundId))
        {
            Debug.LogWarning($"Positional sound '{soundId}' not found!");
            return;
        }

        PositionalAmbientSound sound = positionalDatabase[soundId];
        AudioSource audioSource = GetPooledAudioSource();

        if (audioSource == null)
        {
            Debug.LogWarning("No available audio sources in pool!");
            return;
        }

        Vector3 soundPosition = position ?? sound.sourceTransform.position;
        audioSource.transform.position = soundPosition;
        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.loop = sound.loop;
        audioSource.maxDistance = sound.maxDistance;
        audioSource.rolloffMode = sound.rolloffMode;
        audioSource.outputAudioMixerGroup = sound.mixerGroup ?? positionalMixerGroup;
        audioSource.spatialBlend = useVRSpatialBlend ? vrSpatialBlendStrength : 1f;

        if (sound.customRolloffCurve != null)
        {
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, sound.customRolloffCurve);
        }

        audioSource.Play();
        activeAudioSources.Add(audioSource);

        OnPositionalSoundTriggered?.Invoke(soundId);

        if (!sound.loop)
        {
            StartCoroutine(ReturnToPool(audioSource, sound.clip.length));
        }
    }

    // ================= POOLING SYSTEM =================
    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        if (activeAudioSources.Count > 0)
        {
            AudioSource oldestSource = activeAudioSources[0];
            activeAudioSources.RemoveAt(0);
            oldestSource.Stop();
            return oldestSource;
        }

        return null;
    }

    private IEnumerator ReturnToPool(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeAudioSources.Contains(audioSource))
        {
            activeAudioSources.Remove(audioSource);
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.outputAudioMixerGroup = null;
            audioSourcePool.Enqueue(audioSource);
        }
    }

    private IEnumerator UpdateAmbientSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (playerTransform == null) continue;

            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = activeAudioSources[i];
                float distance = Vector3.Distance(playerTransform.position, source.transform.position);

                if (distance > cullingDistance)
                {
                    source.Stop();
                    activeAudioSources.RemoveAt(i);
                    audioSourcePool.Enqueue(source);
                }
            }
        }
    }

    // ================= MIXER VOLUME CONTROLS =================
    public void SetMasterVolume(float volume)
    {
        SetMixerVolume(masterMixerGroup?.audioMixer, "MasterVolume", volume);
    }

    public void SetAmbientVolume(float volume)
    {
        SetMixerVolume(ambientMixerGroup?.audioMixer, "AmbientVolume", volume);
    }

    public void SetPositionalVolume(float volume)
    {
        SetMixerVolume(positionalMixerGroup?.audioMixer, "PositionalVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetMixerVolume(sfxMixerGroup?.audioMixer, "SFXVolume", volume);
    }

    public void SetFootstepVolume(float volume)
    {
        SetMixerVolume(footstepMixerGroup?.audioMixer, "FootstepVolume", volume);
    }

    public void SetMixerVolume(AudioMixer mixer, string parameterName, float volume)
    {
        if (mixer == null) return;
        float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat(parameterName, db);
    }

    public void SetMixerParameter(string parameterName, float value)
    {
        if (masterMixerGroup?.audioMixer != null)
        {
            float dBValue = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            masterMixerGroup.audioMixer.SetFloat(parameterName, dBValue);
        }
    }

    public IEnumerator TransitionMixerParameter(string parameterName, float targetValue, float duration)
    {
        if (masterMixerGroup?.audioMixer == null) yield break;

        var mixer = masterMixerGroup.audioMixer;
        mixer.GetFloat(parameterName, out float currentValue);

        float startValue = Mathf.Pow(10, currentValue / 20);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            float currentLinear = Mathf.Lerp(startValue, targetValue, normalizedTime);
            float dBValue = currentLinear > 0 ? Mathf.Log10(currentLinear) * 20 : -80f;

            mixer.SetFloat(parameterName, dBValue);
            yield return null;
        }
    }

    // ================= CONTROL METHODS =================
    public void StopAllAmbientSounds()
    {
        foreach (var audioSource in zoneAudioSources.Values)
        {
            audioSource.Stop();
        }

        foreach (var audioSource in activeAudioSources)
        {
            audioSource.Stop();
        }

        activeAudioSources.Clear();
    }

    public void PauseAllAmbientSounds()
    {
        foreach (var audioSource in zoneAudioSources.Values)
        {
            audioSource.Pause();
        }

        foreach (var audioSource in activeAudioSources)
        {
            audioSource.Pause();
        }
    }

    public void ResumeAllAmbientSounds()
    {
        foreach (var audioSource in zoneAudioSources.Values)
        {
            audioSource.UnPause();
        }

        foreach (var audioSource in activeAudioSources)
        {
            audioSource.UnPause();
        }
    }

    // ================= GETTERS =================
    public string GetCurrentZone() => currentZone;
    public bool IsZoneActive(string zoneName) => currentZone == zoneName;

    // ================= CLEANUP =================
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // ================= DEBUG GIZMOS =================
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var sound in positionalSounds)
        {
            if (sound.sourceTransform != null)
            {
                Gizmos.DrawWireSphere(sound.sourceTransform.position, sound.maxDistance);
            }
        }

        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, cullingDistance);
        }
    }
#endif
}

// ================= DATA STRUCTURES =================
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