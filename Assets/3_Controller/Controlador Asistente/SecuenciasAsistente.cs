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
        AudioClip audio;

        // Saludo
        audio = audiosIntroducciónAntesDeRuptura[0];
        controlador.SetExpresion(ExpresionesAsistente.saludo);
        controlador.PlayDialog(audio, "Acabas de activar una pieza muy especial.\nSoy Laia, y te acompañare.  ");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Introducción a la pieza
        audio = audiosIntroducciónAntesDeRuptura[1];
        controlador.SetExpresion(ExpresionesAsistente.idle2);
        controlador.PlayDialog(audio, "Este es un silbato en forma de perro.\nPuedes explorarlo… gíralo y acércate para ver sus detalles.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Advertencia de manipulación
        audio = audiosIntroducciónAntesDeRuptura[2];
        controlador.SetExpresion(ExpresionesAsistente.deHecho1);
        controlador.PlayDialog(audio, "Pero… hazlo con cuidado.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);
    }
    public IEnumerator RupturaModelo()
    {
        if (!getController()) yield break;

        AudioClip audio;

        // Susto por ruptura
        audio = audiosRuptura[0];
        controlador.SetExpresion(ExpresionesAsistente.sorpresa);
        controlador.PlayDialog(audio, "Oh… se ha roto.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Explicación de fragilidad
        audio = audiosRuptura[1];
        controlador.SetExpresion(ExpresionesAsistente.explicando1);
        controlador.PlayDialog(audio, "Las piezas reales son frágiles… y únicas.\nPor eso solo pueden ser manipuladas por expertos.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Propuesta de reparación
        audio = audiosRuptura[2];
        controlador.SetExpresion(ExpresionesAsistente.amable2);
        controlador.PlayDialog(audio, "Pero aún podemos recuperarla.\nTe ayudaré a reconstruirla mientras descubrimos su historia.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        // Acompañamiento
        audio = audiosRuptura[3];
        controlador.SetExpresion(ExpresionesAsistente.idle2);
        controlador.PlayDialog(audio, "Te acompañaré a descubrir la historia de esta pieza.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);
    }
    public IEnumerator Visor3DLibre()
    {
        if (!getController()) yield break;
        
        AudioClip audio;

        audio = audiosVisor3DLibre[0];
        controlador.SetExpresion(ExpresionesAsistente.explicando2);
        controlador.PlayDialog(audio, "¡Genial! Has reconstruido la pieza y comprendido su historia cultural.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);

        audio = audiosVisor3DLibre[0];
        controlador.SetExpresion(ExpresionesAsistente.idle2);
        controlador.PlayDialog(audio, "Ahora puedes observarla libremente… ya no se romperá.");
        yield return new WaitForSeconds(audio.length + tiempoEspera);
    }
}