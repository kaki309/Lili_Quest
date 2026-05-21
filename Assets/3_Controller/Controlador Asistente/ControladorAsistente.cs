using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador del asistente virtual interactivo.
/// Gestiona reproducción de audio y subtítulos sincronizados con cambios de expresión.
/// Se reinicia por escena (no usa DontDestroyOnLoad).
/// </summary>
public class ControladorAsistente : MonoBehaviour
{
    public static ControladorAsistente Instance { get; private set; }
    public bool AsistenteActivo => estaReproduciendo;

    [SerializeField] Image imagenAsistente;
    [SerializeField] TMP_Text subtituloText;

    CanvasGroup canvasGroupImagen;
    bool estaReproduciendo = false;
    bool estaOculto = true;

    #region Lifecycle

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ValidarReferencias();
        InicializarElementos();
    }

    #endregion

    #region API Pública

    /// <summary>
    /// Establece la expresión emocional del asistente.
    /// Si no está reproduciendo audio, aplica fade-in de la imagen.
    /// Si ya está reproduciendo, cambia el sprite instantáneamente.
    /// </summary>
    public void SetExpresion(ExpresionesAsistente expresion)
    {
        if (estaOculto)
        {
            // Primera aparición de expresión
            estaOculto = false;
            StartCoroutine(mostrarExpresionProgresivo(expresion));
        }
        else
        {
            // Ya estaba activo, cambio instantáneo de sprite
            CambiarExpresion(expresion);
        }
    }
    /// <summary>
    /// Oculta la expresión del asistente con fade-out de la imagen.
    /// </summary>
    public IEnumerator HideExpresion()
    {
        estaOculto = true;
        yield return ocultarExpresionProgresivo();
    }

    /// <summary>
    /// Reproduce un diálogo con audio y subtítulo.
    /// </summary>
    public void PlayDialog(AudioClip audioClip, string texto = "")
    {
        if (audioClip == null)
        {
            Debug.LogError("[ControladorAsistente] AudioClip nulo en PlayDialog.");
            return;
        }

        StartCoroutine(PlayDialogCoroutine(audioClip, texto));
    }

    /// <summary>
    /// Ejecuta una secuencia predefinida del asistente.
    /// Solo permite una secuencia a la vez; otras solicitudes serán ignoradas.
    /// La secuencia automáticamente establece estaReproduciendo a true/false.
    /// </summary>
    /// <param name="secuencia">Método de la secuencia a ejecutar.</param>
    public IEnumerator PlaySequence(IEnumerator secuencia)
    {
        if (estaReproduciendo)
        {
            Debug.LogWarning("[ControladorAsistente] Ya hay una secuencia en reproducción.");
            yield break;
        }

        yield return ExecuteSequenceCoroutine(secuencia);
        yield return ClearAsistente();
    }

    /// <summary>
    /// Oculta el asistente y detiene cualquier audio en reproducción.
    /// Limpia la imagen y los subtítulos instantáneamente.
    /// </summary>
    public IEnumerator ClearAsistente()
    {
        AudioController.Instance.StopDialogue();
        subtituloText.text = "";
        subtituloText.gameObject.SetActive(false);
        yield return HideExpresion();
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Valida que todas las referencias necesarias estén asignadas.
    /// </summary>
    private void ValidarReferencias()
    {
        if (imagenAsistente == null)
        {
            Debug.LogError("[ControladorAsistente] imagenAsistente no asignada en el inspector.");
        }

        if (subtituloText == null)
        {
            Debug.LogError("[ControladorAsistente] subtituloText no asignada en el inspector.");
        }
    }

    /// <summary>
    /// Inicializa los CanvasGroup para manejar fade-in/out de imagen.
    /// </summary>
    private void InicializarElementos()
    {
        if (imagenAsistente != null)
        {
            canvasGroupImagen = imagenAsistente.GetComponent<CanvasGroup>();
            if (canvasGroupImagen == null)
            {
                canvasGroupImagen = imagenAsistente.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroupImagen.alpha = 0f;
        }

        if (subtituloText != null)
        {
            subtituloText.text="";
        }
    }

    // ======================================
    // Expresiones
    // =====================================

    /// <summary>
    /// Cambia el sprite del asistente según la expresión instantáneamente.
    /// Obtiene el sprite desde ConfiguracionAsistente.
    /// </summary>
    private void CambiarExpresion(ExpresionesAsistente expresion)
    {
        if (imagenAsistente == null)
        {
            return;
        }

        var configuracion = ConfiguracionAsistente.Instance;
        if (configuracion == null)
        {
            Debug.LogError("[ControladorAsistente] ConfiguracionAsistente no disponible.");
            return;
        }

        Sprite sprite = configuracion.GetExpresionSprite(expresion);
        if (sprite != null)
        {
            imagenAsistente.sprite = sprite;
        }
    }
    private IEnumerator mostrarExpresionProgresivo(ExpresionesAsistente expresion)
    {
        // Cambiar sprite mientras está oculto
        CambiarExpresion(expresion);

        // Mostrar imagen con fade-in
        yield return FadeImagenCoroutine(true, ConfiguracionAsistente.Instance.DuracionFadeImagen);
    }
    private IEnumerator ocultarExpresionProgresivo()
    {
        yield return FadeImagenCoroutine(false, ConfiguracionAsistente.Instance.DuracionFadeImagen);
    }
    private IEnumerator FadeImagenCoroutine(bool mostrar, float duracion)
    {
        if (canvasGroupImagen == null)
        {
            yield break;
        }

        float targetAlpha = mostrar ? 1f : 0f;
        float tiempoTranscurrido = 0f;
        float alphaInicial = canvasGroupImagen.alpha;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracion;
            canvasGroupImagen.alpha = Mathf.Lerp(alphaInicial, targetAlpha, progreso);
            yield return null;
        }

        canvasGroupImagen.alpha = targetAlpha;
    }

    // ======================================
    // Diálogos
    // =====================================

    /// <summary>
    /// Corrutina para reproducir un diálogo con audio y subtítulo.
    /// </summary>
    private IEnumerator PlayDialogCoroutine(AudioClip audioClip, string texto)
    {
        // 1. Mostrar subtítulo
        subtituloText.text = texto;
        subtituloText.gameObject.SetActive(true);

        // 2. Reproducir audio
        AudioController.Instance.PlayDialogue(audioClip);

        // 3. Esperar a que termine el audio y otro poquito
        yield return new WaitForSeconds(audioClip.length + ConfiguracionAsistente.Instance.EsperaDespuesDeAudio);

        // 4. Limpiar subtítulo instantáneamente
        subtituloText.gameObject.SetActive(false);
        subtituloText.text = "";
    }

    // ======================================
    // Secuencias
    // =====================================

    /// <summary>
    /// Corrutina que ejecuta una secuencia obtenida desde ConfiguracionAsistente.Instance.Secuencias.
    /// Automáticamente gestiona el estado estaReproduciendo al inicio y al final.
    /// </summary>
    private IEnumerator ExecuteSequenceCoroutine(IEnumerator secuencia)
    {
        estaReproduciendo = true;

        var configuracion = ConfiguracionAsistente.Instance;
        if (configuracion == null)
        {
            Debug.LogError("[ControladorAsistente] ConfiguracionAsistente no disponible.");
            estaReproduciendo = false;
            yield break;
        }

        // Ejecuta la secuencia
        // yield return hace que este código se pause hasta que la secuencia lanzada acabe completamente
        yield return secuencia;

        // Termina la reproducción y restablece el estado
        estaReproduciendo = false;
    }

    #endregion
}

