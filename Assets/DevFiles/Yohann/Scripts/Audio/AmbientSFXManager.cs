using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AmbientSFXManager : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioMixerGroup masterMixerGroup;
    public AudioMixerGroup ambientMixerGroup;
    public AudioMixerGroup positionalMixerGroup;

    [Header("Zone Management")]
    public AmbientSoundZone[] ambientZones;
    public string defaultZone = "Default";

    [Header("Positional Sounds")]
    public PositionalAmbientSound[] positionalSounds;

    [Header("Performance Settings")]
    public int maxConcurrentSources = 10;
    public float audioSourcePoolSize = 20;
    public float updateInterval = 0.1f;
    public float cullingDistance = 100f;

    [Header("VR Specific")]
    public bool useVRSpatialBlend = true;
    public float vrSpatialBlendStrength = 1f;
    public bool enableReverbZones = true;

    // Private variables
    private Dictionary<string, AmbientSoundZone> zoneDatabase;
    private Dictionary<string, PositionalAmbientSound> positionalDatabase;
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;
    private Dictionary<string, AudioSource> zoneAudioSources;
    private Dictionary<string, Coroutine> fadeCoroutines;
    private Transform playerTransform;
    private string currentZone;
    private Camera vrCamera;

    // Events
    public event Action<string> OnZoneChanged;
    public event Action<string> OnPositionalSoundTriggered;

    private void Awake()
    {
        InitializeManager();
    }

    private void Start()
    {
        SetupVRReferences();
        StartCoroutine(UpdateAmbientSounds());
    }

    private void InitializeManager()
    {
        // Initialize dictionaries
        zoneDatabase = new Dictionary<string, AmbientSoundZone>();
        positionalDatabase = new Dictionary<string, PositionalAmbientSound>();
        activeAudioSources = new List<AudioSource>();
        zoneAudioSources = new Dictionary<string, AudioSource>();
        fadeCoroutines = new Dictionary<string, Coroutine>();

        // Populate zone database
        foreach (var zone in ambientZones)
        {
            if (!zoneDatabase.ContainsKey(zone.zoneName))
            {
                zoneDatabase.Add(zone.zoneName, zone);
            }
        }

        // Populate positional sound database
        foreach (var sound in positionalSounds)
        {
            if (!positionalDatabase.ContainsKey(sound.soundId))
            {
                positionalDatabase.Add(sound.soundId, sound);
            }
        }

        // Initialize audio source pool
        InitializeAudioSourcePool();
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

    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new Queue<AudioSource>();

        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioSourceObject = new GameObject($"PooledAudioSource_{i}");
            audioSourceObject.transform.SetParent(transform);
            AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();

            // Configure for VR
            audioSource.spatialBlend = useVRSpatialBlend ? vrSpatialBlendStrength : 0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.maxDistance = 50f;
            audioSource.playOnAwake = false;

            audioSourcePool.Enqueue(audioSource);
        }
    }

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

        // Get the source that's currently playing
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

            // Fade out old zone
            if (fromSource != null)
            {
                fromSource.volume = Mathf.Lerp(startFromVolume, 0f, normalizedTime);
            }

            // Fade in new zone
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

        // Stop existing fade coroutine for this zone if it exists
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

        // Stop existing fade coroutine for this zone if it exists
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
            // Setup audio source for new zone
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
            float startVolume = 0f;

            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, normalizedTime);
                yield return null;
            }

            audioSource.volume = targetVolume;

            // Handle randomized playback
            if (zone.randomizePlayback && zone.ambientClips.Length > 1)
            {
                StartCoroutine(RandomizedPlaybackCoroutine(zone));
            }
        }
        else
        {
            // Fade out
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

        // Clean up the coroutine reference
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

            // Configure for ambient use
            zoneAudio.spatialBlend = 0f; // 2D for ambient zones
            zoneAudio.playOnAwake = false;

            zoneAudioSources[zoneName] = zoneAudio;
        }

        return zoneAudioSources[zoneName];
    }

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

        // Position the audio source
        Vector3 soundPosition = position ?? sound.sourceTransform.position;
        audioSource.transform.position = soundPosition;

        // Configure audio source
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
            StartCoroutine(ReturnAudioSourceToPool(audioSource, sound.clip.length));
        }
    }

    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        // If pool is empty, try to recycle oldest active source
        if (activeAudioSources.Count > 0)
        {
            AudioSource oldestSource = activeAudioSources[0];
            activeAudioSources.RemoveAt(0);
            oldestSource.Stop();
            return oldestSource;
        }

        return null;
    }

    private IEnumerator ReturnAudioSourceToPool(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeAudioSources.Contains(audioSource))
        {
            activeAudioSources.Remove(audioSource);
            audioSourcePool.Enqueue(audioSource);
        }
    }

    private IEnumerator UpdateAmbientSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (playerTransform == null) continue;

            // Update positional sounds based on distance
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = activeAudioSources[i];
                float distance = Vector3.Distance(playerTransform.position, source.transform.position);

                // Cull distant sounds
                if (distance > cullingDistance)
                {
                    source.Stop();
                    activeAudioSources.RemoveAt(i);
                    audioSourcePool.Enqueue(source);
                }
            }
        }
    }

    public void SetMasterVolume(float volume)
    {
        if (masterMixerGroup != null)
        {
            masterMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetAmbientVolume(float volume)
    {
        if (ambientMixerGroup != null)
        {
            ambientMixerGroup.audioMixer.SetFloat("AmbientVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetPositionalVolume(float volume)
    {
        if (positionalMixerGroup != null)
        {
            positionalMixerGroup.audioMixer.SetFloat("PositionalVolume", Mathf.Log10(volume) * 20);
        }
    }

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

    public string GetCurrentZone()
    {
        return currentZone;
    }

    public bool IsZoneActive(string zoneName)
    {
        return currentZone == zoneName;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    public void SetMixerParameter(string parameterName, float value)
    {
        if (masterMixerGroup?.audioMixer != null)
        {
            float dBValue = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            masterMixerGroup.audioMixer.SetFloat(parameterName, dBValue);
        }
    }

    public System.Collections.IEnumerator TransitionMixerParameter(string parameterName, float targetValue, float duration)
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
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw positional sound ranges
        Gizmos.color = Color.cyan;
        foreach (var sound in positionalSounds)
        {
            if (sound.sourceTransform != null)
            {
                Gizmos.DrawWireSphere(sound.sourceTransform.position, sound.maxDistance);
            }
        }

        // Draw culling distance
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, cullingDistance);
        }
    }
#endif
}