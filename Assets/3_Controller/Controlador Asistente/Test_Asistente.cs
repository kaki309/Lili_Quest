using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Script de prueba para el ControladorAsistente.
/// Simula el uso de la API pública y permite verificar el funcionamiento correcto.
/// </summary>
public class Test_Asistente : MonoBehaviour
{
    [SerializeField] AudioClip audioPrueba;
    [SerializeField] TMP_Text titulo;

    private void Start()
    {
        if (ControladorAsistente.Instance == null)
        {
            Debug.LogError("[Test_Asistente] ControladorAsistente no encontrado en la escena.");
            return;
        }

        // Inicia las pruebas
        Debug.Log("[Test_Asistente] Iniciando pruebas del ControladorAsistente...");
        //StartCoroutine(EjecutarPruebas());
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
        asistente.PlayDialog(audioPrueba, "Este es el primer diálogo de prueba.");
        yield return new WaitForSeconds(audioPrueba.length + 3f);
        asistente.HideExpresion();
        yield return new WaitForSeconds(4f);

        // ============ PRUEBA 3: Cambiar expresión durante reproducción ============
        Debug.Log("[Test_Asistente] PRUEBA 3: Establecer expresión y cambiarla");
        asistente.SetExpresion(ExpresionesAsistente.sorpresa);
        yield return new WaitForSeconds(3f);
        asistente.SetExpresion(ExpresionesAsistente.deHecho1);
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
        yield return new WaitForSeconds(2.5f);

        ControladorAsistente asistente = ControladorAsistente.Instance;

        titulo.text = "Ejecutando IntroducciónAntesDeRuptura";
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.IntroducciónAntesDeRuptura());

        titulo.text = "Ejecutando RupturaModelo";
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.RupturaModelo());

        titulo.text = "Ejecutando Visor3DLibre";
        yield return asistente.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.Visor3DLibre());

        Debug.Log("[Test_Asistente] ===== TODAS LAS PRUEBAS COMPLETADAS =====");
    }
}
