using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorNarrativa : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] AudioClip respuestaCorrectaSFX;
    [SerializeField] AudioClip respuestaIncorrectaSFX;

    EntradaAudioClipSprite[] laiaFelicitaciones;
    EntradaAudioClipSprite[] laiaIntentaNuevamente;
    GestorInterfazPantallaNarrativa UI;
    bool isAnsweringTrivia = false;
    AudioClip currentAudio;
    ControladorAsistente asistente;
    AudioController controladorAudio;
    ParsedExperienceData currentExperienceData;
    string respuestaCorrectaTrivia;
    List<string> respuestasIncorrectasTrivia = new List<string>();
    bool isProcessingCorrectAnswer = false;
    Coroutine incorrectAnswerCoroutine;

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
        yield return new WaitForSeconds(2f);

        UI.EncuadreFoto.SetActive(true);
        UI.ReferenciaInfo.text = "MediaTech (2026)";

        // ###################################################################### ############################################################# Bloque 1

        // --------------- Primera parte
        asistente.SetExpresion(ExpresionesAsistente.idle1);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_1-audio_1"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_1-imagen_1"]);
        UI.Subtitulo.text = "Contempla esta pequeña figura... no es solo barro moldeado, es un eco que ha viajado por siglos, hasta llegar frente a tus ojos.";
        yield return new WaitForSeconds(currentAudio.length);

        // --------------- Segunda parte
        asistente.SetExpresion(ExpresionesAsistente.explicando1);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_1-audio_2"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_1-imagen_2"]);
        UI.Subtitulo.text = "Este silbato en forma de perro pertenece a la cultura Quimbaya, antiguos maestros de la tierra que habitaron el corazón de Colombia.";
        yield return new WaitForSeconds(currentAudio.length);

        // --------------- Tercera parte
        asistente.SetExpresion(ExpresionesAsistente.deHecho1);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_1-audio_3"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_1-imagen_3"]);
        UI.Subtitulo.text = "Mira la firmeza de sus patas y la precisión de sus detalles. Los artesanos lograron capturar la esencia de un compañero leal que parece seguir esperando una orden del pasado.";
        yield return new WaitForSeconds(currentAudio.length);

        // ###################################################################### ############################################################# Trivia

        yield return new WaitForSeconds(2f);
        UI.PreguntaTrivia.text = "¿En qué material solían fabricar los Quimbaya sus piezas más famosas, además de la cerámica?";
        respuestaCorrectaTrivia = "Tumbaga (Oro y Cobre)";
        respuestasIncorrectasTrivia.Clear();
        respuestasIncorrectasTrivia.Add("Plata pura");
        respuestasIncorrectasTrivia.Add("Hierro forjado");
        configureTriviaButtons();
        yield return waitForTriviaToBeCompleted();
        yield return new WaitForSeconds(0.2f);

        // ###################################################################### ############################################################# Bloque 2

        // --------------- Primera parte
        asistente.SetExpresion(ExpresionesAsistente.pensando1);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_2-audio_1"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_2-imagen_1"]);
        UI.Subtitulo.text = "Pero su propósito iba mucho más allá de lo visual. Este silbato era una herramienta de conexión entre antiguos pobladores.";
        yield return new WaitForSeconds(currentAudio.length);

        // --------------- Segunda parte
        asistente.SetExpresion(ExpresionesAsistente.idle2);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_2-audio_2"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_2-imagen_2"]);
        UI.Subtitulo.text = "Su sonido agudo atravesaba la espesura de la selva y el vaho de las montañas, llevando mensajes entre aldeas separadas por grandes distancias.";
        yield return new WaitForSeconds(currentAudio.length);

        // --------------- Tercera parte
        asistente.SetExpresion(ExpresionesAsistente.amable2);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_2-audio_3"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_2-imagen_3"]);
        UI.Subtitulo.text = "Tal vez también guiaba rituales o acompañaba el viaje espiritual de quienes partían. Lo que escuchas hoy es la voz persistente de una civilización que se negaba a quedar en silencio.";
        yield return new WaitForSeconds(currentAudio.length);

        // ###################################################################### ############################################################# Trivia

        yield return new WaitForSeconds(2f);
        UI.PreguntaTrivia.text = "¿Cuál era una de las funciones principales de este silbato según los arqueólogos?";
        respuestaCorrectaTrivia = "Guía espiritual y comunicación";
        respuestasIncorrectasTrivia.Clear();
        respuestasIncorrectasTrivia.Add("Juguete para niños");
        respuestasIncorrectasTrivia.Add("Moneda de cambio");
        configureTriviaButtons();
        yield return waitForTriviaToBeCompleted();
        yield return new WaitForSeconds(0.2f);

        // ###################################################################### ############################################################# Bloque 3

        // --------------- Primera parte
        asistente.SetExpresion(ExpresionesAsistente.explicando2);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_3-audio_1"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_3-imagen_1"]);
        UI.Subtitulo.text = "Su regreso a la luz ocurrió casi como un acto del destino. La pieza fue encontrada durante la construcción del campus universitario.";
        yield return new WaitForSeconds(currentAudio.length);

        // --------------- Segunda parte
        asistente.SetExpresion(ExpresionesAsistente.amable2);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_3-audio_2"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_3-imagen_2"]);
        UI.Subtitulo.text = "Emergió desde las raíces de la tierra para recordarnos quiénes caminaron este suelo mucho antes que nosotros.";
        yield return new WaitForSeconds(currentAudio.length);

        // --------------- Tercera parte
        asistente.SetExpresion(ExpresionesAsistente.idle2);
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["bloque_3-audio_3"], (clip) =>
        {
            currentAudio = clip;
        });
        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["bloque_3-imagen_3"]);
        UI.Subtitulo.text = "Por eso el museo lo llama ‘nuestro ombligo’. Porque representa el punto de unión con nuestra identidad y nos recuerda que, bajo las aulas, aún late un corazón indígena lleno de memoria.";
        yield return new WaitForSeconds(currentAudio.length);

        // ###################################################################### ############################################################# Bloque 5

        UI.PreguntaTrivia.text = "¿Por qué el museo apoda a esta pieza como 'nuestro ombligo'?";
        respuestaCorrectaTrivia = "Porque es el punto de conexión con nuestra identidad";
        respuestasIncorrectasTrivia.Clear();
        respuestasIncorrectasTrivia.Add("Por su forma redonda");
        respuestasIncorrectasTrivia.Add("Porque fue encontrado en el centro de una plaza");
        configureTriviaButtons();
        yield return waitForTriviaToBeCompleted();
        yield return new WaitForSeconds(0.5f);
        finishSecuence();
    }
    public void answerTriviaCorrectly()
    {
        if (isProcessingCorrectAnswer) return;
        StartCoroutine(answerCorrect());
    }
    public void answerTriviaIncorrectly()
    {
        if (isProcessingCorrectAnswer) return;
        // Cancelar feedback incorrecto anterior
        if (incorrectAnswerCoroutine != null)
        {
            StopCoroutine(incorrectAnswerCoroutine);
        }

        incorrectAnswerCoroutine = StartCoroutine(answerIncorrect());
    }

    IEnumerator answerCorrect()
    {
        isProcessingCorrectAnswer = true;
        AudioController.Instance.StopSFX();
        AudioController.Instance.PlaySFX(respuestaCorrectaSFX);

        EntradaAudioClipSprite laiaFeedback = getRandomLaiaFeedback(laiaFelicitaciones);
        AudioClip audio = laiaFeedback.audioClip;
        Sprite image = laiaFeedback.sprite;

        // configurar LaIA
        UI.LaIaInTrivia.sprite = image;
        Animator laiaAnim = UI.LaIaInTrivia.GetComponent<Animator>();
        laiaAnim.SetBool("isTalking", true);

        AudioController.Instance.PlayDialogue(audio);

        yield return new WaitForSeconds(audio.length);

        laiaAnim.SetBool("isTalking", false);

        yield return new WaitForSeconds(2);

        isAnsweringTrivia = false;
    }
    IEnumerator answerIncorrect()
    {
        AudioController.Instance.StopSFX();
        AudioController.Instance.PlaySFX(respuestaIncorrectaSFX);

        EntradaAudioClipSprite laiaFeedback = getRandomLaiaFeedback(laiaIntentaNuevamente);
        AudioClip audio = laiaFeedback.audioClip;
        Sprite image = laiaFeedback.sprite;

        // Sprite LaIA
        UI.LaIaInTrivia.sprite = image;
        Animator laiaAnim = UI.LaIaInTrivia.GetComponent<Animator>();
        laiaAnim.SetBool("isTalking", true);

        AudioController.Instance.PlayDialogue(audio);

        yield return new WaitForSeconds(audio.length);

        laiaAnim.SetBool("isTalking", false);

        incorrectAnswerCoroutine = null;
    }
    IEnumerator waitForTriviaToBeCompleted()
    {
        UI.CanvasTrivia.SetActive(true);
        isAnsweringTrivia = true;
        isProcessingCorrectAnswer = false;
        while (isAnsweringTrivia) yield return null;
        UI.CanvasTrivia.SetActive(false);
    }
    void finishSecuence()
    {
        ControladorFlujo.Instance.FinishNarrativaState();
    }
    void configureTriviaButtons()
    {
        // Limpiar listeners anteriores
        foreach (Button btn in UI.BotonesTrivia)
        {
            btn.onClick.RemoveAllListeners();
        }

        // --------- Organizar botones
        UI.SetDistribucionBotones();

        // -------- botón correcto
        UI.BotonCorrecto.GetComponentInChildren<TMP_Text>().text = respuestaCorrectaTrivia;
        UI.BotonCorrecto.onClick.AddListener(answerTriviaCorrectly);
        SetButtonPressedColor(UI.BotonCorrecto, correctPressedColor);
        // -------- botones incorrectos
        for (int i = 0; i < UI.BotonesIncorrectos.Length; i++)
        {
            UI.BotonesIncorrectos[i].GetComponentInChildren<TMP_Text>().text = respuestasIncorrectasTrivia[i];
            // Añadir listener para respuesta incorrecta
            UI.BotonesIncorrectos[i].onClick.AddListener(answerTriviaIncorrectly);
            SetButtonPressedColor(UI.BotonesIncorrectos[i], incorrectPressedColor);
        }

    }
    /// <summary>
    /// Devuelve un Sprite cargado desde una ruta completa del sistema
    /// </summary>
    Sprite LoadSpriteFromPath(string imagePath)
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

    readonly Color correctPressedColor = new Color32(130, 255, 148, 255); // #82ff94
    readonly Color incorrectPressedColor = new Color32(255, 65, 65, 255); // #ff4141
    void SetButtonPressedColor(Button button, Color pressedColor)
    {
        ColorBlock colors = button.colors;
        colors.pressedColor = pressedColor;
        button.colors = colors;
    }
}
