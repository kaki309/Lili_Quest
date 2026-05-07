using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

/// <summary>
/// Controlador de audio global.
/// No guarda ningún clip internamente.
/// Recibe AudioClip por parámetro y lo reproduce en el canal correcto.
/// Patrón Singleton, persiste entre escenas.
/// </summary>
public class AudioController : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static AudioController Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════════
    //  INSPECTOR
    // ══════════════════════════════════════════════════════════════════════════

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("AudioSources (hijos del GameObject)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource dialogueSource;

    [Header("Volúmenes generales (0 a 1)")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume    = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume      = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float dialogueVolume = 1.0f;

    // ── Parámetros del AudioMixer ──────────────────────────────────────────────
    private const string MIXER_MASTER   = "MasterVolume";
    private const string MIXER_MUSIC    = "MusicVolume";
    private const string MIXER_SFX      = "SFXVolume";
    private const string MIXER_DIALOGUE = "DialogueVolume";

    // ── Estado interno ─────────────────────────────────────────────────────────
    private float _musicVol;
    private float _sfxVol;
    private float _dialogueVol;

    private bool _musicMuted;
    private bool _sfxMuted;
    private bool _dialogueMuted;

    private Coroutine _fadeCoroutine;

    // ══════════════════════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeVolumes();
    }

    private void InitializeVolumes()
    {
        _musicVol    = musicVolume;
        _sfxVol      = sfxVolume;
        _dialogueVol = dialogueVolume;

        ApplyMixerVolume(MIXER_MUSIC,    _musicVol);
        ApplyMixerVolume(MIXER_SFX,      _sfxVol);
        ApplyMixerVolume(MIXER_DIALOGUE, _dialogueVol);
    }

    private void OnValidate()
    {
        _musicVol    = musicVolume;
        _sfxVol      = sfxVolume;
        _dialogueVol = dialogueVolume;

        ApplyMixerVolume(MIXER_MUSIC,    _musicVol);
        ApplyMixerVolume(MIXER_SFX,      _sfxVol);
        ApplyMixerVolume(MIXER_DIALOGUE, _dialogueVol);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  API PÚBLICA — REPRODUCCIÓN CON AUDIOCLIP
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Reproduce un clip en el canal de Música.
    /// El clip viene desde el script que llama este método.
    /// </summary>
    public void PlayMusic(AudioClip audio, bool loop = true)
    {
        if (audio == null || musicSource == null) return;
        musicSource.clip = audio;
        musicSource.loop = loop;
        musicSource.Play();
    }

    /// <summary>
    /// Reproduce un clip en el canal de SFX.
    /// El clip viene desde el script que llama este método.
    /// </summary>
    public void PlaySFX(AudioClip audio)
    {
        if (audio == null || sfxSource == null || _sfxMuted) return;
        sfxSource.PlayOneShot(audio, _sfxVol);
    }

    /// <summary>
    /// Reproduce un clip en el canal de Diálogos.
    /// El clip viene desde el script que llama este método.
    /// Interrumpe el diálogo anterior si hay uno en curso.
    /// </summary>
    public void PlayDialogue(AudioClip audio)
    {
        if (audio == null || dialogueSource == null || _dialogueMuted) return;
        dialogueSource.Stop();
        dialogueSource.clip = audio;
        dialogueSource.Play();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  API PÚBLICA — REPRODUCCIÓN CON PATH DEL SISTEMA
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Carga y reproduce música desde un path del sistema.
    /// Ejemplo: PlayMusic(@"C:\experiencia\musica.wav")
    /// </summary>
    public void PlayMusic(string path, bool loop = true)
    {
        StartCoroutine(LoadAndPlay(path, musicSource, loop));
    }

    /// <summary>
    /// Carga y reproduce un SFX desde un path del sistema.
    /// Ejemplo: PlaySFX(@"C:\experiencia\efecto.wav")
    /// </summary>
    public void PlaySFX(string path)
    {
        StartCoroutine(LoadAndPlayOneShot(path, sfxSource, _sfxVol));
    }

    /// <summary>
    /// Carga y reproduce un diálogo desde un path del sistema.
    /// Ejemplo: PlayDialogue(@"C:\experiencia\dialogo.wav")
    /// </summary>
    public void PlayDialogue(string path)
    {
        StartCoroutine(LoadAndPlay(path, dialogueSource, false));
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CARGA DESDE PATH DEL SISTEMA
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator CargarAudioDesdePath(string path, System.Action<AudioClip> onLoaded)
    {
        string uri = "file:///" + path.Replace("\\", "/");
        AudioType audioType = GetAudioType(path);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                onLoaded?.Invoke(DownloadHandlerAudioClip.GetContent(www));
            }
            else
            {
                Debug.LogError($"[AudioController] Error cargando: {path}\n{www.error}");
                onLoaded?.Invoke(null);
            }
        }
    }

    private IEnumerator LoadAndPlay(string path, AudioSource source, bool loop)
    {
        yield return StartCoroutine(CargarAudioDesdePath(path, (clip) =>
        {
            if (clip == null || source == null) return;
            source.Stop();
            source.clip = clip;
            source.loop = loop;
            source.Play();
        }));
    }

    private IEnumerator LoadAndPlayOneShot(string path, AudioSource source, float volume)
    {
        yield return StartCoroutine(CargarAudioDesdePath(path, (clip) =>
        {
            if (clip == null || source == null) return;
            source.PlayOneShot(clip, volume);
        }));
    }

    private AudioType GetAudioType(string path)
    {
        string ext = System.IO.Path.GetExtension(path).ToLower();
        return ext switch
        {
            ".wav"  => AudioType.WAV,
            ".mp3"  => AudioType.MPEG,
            ".ogg"  => AudioType.OGGVORBIS,
            _       => AudioType.UNKNOWN
        };
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CROSSFADE
    // ══════════════════════════════════════════════════════════════════════════

    public void CrossfadeTo(AudioClip newClip, float duration = 1.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeRoutine(newClip, duration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
    {
        float half = duration / 2f;

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(_musicVol, 0f, t / half);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, _musicMuted ? 0f : _musicVol, t / half);
            yield return null;
        }

        musicSource.volume = _musicMuted ? 0f : _musicVol;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CONTROL DE VOLUMEN
    // ══════════════════════════════════════════════════════════════════════════

    public void SetMasterVolume(float vol)   => ApplyMixerVolume(MIXER_MASTER, Mathf.Clamp01(vol));

    public void SetMusicVolume(float vol)
    {
        _musicVol = Mathf.Clamp01(vol);
        if (!_musicMuted) ApplyMixerVolume(MIXER_MUSIC, _musicVol);
    }

    public void SetSFXVolume(float vol)
    {
        _sfxVol = Mathf.Clamp01(vol);
        if (!_sfxMuted) ApplyMixerVolume(MIXER_SFX, _sfxVol);
    }

    public void SetDialogueVolume(float vol)
    {
        _dialogueVol = Mathf.Clamp01(vol);
        if (!_dialogueMuted) ApplyMixerVolume(MIXER_DIALOGUE, _dialogueVol);
    }

    private void ApplyMixerVolume(string parameter, float linearValue)
    {
        if (masterMixer == null) return;
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        masterMixer.SetFloat(parameter, dB);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MUTE
    // ══════════════════════════════════════════════════════════════════════════

    public void SetMuteMusic(bool mute)
    {
        _musicMuted = mute;
        ApplyMixerVolume(MIXER_MUSIC, mute ? 0f : _musicVol);
    }

    public void SetMuteSFX(bool mute)
    {
        _sfxMuted = mute;
        ApplyMixerVolume(MIXER_SFX, mute ? 0f : _sfxVol);
    }

    public void SetMuteDialogue(bool mute)
    {
        _dialogueMuted = mute;
        ApplyMixerVolume(MIXER_DIALOGUE, mute ? 0f : _dialogueVol);
    }

    public void ToggleMuteMusic()    => SetMuteMusic(!_musicMuted);
    public void ToggleMuteSFX()      => SetMuteSFX(!_sfxMuted);
    public void ToggleMuteDialogue() => SetMuteDialogue(!_dialogueMuted);

    // ══════════════════════════════════════════════════════════════════════════
    //  PAUSA / STOP
    // ══════════════════════════════════════════════════════════════════════════

    public void PauseAll()
    {
        musicSource?.Pause();
        sfxSource?.Pause();
        dialogueSource?.Pause();
    }

    public void ResumeAll()
    {
        musicSource?.UnPause();
        sfxSource?.UnPause();
        dialogueSource?.UnPause();
    }

    public void StopAll()
    {
        musicSource?.Stop();
        sfxSource?.Stop();
        dialogueSource?.Stop();
    }

    public void StopMusic()    => musicSource?.Stop();
    public void StopSFX()      => sfxSource?.Stop();
    public void StopDialogue() => dialogueSource?.Stop();
}