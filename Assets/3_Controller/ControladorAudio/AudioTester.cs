using UnityEngine;

/// <summary>
/// Script de prueba del ControladorAudio.
/// No necesita clips asignados aquí — los toma directo del AudioSource hijo.
///
/// ══════════════════════════════════════════
///  TECLAS DE PRUEBA
/// ══════════════════════════════════════════
///
///  — REPRODUCCIÓN (usa el clip del AudioSource) —
///  C  →  PlaySFX
///  M  →  PlayMusic (loop)
///  N  →  PlayDialogue
///
///  — MUTE —
///  Q  →  Toggle Mute Música
///  W  →  Toggle Mute SFX
///  E  →  Toggle Mute Diálogo
///
///  — PAUSA / STOP —
///  P  →  Pausar todo
///  O  →  Reanudar todo
///  Espacio  →  Detener todo
/// ══════════════════════════════════════════
/// </summary>
public class AudioTester : MonoBehaviour
{
    void Update()
    {
        // ══════════════════════════════════════
        //  REPRODUCCIÓN
        // ══════════════════════════════════════

        // C — SFX (usa el clip asignado en el AudioSource SFX)
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("▶ [SFX] PlaySFX");
            ControladorAudio.Instance.PlaySFX();
        }

        // M — Música en loop (usa el clip asignado en el AudioSource Música)
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("▶ [Música] PlayMusic en loop");
            ControladorAudio.Instance.PlayMusic(loop: true);
        }

        // N — Diálogo (usa el clip asignado en el AudioSource Diálogos)
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("▶ [Diálogo] PlayDialogue");
            ControladorAudio.Instance.PlayDialogue();
        }

        // ══════════════════════════════════════
        //  MUTE
        // ══════════════════════════════════════

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("🔇 [Música] Toggle Mute");
            ControladorAudio.Instance.ToggleMuteMusic();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("🔇 [SFX] Toggle Mute");
            ControladorAudio.Instance.ToggleMuteSFX();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("🔇 [Diálogo] Toggle Mute");
            ControladorAudio.Instance.ToggleMuteDialogue();
        }

        // ══════════════════════════════════════
        //  PAUSA / STOP
        // ══════════════════════════════════════

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("⏸ PauseAll");
            ControladorAudio.Instance.PauseAll();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("▶ ResumeAll");
            ControladorAudio.Instance.ResumeAll();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("⏹ StopAll");
            ControladorAudio.Instance.StopAll();
        }
    }
}