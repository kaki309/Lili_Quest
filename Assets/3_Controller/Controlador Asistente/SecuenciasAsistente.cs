using System.Collections;
using UnityEngine;

public class SecuenciasAsistente : MonoBehaviour
{
    [Header("Introducción antes de ruptura")]
    [SerializeField] AudioClip[] audiosIntroducciónAntesDeRuptura;
    [Header("Ruptura")]
    [SerializeField] AudioClip[] audiosRuptura;
    [Header("Visor 3D libre")]
    [SerializeField] AudioClip[] audiosVisor3DLibre;

    ControladorAsistente controlador;
    AudioClip audioActual;
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

    public IEnumerator IntroducciónAntesDeRuptura()
    {
        if (!getController()) yield break;

        // Saludo
        audioActual = audiosIntroducciónAntesDeRuptura[0];
        controlador.SetExpresion(ExpresionesAsistente.saludo);
        controlador.PlayDialog(audioActual, "Acabas de activar una pieza muy especial.\nSoy Laia, y te acompañaré.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        // Introducción a la pieza
        audioActual = audiosIntroducciónAntesDeRuptura[1];
        controlador.SetExpresion(ExpresionesAsistente.idle2);
        controlador.PlayDialog(audioActual, "Este es un silbato en forma de perro.\nPuedes explorarlo… gíralo y acércate para ver sus detalles.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        // Advertencia de manipulación
        audioActual = audiosIntroducciónAntesDeRuptura[2];
        controlador.SetExpresion(ExpresionesAsistente.deHecho1);
        controlador.PlayDialog(audioActual, "Pero… hazlo con cuidado.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        yield return controlador.ClearAsistente();
    }
    public IEnumerator RupturaModelo()
    {
        if (!getController()) yield break;

        // Susto por ruptura
        audioActual = audiosRuptura[0];
        controlador.SetExpresion(ExpresionesAsistente.sorpresa);
        controlador.PlayDialog(audioActual, "Oh… se ha roto.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        // Explicación de fragilidad
        audioActual = audiosRuptura[1];
        controlador.SetExpresion(ExpresionesAsistente.explicando1);
        controlador.PlayDialog(audioActual, "Las piezas reales son frágiles… y únicas.\nPor eso solo pueden ser manipuladas por expertos.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        // Propuesta de reparación
        audioActual = audiosRuptura[2];
        controlador.SetExpresion(ExpresionesAsistente.amable2);
        controlador.PlayDialog(audioActual, "Pero aún podemos recuperarla.\nTe ayudaré a reconstruirla mientras descubrimos su historia.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        // Acompañamiento
        audioActual = audiosRuptura[3];
        controlador.SetExpresion(ExpresionesAsistente.idle2);
        controlador.PlayDialog(audioActual, "Te acompañaré a descubrir la historia de esta pieza.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        yield return controlador.ClearAsistente();
    }
    public IEnumerator Visor3DLibre()
    {
        if (!getController()) yield break;

        audioActual = audiosVisor3DLibre[0];
        controlador.SetExpresion(ExpresionesAsistente.explicando2);
        controlador.PlayDialog(audioActual, "¡Genial! Has reconstruido la pieza y comprendido su historia cultural.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        audioActual = audiosVisor3DLibre[1];
        controlador.SetExpresion(ExpresionesAsistente.idle2);
        controlador.PlayDialog(audioActual, "Ahora puedes observarla libremente… ya no se romperá.");
        yield return new WaitForSeconds(audioActual.length + tiempoEspera);

        yield return controlador.ClearAsistente();
    }
}