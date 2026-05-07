using System.Collections;
using UnityEngine;

public class SecuenciasAsistente : MonoBehaviour
{
    [Header("Pantalla Inicio")]
    [SerializeField] AudioClip audioPantallaInicio;
    [Header("Introducción antes de ruptura")]
    [SerializeField] AudioClip[] audiosIntroducciónAntesDeRuptura;
    [Header("Ruptura")]
    [SerializeField] AudioClip[] audiosRuptura;
    [Header("Inicio Narrativa")]
    [SerializeField] AudioClip audioInicioNarrativa;
    [Header("Cierre Narrativa")]
    [SerializeField] AudioClip audioCierreNarrativa;
    [Header("Visor 3D libre")]
    [SerializeField] AudioClip audioVisor3DLibre;

    ControladorAsistente controlador;
    float tiempoEspera;
    void Start()
    {
        tiempoEspera = ConfiguracionAsistente.Instance.EsperaDespuesDeAudio;
    }

    bool getController()
    {
        controlador = ControladorAsistente.Instance;

        if (controlador == null)
        {
            Debug.LogError("[SecuenciasAsistente] ControladorAsistente no disponible.");
            return false;
        }
        return true;
    }

    public IEnumerator InicioExperiencia()
    {
        if (!getController()) yield break;

        // Aquí se llama a reproducir un diálogo directamente sobre el controlador de audio

        yield return new WaitForSeconds(audioPantallaInicio.length + tiempoEspera);
    }
    public IEnumerator IntroducciónAntesDeRuptura()
    {
        if (!getController()) yield break;
        AudioClip audio;

        // Introducción a la pieza
        audio = audiosIntroducciónAntesDeRuptura[0];
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audio, "Este es un silbato en forma de perro.\nPuedes explorarlo… gíralo y acércate para ver sus detalles.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Advertencia de manipulación
        audio = audiosIntroducciónAntesDeRuptura[1];
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audio, "Pero… hazlo con cuidado.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);
    }
    public IEnumerator RupturaModelo()
    {
        if (!getController()) yield break;

        AudioClip audio;

        // Susto por ruptura
        audio = audiosRuptura[0];
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audio, "Oh… se ha roto.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Explicación de fragilidad
        audio = audiosRuptura[1];
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audio, "Las piezas reales son frágiles… y únicas.\nPor eso solo pueden ser manipuladas por expertos.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Propuesta de reparación
        audio = audiosRuptura[2];
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audio, "Pero aún podemos recuperarla.\nTe ayudaré a reconstruirla mientras descubrimos su historia.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);
    }
    public IEnumerator InicioNarrativa()
    {
        if (!getController()) yield break;

        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audioInicioNarrativa, "Te acompañaré a descubrir la historia de esta pieza.");
        yield return new WaitForSeconds(audioInicioNarrativa.length + tiempoEspera);
    }
    public IEnumerator CierreNarrativa()
    {
        if (!getController()) yield break;

        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audioCierreNarrativa, "¡Genial! Has reconstruido la pieza y comprendido su historia cultural.");
        yield return new WaitForSeconds(audioCierreNarrativa.length + tiempoEspera);
    }
    public IEnumerator Visor3DLibre()
    {
        if (!getController()) yield break;

        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audioVisor3DLibre, "Ahora puedes observarla libremente… ya no se romperá.");
        yield return new WaitForSeconds(audioCierreNarrativa.length + tiempoEspera);
    }
}