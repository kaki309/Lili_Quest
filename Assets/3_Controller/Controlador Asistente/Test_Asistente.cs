using UnityEngine;
using System.Collections;

/// <summary>
/// Script de prueba para el ControladorAsistente.
/// Simula el uso de la API pública y permite verificar el funcionamiento correcto.
/// </summary>
public class Test_Asistente : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;

    private void Start()
    {
        if (ControladorAsistente.Instance == null)
        {
            Debug.LogError("[Test_Asistente] ControladorAsistente no encontrado en la escena.");
            return;
        }

        // Inicia las pruebas
        Debug.Log("[Test_Asistente] Iniciando pruebas del ControladorAsistente...");
        StartCoroutine(testSecuencias());
    }

    private IEnumerator EjecutarPruebas()
    {
        ControladorAsistente asistente = ControladorAsistente.Instance;

        yield return new WaitForSeconds(6f);

        // ============ PRUEBA 1: Establecer expresión sin audio ============
        Debug.Log("[Test_Asistente] PRUEBA 1: Establecer expresión (Happy) y ocultar asistente");
        asistente.SetExpresion(ExpresionesAsistente.idle1);
        yield return new WaitForSeconds(3f);
        asistente.HideExpresion();
        yield return new WaitForSeconds(4f);

        // ============ PRUEBA 2: Reproducir diálogo ============
        Debug.Log("[Test_Asistente] PRUEBA 2: Reproducir audio y establecer imagen (Happy), luego ocultar");
        asistente.SetExpresion(ExpresionesAsistente.idle1);
        asistente.PlayDialog(audioClip, "Este es el primer diálogo de prueba.");
        yield return new WaitForSeconds(audioClip.length + 3f);
        asistente.HideExpresion();
        yield return new WaitForSeconds(4f);

        // ============ PRUEBA 3: Cambiar expresión durante reproducción ============
        Debug.Log("[Test_Asistente] PRUEBA 3: Establecer expresión y cambiarla");
        asistente.SetExpresion(ExpresionesAsistente.idle1);
        yield return new WaitForSeconds(3f);
        asistente.SetExpresion(ExpresionesAsistente.idle1);
        yield return new WaitForSeconds(3f);
        asistente.HideExpresion();
        yield return new WaitForSeconds(4f);

        // ============ PRUEBA 4: Ejecutar secuencia de ruptura ============
        Debug.Log("[Test_Asistente] PRUEBA 4: Ejecutar secuencia de ruptura");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.RupturaModelo());

        Debug.Log("[Test_Asistente] ===== TODAS LAS PRUEBAS COMPLETADAS =====");
    }

    private IEnumerator testSecuencias()
    {
        ControladorAsistente asistente = ControladorAsistente.Instance;

        Debug.Log("[Test_Asistente] Ejecutando InicioExperiencia");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.InicioExperiencia());

        Debug.Log("[Test_Asistente] Ejecutando IntroducciónAntesDeRuptura");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.IntroducciónAntesDeRuptura());

        Debug.Log("[Test_Asistente] Ejecutando RupturaModelo");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.RupturaModelo());

        Debug.Log("[Test_Asistente] Ejecutando InicioNarrativa");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.InicioNarrativa());

        Debug.Log("[Test_Asistente] Ejecutando CierreNarrativa");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.CierreNarrativa());

        Debug.Log("[Test_Asistente] Ejecutando Visor3DLibre");
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.Visor3DLibre());

        Debug.Log("[Test_Asistente] ===== TODAS LAS PRUEBAS COMPLETADAS =====");
    }
}
