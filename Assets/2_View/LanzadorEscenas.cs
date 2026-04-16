using UnityEngine;
using UnityEngine.SceneManagement;

public enum EscenasSistema
{
    Inicio = 0,
    Visor3D = 1,
    Narrativa = 2,
}

public class LanzadorEscenas : MonoBehaviour
{
    public static LanzadorEscenas Instance { get; private set; }


    void Awake()
    {
        // Implementar Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void cargarEscena(EscenasSistema escena)
    {
        SceneManager.LoadScene((int)escena);
    }
}
