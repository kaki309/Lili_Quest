using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ControladorNarrativa : MonoBehaviour
{
    GestorInterfazPantallaNarrativa UI;
    bool isPlayingSecuence = false;
    bool hasStartedExit = false;
    bool isAnsweringTrivia = false;
    [Header("Sonidos del sistema")]
    [SerializeField] AudioClip respuestaCorrecta;
    [SerializeField] AudioClip respuestaIncorrecta;
    [SerializeField] AudioClip laiaFelicitacion;

    void Start()
    {
        UI = GestorInterfazPantallaNarrativa.Instance;
        TurnOffEveryUIElement();
        StartCoroutine(simularSecuencua());
    }
    void TurnOffEveryUIElement()
    {
        UI.Foto.SetActive(false);
        UI.Subtitulo.gameObject.SetActive(false);
        UI.CanvasTrivia.SetActive(false);
        UI.LaiaNarrator.gameObject.SetActive(false);
    }
    void Update()
    {
        if (isPlayingSecuence || hasStartedExit) return;
        finishSecuence();
    }
    IEnumerator simularSecuencua()
    {
        isPlayingSecuence = true;

        yield return new WaitForSeconds(3);

        UI.Foto.SetActive(true);

        yield return new WaitForSeconds(2);

        UI.Foto.SetActive(false);

        UI.LaiaNarrator.gameObject.SetActive(true);
        UI.Subtitulo.gameObject.SetActive(true);

        yield return new WaitForSeconds(4);

        UI.Subtitulo.gameObject.SetActive(false);
        UI.LaiaNarrator.gameObject.SetActive(false);
        UI.CanvasTrivia.SetActive(true);

        isAnsweringTrivia = true;
        while (isAnsweringTrivia) yield return null;

        UI.CanvasTrivia.SetActive(false);

        isPlayingSecuence = false;
        yield break;
    }
    void finishSecuence()
    {
        hasStartedExit = true;
        ControladorFlujo.Instance.finishNarrativaState();
    }
    public void answerTriviaCorrectly()
    {
        UI.AudioSource.PlayOneShot(respuestaCorrecta);
        UI.LaIaHappyInTrivia.gameObject.SetActive(true);
        UI.LaIaHappyInTrivia.GetComponent<Animator>().SetTrigger("moveIn");
        Invoke(nameof(setTriviaAsCompleted), 4);
        UI.AudioSource.PlayOneShot(laiaFelicitacion);
    }
    public void answerTriviaIncorrectly()
    {
        UI.AudioSource.clip = respuestaIncorrecta;
        UI.AudioSource.Play();
    }
    void setTriviaAsCompleted()
    {
        isAnsweringTrivia = false;
    }
}
