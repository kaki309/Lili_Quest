using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] Slider loadingSlider;
    [SerializeField] float maxProgressWhenCallback = 85f;
    private int sceneToLoad;

    public void SetSceneAndStartLoad(int number, Action<Action> callback = null)
    {
        sceneToLoad = number;
        StartCoroutine(LoadSceneAsync(callback));
    }

    IEnumerator LoadSceneAsync(Action<Action> callback = null)
    {
        AsyncOperation asyncLoad = null;

        if (callback == null)
        { asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad); }
        else
        { asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive); }

        asyncLoad.allowSceneActivation = false;

        // Actualizamos el slider de manera lenta
        while (asyncLoad.progress < 0.9f)
        {
            loadingSlider.value += Time.deltaTime*2;
            yield return null;
        }
        // Si hay callback y el progreso de la escena no superó el maxProgress, ponemos tope
        if (callback != null)
        {
            float target = maxProgressWhenCallback / 100f; // 90 → 0.9
            while (loadingSlider.value < target - 0.001f)
            {
                loadingSlider.value = Mathf.MoveTowards(loadingSlider.value, target, Time.deltaTime * 0.1f);
                yield return null;
            }
            loadingSlider.value = target;
        }

        // Activamos la escena pero aún no descargamos la pantalla de carga
        asyncLoad.allowSceneActivation = true;
        // Esperamos un frame para que Unity inicialice la escena
        yield return null;
        // Si hay callback, esperamos a que señalice que terminó
        bool callbackCompleted = false;
        // Le pasamos al callback una Action que debe invocar cuando termine
        callback?.Invoke(() => callbackCompleted = true);
        // Esperar hasta que el callback termine de ejecutar
        while (!callbackCompleted)
        {
            yield return null;
        }
        // Ponemos el slider rápidamente en 100%
        yield return null;
        loadingSlider.value = 1f;
        // Descargamos la pantalla de carga
        SceneManager.UnloadSceneAsync((int)EscenasSistema.PantallaCarga);
        yield break;
    }
}