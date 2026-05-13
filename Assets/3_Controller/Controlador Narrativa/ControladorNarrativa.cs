using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ControladorNarrativa : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] AudioClip respuestaCorrecta;
    [SerializeField] AudioClip respuestaIncorrecta;

    EntradaAudioClipSprite[] laiaFelicitaciones;
    EntradaAudioClipSprite[] laiaIntentaNuevamente;
    GestorInterfazPantallaNarrativa UI;
    bool isAnsweringTrivia = false;
    AudioClip currentAudio;
    ControladorAsistente asistente;
    AudioController controladorAudio;
    ParsedExperienceData currentExperienceData;

    void Start()
    {
        asistente = ControladorAsistente.Instance;
        controladorAudio = AudioController.Instance;
        laiaFelicitaciones = ConfiguracionAsistente.Instance.feedbackCorrectoTrivia;
        laiaIntentaNuevamente = ConfiguracionAsistente.Instance.feedbackIncorrectoTrivia;
        UI = GestorInterfazPantallaNarrativa.Instance;
        currentExperienceData = ControladorFlujo.Instance.GetCurrentExperienceData();

        HideUIElements();
        StartCoroutine(NarrativaPerrito());
    }
    void HideUIElements()
    {
        UI.CanvasTrivia.SetActive(false);
        UI.EncuadreFoto.SetActive(false);
        UI.ReferenciaInfo.text = "";
    }
    IEnumerator SimularSecuencia()
    {
        yield return new WaitForSeconds(3);

        UI.EncuadreFoto.SetActive(true);

        yield return new WaitForSeconds(2);

        UI.EncuadreFoto.SetActive(false);

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
    IEnumerator NarrativaPerrito()
    {
        asistente.SetExpresion(ExpresionesAsistente.idle1);
        currentAudio = controladorAudio.PlaySFX(currentExperienceData.audios["intro_silbato"]);
        UI.Subtitulo.text = "reproduciendo intro_silbato";

        yield return new WaitForSeconds(currentAudio.length + 2f);

        asistente.SetExpresion(ExpresionesAsistente.deHecho1);
        currentAudio = controladorAudio.PlaySFX(currentExperienceData.audios["contexto_quimbaya"]);
        UI.Subtitulo.text = "reproduciendo contexto_quimbaya";
        UI.EncuadreFoto.SetActive(true);
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["ceramica"]);

        yield return new WaitForSeconds(currentAudio.length + 2f);
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

    /// <summary>
    /// Devuelve un Sprite cargado desde una ruta completa del sistema
    /// </summary>
    private Sprite LoadSpriteFromPath(string imagePath)
    {
        byte[] imageData = System.IO.File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageData);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }

    EntradaAudioClipSprite getRandomLaiaFeedback(EntradaAudioClipSprite[] entrada)
    {
        int index = Random.Range(0, entrada.Length);
        return entrada[index];
    }
}
