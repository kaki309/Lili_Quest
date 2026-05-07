using System.Collections;
using UnityEngine;

public class SecuenciasAsistente : MonoBehaviour
{
    [Header("Pantalla Inicio")]
    [SerializeField] AudioClip audioPantallaInicio;
    [Header("Interacción Ruptura")]
    [SerializeField] AudioClip[] audiosInteracciónRuptura;
    [Header("Narrativa")]
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

    public IEnumerator RupturaModelo()
    {
        if (!getController()) yield break;

        yield return new WaitForSeconds(1.5f);

        // Paso 1: Mostrar expresión asustada con fade-in y reproducir audio asustado
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audioPrueba, "LA PRIMERA SE DESESPERA");
        yield return new WaitForSeconds(audioPrueba.length + 1f);
    }
    public IEnumerator CierreNarrativa()
    {
        if (!getController()) yield break;
    }
    public IEnumerator Visor3DLibre()
    {
        if (!getController()) yield break;
    }
}