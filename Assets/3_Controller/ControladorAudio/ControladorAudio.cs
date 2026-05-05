using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Sistema de audio completo para la experiencia del museo UAO - Lili.
/// Maneja tres canales independientes: Música (BGM), Efectos (SFX) y Diálogos.
/// Usa AudioMixer de Unity para control profesional de volumen por canal.
/// Patrón Singleton — persiste entre escenas.
/// </summary>
public class AudioController : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static AudioController Instance { get; private set; }

    // ══════════════════════════════════════════════════════════════════════════
    //  INSPECTOR
    // ══════════════════════════════════════════════════════════════════════════

    [Header("Audio Mixer principal")]
    [Tooltip("Arrastra aquí el AudioMixer creado en el proyecto (Assets > Create > Audio Mixer)")]
    [SerializeField] private AudioMixer masterMixer;

    // ── Fuentes de audio (una por canal) ──────────────────────────────────────
    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource dialogueSource;

    // ── Clips de música ───────────────────────────────────────────────────────
    [Header("Clips — Música")]
    [SerializeField] private AudioClip mainTheme;
    [SerializeField] private AudioClip successJingle;   // Al completar la pieza

    // ── Clips de SFX ─────────────────────────────────────────────────────────
    [Header("Clips — Efectos de sonido")]
    [SerializeField] private AudioClip sfxPickup;
    [SerializeField] private AudioClip sfxDrop;
    [SerializeField] private AudioClip sfxCorrect;
    [SerializeField] private AudioClip sfxError;

    // ── Clips de diálogo ──────────────────────────────────────────────────────
    [Header("Clips — Diálogos / Narración")]
    [SerializeField] private AudioClip[] dialogueLines;  // Asigna en el Inspector

    // ── Volúmenes iniciales ───────────────────────────────────────────────────
    [Header("Volúmenes iniciales (0 a 1)")]
    [Range(0f, 1f)] [SerializeField] private float initialMusicVolume    = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float initialSfxVolume      = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float initialDialogueVolume = 1.0f;

    // ── Parámetros del AudioMixer (deben coincidir con los nombres expuestos) ──
    private const string MIXER_MUSIC    = "MusicVolume";
    private const string MIXER_SFX      = "SFXVolume";
    private const string MIXER_DIALOGUE = "DialogueVolume";
    private const string MIXER_MASTER   = "MasterVolume";

    // ══════════════════════════════════════════════════════════════════════════
    //  ESTADO INTERNO
    // ══════════════════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════════════════
    //  INICIALIZACIÓN
    // ══════════════════════════════════════════════════════════════════════════

    private void InitializeVolumes()
    {
        _musicVol    = initialMusicVolume;
        _sfxVol      = initialSfxVolume;
        _dialogueVol = initialDialogueVolume;

        ApplyMixerVolume(MIXER_MUSIC,    _musicVol);
        ApplyMixerVolume(MIXER_SFX,      _sfxVol);
        ApplyMixerVolume(MIXER_DIALOGUE, _dialogueVol);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CANAL: MÚSICA
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Reproduce un clip de música en loop.</summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    /// <summary>Inicia la música principal del museo.</summary>
    public void PlayMainTheme() => PlayMusic(mainTheme);

    /// <summary>Reproduce el jingle de éxito al completar la actividad.</summary>
    public void PlaySuccessJingle() => PlayMusic(successJingle, false);

    public void StopMusic()   => musicSource?.Stop();
    public void PauseMusic()  => musicSource?.Pause();
    public void ResumeMusic() => musicSource?.UnPause();

    /// <summary>
    /// Hace un fade suave entre la música actual y un nuevo clip.
    /// Ideal para transiciones entre zonas del museo.
    /// </summary>
    public void CrossfadeTo(AudioClip newClip, float duration = 1.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeRoutine(newClip, duration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
    {
        float half = duration / 2f;

        // Fade out
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(_musicVol, 0f, t / half);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, _musicMuted ? 0f : _musicVol, t / half);
            yield return null;
        }

        musicSource.volume = _musicMuted ? 0f : _musicVol;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CANAL: SFX
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Reproduce cualquier clip de efecto de sonido.</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null || _sfxMuted) return;
        sfxSource.PlayOneShot(clip, _sfxVol);
    }

    public void PlayPickup()  => PlaySFX(sfxPickup);
    public void PlayDrop()    => PlaySFX(sfxDrop);
    public void PlayCorrect() => PlaySFX(sfxCorrect);
    public void PlayError()   => PlaySFX(sfxError);

    // ══════════════════════════════════════════════════════════════════════════
    //  CANAL: DIÁLOGOS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Reproduce una línea de diálogo/narración. Interrumpe la anterior.</summary>
    public void PlayDialogue(AudioClip clip)
    {
        if (clip == null || dialogueSource == null || _dialogueMuted) return;
        dialogueSource.Stop();
        dialogueSource.clip = clip;
        dialogueSource.Play();
    }

    /// <summary>Reproduce una línea de diálogo por índice (asignados en el Inspector).</summary>
    public void PlayDialogueLine(int index)
    {
        if (dialogueLines == null || index < 0 || index >= dialogueLines.Length) return;
        PlayDialogue(dialogueLines[index]);
    }

    public void StopDialogue() => dialogueSource?.Stop();

    // ══════════════════════════════════════════════════════════════════════════
    //  CONTROL DE VOLUMEN POR CANAL
    // ══════════════════════════════════════════════════════════════════════════

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

    public void SetMasterVolume(float vol)
    {
        ApplyMixerVolume(MIXER_MASTER, Mathf.Clamp01(vol));
    }

    // ── Conversión lineal → dB (AudioMixer trabaja en decibeles) ─────────────
    private void ApplyMixerVolume(string parameter, float linearValue)
    {
        if (masterMixer == null) return;
        // Evita log(0): mínimo -80 dB
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        masterMixer.SetFloat(parameter, dB);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MUTE POR CANAL
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
    //  PAUSA GLOBAL
    // ══════════════════════════════════════════════════════════════════════════

    public void PauseAll()
    {
        musicSource?.Pause();
        sfxSource?.Pause();
        dialogueSource?.Stop();
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
}