using System.Collections;
using System.Linq;
using UnityEngine;

public class ControladorNarrativa : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] AudioClip respuestaCorrecta;
    [SerializeField] AudioClip respuestaIncorrecta;

    EntradaAudioClipSprite[] laiaFelicitaciones;
    EntradaAudioClipSprite[] laiaIntentaNuevamente;
    GestorInterfazPantallaNarrativa UI;
    bool isAnsweringTrivia = false;

    void Start()
    {
        laiaFelicitaciones = ConfiguracionAsistente.Instance.feedbackCorrectoTrivia;
        laiaIntentaNuevamente = ConfiguracionAsistente.Instance.feedbackIncorrectoTrivia;
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
        asistente.PlayDialog(laiaFelicitaciones[0].audioClip, "Primer texto de la narrativa");
        yield return new WaitForSeconds(laiaFelicitaciones[0].audioClip.length);
        asistente.HideExpresion();

        yield return new WaitForSeconds(3);

        UI.CanvasTrivia.SetActive(true);

        isAnsweringTrivia = true;
        // Esta variable se cancela desde los botones de la trivia
        while (isAnsweringTrivia) yield return null;

        UI.CanvasTrivia.SetActive(false);

        finishSecuence();
    }
    public void answerTriviaCorrectly() => StartCoroutine(answerCorrect());
    public void answerTriviaIncorrectly() => StartCoroutine(answerIncorrect());
    IEnumerator answerCorrect()
    {
        EntradaAudioClipSprite laiaFeedback = getRandomLaiaFeedback(laiaFelicitaciones);
        AudioClip audio = laiaFeedback.audioClip;
        Sprite image = laiaFeedback.sprite;

        AudioController.Instance.PlaySFX(respuestaCorrecta);
        // Sprite LaIA
        UI.LaIaInTrivia.sprite = image;
        UI.LaIaInTrivia.GetComponent<Animator>().SetTrigger("moveIn");

        AudioController.Instance.PlayDialogue(audio);

        yield return new WaitForSeconds(audio.length + 1f);

        UI.LaIaInTrivia.GetComponent<Animator>().SetTrigger("moveOut");
        Invoke(nameof(setTriviaAsCompleted), 2);
    }
    IEnumerator answerIncorrect()
    {
        EntradaAudioClipSprite laiaFeedback = getRandomLaiaFeedback(laiaIntentaNuevamente);
        AudioClip audio = laiaFeedback.audioClip;
        Sprite image = laiaFeedback.sprite;

        AudioController.Instance.PlaySFX(respuestaIncorrecta);
        // Sprite LaIA
        UI.LaIaInTrivia.sprite = image;
        UI.LaIaInTrivia.GetComponent<Animator>().SetTrigger("moveIn");

        AudioController.Instance.PlayDialogue(audio);

        yield return new WaitForSeconds(audio.length + 1f);

        UI.LaIaInTrivia.GetComponent<Animator>().SetTrigger("moveOut");
    }
    void setTriviaAsCompleted()
    {
        isAnsweringTrivia = false;
    }
    void finishSecuence()
    {
        ControladorFlujo.Instance.FinishNarrativaState();
    }
    EntradaAudioClipSprite getRandomLaiaFeedback(EntradaAudioClipSprite[] entrada)
    {
        int index = Random.Range(0, entrada.Length);
        return entrada[index];
    }
}
