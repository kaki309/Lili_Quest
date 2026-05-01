using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EscenasSistema
{
    Inicio = 0,
    Visor3D = 1,
    Narrativa = 2,
    PantallaCarga = 3
}

public class LanzadorEscenas : MonoBehaviour
{
    public static LanzadorEscenas Instance { get; private set; }

    [SerializeField] private int loadingSceneIndex = (int)EscenasSistema.PantallaCarga;

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
        StartCoroutine(LoadLoadingScene((int)escena));
    }
    public void cargarEscenaYEjecutar(EscenasSistema escena, Action<Action> callback)
    {
        StartCoroutine(LoadLoadingScene((int)escena, callback));
    }

    // Coroutine
    IEnumerator LoadLoadingScene(int sceneNumber, Action<Action> callback = null)
    {
        // Load the loading scene
        SceneManager.LoadScene(loadingSceneIndex);

        // Find the loading controller
        LoadingSceneController loader = null;
        while (loader == null)
        {
            loader = FindObjectOfType<LoadingSceneController>();
            yield return null;
        }
        
        loader.SetSceneAndStartLoad(sceneNumber, callback);
        yield break;
    }
}
