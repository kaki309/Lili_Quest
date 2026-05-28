using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] Slider loadingSlider;
    int sceneToLoad;
    Action<Action> optionalCallback = null;

    public void SetSceneAndStartLoad(int number, Action<Action> callback = null)
    {
        sceneToLoad = number;
        optionalCallback = callback;
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Descargar todas las escenas activas excepto la de carga
        // Esto previene acumulación de escenas
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            // No descargar la escena de carga (esta es la pantalla de carga actual)
            if (scene.buildIndex != (int)EscenasSistema.PantallaCarga && scene.isLoaded)
            {
                Debug.Log($"[LoadingSceneController] Descargando escena: {scene.name}");
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        // Pequeña espera para que las escenas se descarguen
        yield return null;

        // Cargar la nueva escena en modo Additive
        AsyncOperation asyncLoad = null;
        asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        asyncLoad.allowSceneActivation = false;

        // Pausar música
        AudioController.Instance.PauseMusic();

        // Esperamos que la escena cargue hasta su 90%
        // o que el slider no haya llegado a X valor aleatorio (Dinamismo)
        float loadProgressBeforeExecution = UnityEngine.Random.Range(0.35f, 0.65f);
        while (asyncLoad.progress < 0.9f || loadingSlider.value < loadProgressBeforeExecution)
        {
            float velocidad = 0f;
            loadingSlider.value = Mathf.SmoothDamp(
                loadingSlider.value,
                loadProgressBeforeExecution + 0.1f,
                ref velocidad,
                0.125f
            );
            yield return null;
        }

        // Activamos la escena pero aún no descargamos la pantalla de carga
        asyncLoad.allowSceneActivation = true;
        // Esperamos un frame para que Unity inicialice la escena
        yield return null;

        if (optionalCallback != null)
        {
            bool callbackCompleted = false;
            // Le pasamos al callback una Action que debe invocar cuando termine
            optionalCallback.Invoke(() => callbackCompleted = true);

            while (!callbackCompleted) yield return null;
        }

        // Ponemos el slider rápidamente en 100%
        while (loadingSlider.value < 0.95f)
        {
            loadingSlider.value += Time.deltaTime / 2;
            yield return null;
        }
        loadingSlider.value = 1;

        // Reanudamos música
        AudioController.Instance.ResumeMusic();

        // Descargamos la pantalla de carga
        SceneManager.UnloadSceneAsync((int)EscenasSistema.PantallaCarga);
    }
}