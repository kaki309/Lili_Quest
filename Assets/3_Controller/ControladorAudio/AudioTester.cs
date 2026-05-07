using UnityEngine;

public class AudioTester : MonoBehaviour
{
    [Header("Clips de prueba")]
    [SerializeField] private AudioClip audioSFX;
    [SerializeField] private AudioClip audioMusica;
    [SerializeField] private AudioClip audioDialogo;

/// <summary>
///NOTA:
/// PAra llamar un audio se debe poner el tipo de audio que va a ser
/// por ejemplo: audiocontroller.instance.playMusic
/// la parte de playmusic es la que nos idce que tipo  es,
/// si pones play sfx es para los efectos y si ponemos playdialogue es para los dialogos
/// asi para que siga funcionandno el mixer y se puedan manejar los niveles de audio
/// 
/// </summary>


    void Update()
    {
        // C — Reproduce SFX
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Reproduciendo SFX");
            AudioController.Instance.PlaySFX(audioSFX);
        }

        // M — Reproduce Música
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Reproduciendo Música");
            AudioController.Instance.PlayMusic(audioMusica, true);
        }

        // N — Reproduce Diálogo
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Reproduciendo Diálogo");
            AudioController.Instance.PlayDialogue(audioDialogo);
        }

        // Q — Mute Música
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Toggle Mute Música");
            AudioController.Instance.ToggleMuteMusic();
        }

        // W — Mute SFX
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Toggle Mute SFX");
            AudioController.Instance.ToggleMuteSFX();
        }

        // E — Mute Diálogo
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Toggle Mute Diálogo");
            AudioController.Instance.ToggleMuteDialogue();
        }

        // P — Pausar todo
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Pausar todo");
            AudioController.Instance.PauseAll();
        }

        // O — Reanudar todo
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Reanudar todo");
            AudioController.Instance.ResumeAll();
        }

        // Espacio — Detener todo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Detener todo");
            AudioController.Instance.StopAll();
        }
    }
}