using System.Collections;
using UnityEngine;

/// <summary>
/// Repositorio centralizado de secuencias del asistente y configuración de expresiones.
/// Define todas las secuencias disponibles y proporciona métodos para obtenerlas.
/// Se reinicia por escena (no usa DontDestroyOnLoad).
/// </summary>
public class ConfiguracionAsistente : MonoBehaviour
{
    public static ConfiguracionAsistente Instance { get; private set; }

    [SerializeField] AsistenteExpressionSpriteEntry[] expresionesSprites;

    [Header("Audios")]
    [SerializeField] AudioClip audioPrueba;

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    #region API Pública

    /// <summary>
    /// Obtiene el sprite correspondiente a una expresión emocional.
    /// </summary>
    /// <param name="expresion">Expresión solicitada.</param>
    /// <returns>Sprite de la expresión o null si no se encuentra.</returns>
    public Sprite GetExpresionSprite(ExpresionesAsistente expresion)
    {
        if (expresionesSprites == null || expresionesSprites.Length == 0)
        {
            Debug.LogError("[ConfiguracionAsistente] expresionesSprites no configurada.");
            return null;
        }

        foreach (var entry in expresionesSprites)
        {
            if (entry.expresion == expresion)
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning($"[ConfiguracionAsistente] No se encontró sprite para la expresión: {expresion}");
        return null;
    }

    /// <summary>
    /// Retorna la corrutina correspondiente a la secuencia solicitada.
    /// Cada secuencia ejecuta una serie de SetExpresion() y PlayDialog() en orden.
    /// </summary>
    /// <param name="nombreSecuencia">Nombre de la secuencia a ejecutar.</param>
    /// <returns>Corrutina de la secuencia.</returns>
    public IEnumerator GetSequence(SecuenciasDelSistema nombreSecuencia)
    {
        switch (nombreSecuencia)
        {
            case SecuenciasDelSistema.RupturaModelo:
                return RupturaModeloCoroutine();
            default:
                throw new System.NotImplementedException($"Secuencia '{nombreSecuencia}' no implementada.");
        }
    }

    #endregion

    #region Secuencias

    private IEnumerator RupturaModeloCoroutine()
    {
        var controlador = ControladorAsistente.Instance;

        if (controlador == null)
        {
            Debug.LogError("[ConfiguracionAsistente] ControladorAsistente no disponible.");
            yield break;
        }

        // Paso 1: Mostrar expresión asustada con fade-in y reproducir audio asustado
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        controlador.PlayDialog(audioPrueba, "LA PRIMERA SE DESESPERA");
        yield return new WaitForSeconds(audioPrueba.length + 1f);

        // Paso 2: Cambiar a expresión preocupada (cambio instantáneo) y reproducir siguiente audio
        controlador.SetExpresion(ExpresionesAsistente.Feliz);
        controlador.PlayDialog(audioPrueba, "LA SEGUNDA TIENE LA FUNDA");
        yield return new WaitForSeconds(audioPrueba.length/2);
        controlador.SetExpresion(ExpresionesAsistente.Preocupado);
        yield return new WaitForSeconds(audioPrueba.length/2 + 1f);

        // Paso 3: Cambiar a expresión feliz y reproducir último audio
        controlador.SetExpresion(ExpresionesAsistente.Feliz);
        controlador.PlayDialog(audioPrueba, "LA TERCERA ME QUITA EL ESTRÉS");
        yield return new WaitForSeconds(audioPrueba.length + 1f);
    }

    #endregion
}


#region Otros
/// <summary>
/// Enumeración de todas las secuencias disponibles del asistente.
/// Cada secuencia tiene una corrutina correspondiente en ConfiguracionAsistente.
/// </summary>
public enum SecuenciasDelSistema
{
    RupturaModelo,
}
/// <summary>
/// Estructura serializable para mapear expresiones a sprites en el inspector.
/// </summary>
[System.Serializable]
public class AsistenteExpressionSpriteEntry
{
    public ExpresionesAsistente expresion;
    public Sprite sprite;
}
/// <summary>
/// Define los estados emocionales del asistente virtual.
/// Cada expresión corresponde a un sprite diferente.
/// </summary>
public enum ExpresionesAsistente
{
    Neutral = 0,
    Asustado = 1,
    Preocupado = 2,
    Feliz = 3,
    Reflexivo = 4
}
#endregion