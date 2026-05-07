using UnityEngine;

[RequireComponent(typeof(SecuenciasAsistente))]
public class ConfiguracionAsistente : MonoBehaviour
{
    public static ConfiguracionAsistente Instance { get; private set; }

    [SerializeField] float _duracionFadeImagen = 0.5f;
    [SerializeField] float _esperaDespuesDeAudio = 0.3f;
    [SerializeField] EntradaExpresionSprite[] spritesExpresiones;
    public AudioClip[] feedbackCorrectoTrivia;
    public AudioClip[] feedbackIncorrectoTrivia;
    

    public SecuenciasAsistente Secuencias {get; private set;}
    public float DuracionFadeImagen => _duracionFadeImagen;
    public float EsperaDespuesDeAudio => _esperaDespuesDeAudio;

    void Awake()
    {
        // Implementar Singleton
        if (Instance != null && Instance != this)
        {
            Debug.Log("[ConfiguraciónAsistente] Instancia ya existe. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ConfiguraciónAsistente] Singleton inicializado.");
    }
    void Start()
    {
        Secuencias = GetComponent<SecuenciasAsistente>();
        if (!Secuencias)
        {
            Debug.LogWarning("[Configuración Asistente] No hay componente de secuencias asignado");
        }
    }

    /// <summary>
    /// Obtiene el sprite correspondiente a una expresión emocional.
    /// </summary>
    public Sprite GetExpresionSprite(ExpresionesAsistente expresion)
    {
        if (spritesExpresiones == null || spritesExpresiones.Length == 0)
        {
            Debug.LogError("[ConfiguracionAsistente] expresionesSprites no configurada.");
            return null;
        }

        foreach (var entry in spritesExpresiones)
        {
            if (entry.expresion == expresion)
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning($"[ConfiguracionAsistente] No se encontró sprite para la expresión: {expresion}");
        return null;
    }
}

#region Helpers
/// <summary>
/// Estructura serializable para mapear expresiones a sprites en el inspector.
/// </summary>
[System.Serializable]
public class EntradaExpresionSprite
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
    Neutral,
    Asustado,
    Preocupado,
    Feliz,
    Reflexivo
}
#endregion