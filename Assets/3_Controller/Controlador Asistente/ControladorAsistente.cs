using System;
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
    [SerializeField] AudioSource audioSourceAsistente;
    [SerializeField] float duracionFadeImagen = 0.5f;
    [SerializeField] float esperaDespuesDeAudio = 0.3f;

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
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region API Pública

    /// <summary>
    /// Establece la expresión emocional del asistente.
    /// Si no está reproduciendo audio, aplica fade-in de la imagen.
    /// Si ya está reproduciendo, cambia el sprite instantáneamente.
    /// </summary>
    /// <param name="expresion">Expresión emocional a mostrar.</param>
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
            // Ya reproduciendo audio: cambio instantáneo de sprite
            CambiarExpresionInstantaneo(expresion);
        }
    }
    /// <summary>
    /// Oculta la expresión del asistente con fade-out de la imagen.
    /// </summary>
    public void HideExpresion()
    {
        estaOculto = true;
        StartCoroutine(ocultarExpresionProgresivo());
    }

    /// <summary>
    /// Reproduce un diálogo con audio y subtítulo sincronizados.
    /// Solo maneja audio y subtítulo, no la visibilidad de la imagen.
    /// La imagen debe ser mostrada previamente con SetExpresion().
    /// </summary>
    /// <param name="audioClip">Clip de audio a reproducir.</param>
    /// <param name="texto">Texto del subtítulo a mostrar durante el audio.</param>
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
    /// <param name="nombreSecuencia">Nombre de la secuencia a ejecutar.</param>
    public IEnumerator ExecuteSequence(SecuenciasDelSistema nombreSecuencia)
    {
        if (estaReproduciendo)
        {
            Debug.LogWarning("[ControladorAsistente] Ya hay una secuencia en reproducción.");
            yield break;
        }

        yield return StartCoroutine(ExecuteSequenceCoroutine(nombreSecuencia));
    }

    /// <summary>
    /// Oculta el asistente y detiene cualquier audio en reproducción.
    /// Limpia la imagen y los subtítulos instantáneamente.
    /// </summary>
    public void ClearAsistente()
    {
        if (audioSourceAsistente != null && audioSourceAsistente.isPlaying)
        {
            audioSourceAsistente.Stop();
        }
        if (canvasGroupImagen != null)
        {
            HideExpresion();
        }
        if (subtituloText != null)
        {
            subtituloText.text = "";
            subtituloText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Métodos Privados

    // -------------------------- Validación

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

        if (audioSourceAsistente == null)
        {
            // Intenta obtener AudioSource del mismo GameObject
            audioSourceAsistente = GetComponent<AudioSource>();
            if (audioSourceAsistente == null)
            {
                Debug.LogWarning("[ControladorAsistente] audioSource no encontrado. Se creará uno nuevo.");
                audioSourceAsistente = gameObject.AddComponent<AudioSource>();
            }
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
            subtituloText.gameObject.SetActive(false);
        }
    }

    // ======================================
    // Expresiones
    // =====================================

    /// <summary>
    /// Cambia el sprite del asistente según la expresión instantáneamente.
    /// Obtiene el sprite desde ConfiguracionAsistente.
    /// </summary>
    private void CambiarExpresionInstantaneo(ExpresionesAsistente expresion)
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
    /// <summary>
    /// Corrutina para cambiar expresión con fade-in de la imagen.
    /// Se ejecuta solo si no está reproduciendo (primera aparición).
    /// </summary>
    private IEnumerator mostrarExpresionProgresivo(ExpresionesAsistente expresion)
    {
        // Cambiar sprite mientras está oculto
        CambiarExpresionInstantaneo(expresion);

        // Mostrar imagen con fade-in
        yield return StartCoroutine(FadeImagenCoroutine(true, duracionFadeImagen));
    }
    /// <summary>
    /// Corrutina para ocultar la expresión con fade-out de la imagen.
    /// </summary>
    private IEnumerator ocultarExpresionProgresivo()
    {
        yield return StartCoroutine(FadeImagenCoroutine(false, duracionFadeImagen));
    }
    /// <summary>
    /// Realiza fade-in/out de la imagen.
    /// </summary>
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
    /// Solo maneja audio y subtítulo, sin controlar la visibilidad de la imagen.
    /// </summary>
    private IEnumerator PlayDialogCoroutine(AudioClip audioClip, string texto)
    {
        // 1. Mostrar subtítulo sin animación
        MostrarSubtitulo(texto);

        // 2. Reproducir audio
        audioSourceAsistente.clip = audioClip;
        audioSourceAsistente.Play();

        // 3. Esperar a que termine el audio y otro poquito
        yield return new WaitForSeconds(audioClip.length + esperaDespuesDeAudio);

        // 4. Limpiar subtítulo instantáneamente
        OcultarSubtitulo();
    }
    /// <summary>
    /// Muestra el subtítulo instantáneamente (sin animación).
    /// </summary>
    private void MostrarSubtitulo(string texto)
    {
        if (subtituloText == null)
        {
            return;
        }

        subtituloText.text = texto;
        subtituloText.gameObject.SetActive(true);
    }
    /// <summary>
    /// Oculta el subtítulo instantáneamente (sin animación).
    /// </summary>
    private void OcultarSubtitulo()
    {
        if (subtituloText == null)
        {
            return;
        }

        subtituloText.text = "";
        subtituloText.gameObject.SetActive(false);
    }

    // ======================================
    // Secuencias
    // =====================================

    /// <summary>
    /// Corrutina que ejecuta una secuencia obtenida desde ConfiguracionAsistente.
    /// Automáticamente gestiona el estado estaReproduciendo al inicio y al final.
    /// </summary>
    private IEnumerator ExecuteSequenceCoroutine(SecuenciasDelSistema nombreSecuencia)
    {
        estaReproduciendo = true;

        var configuracion = ConfiguracionAsistente.Instance;
        if (configuracion == null)
        {
            Debug.LogError("[ControladorAsistente] ConfiguracionAsistente no disponible.");
            estaReproduciendo = false;
            yield break;
        }

        // Obtiene y ejecuta la secuencia
        // yield return hace que este código se pause hasta que la secuencia lanzada acabe completamente
        var secuencia = configuracion.GetSequence(nombreSecuencia);
        yield return StartCoroutine(secuencia);

        // Termina la reproducción y restablece el estado
        estaReproduciendo = false;
    }

    #endregion
}

