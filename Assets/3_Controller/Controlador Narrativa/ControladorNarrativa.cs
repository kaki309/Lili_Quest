using System.Collections;
using UnityEngine;

public class ControladorNarrativa : MonoBehaviour
{
    GestorInterfazPantallaNarrativa UI;
    bool isAnsweringTrivia = false;
    [Header("Sonidos del sistema")]
    [SerializeField] AudioClip respuestaCorrecta;
    [SerializeField] AudioClip respuestaIncorrecta;
    [SerializeField] AudioClip laiaFelicitacion;

    void Start()
    {
        UI = GestorInterfazPantallaNarrativa.Instance;
        TurnOffEveryUIElement();
        StartCoroutine(SimularSecuencia());
    }
    void TurnOffEveryUIElement()
    {
        UI.Foto.SetActive(false);
        UI.CanvasTrivia.SetActive(false);
    }
    IEnumerator SimularSecuencia()
    {   
        ControladorAsistente asistente = ControladorAsistente.Instance;

        yield return new WaitForSeconds(3);

        UI.Foto.SetActive(true);

        yield return new WaitForSeconds(2);

        UI.Foto.SetActive(false);

        asistente.SetExpresion(ExpresionesAsistente.idle1);
        asistente.PlayDialog(laiaFelicitacion, "Primer texto de la narrativa");
        yield return new WaitForSeconds(laiaFelicitacion.length);
        asistente.HideExpresion();

        yield return new WaitForSeconds(3);

        UI.CanvasTrivia.SetActive(true);

        isAnsweringTrivia = true;
        // Esta variable se cancela desde los botones de la trivia
        while (isAnsweringTrivia) yield return null;

        UI.CanvasTrivia.SetActive(false);

        finishSecuence();
    }
    void finishSecuence()
    {
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
