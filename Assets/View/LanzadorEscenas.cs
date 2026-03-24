using UnityEngine;
using UnityEngine.SceneManagement;

public enum EscenasSistema
{
    Inicio = 0,
    InteraccionConRuptura = 1,
    Narrativa = 2,
    Visor3D = 3
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

    public void cargarEscena(int index)
    {
        SceneManager.LoadScene(index);
    }
}
