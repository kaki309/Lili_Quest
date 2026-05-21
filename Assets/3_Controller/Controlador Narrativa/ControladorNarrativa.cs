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

        // ######### Bloque 1: El Objeto y su Origen

        asistente.SetExpresion(ExpresionesAsistente.idle1);

        // Esperar a que el audio se cargue de forma asincrónica
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["secuencia_1"], (clip) =>
        {
            currentAudio = clip;
        });

        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["audiovisual_1"]);
        UI.Subtitulo.text = "Contempla esta pequeña figura... no es solo barro moldeado, es un eco que ha viajado por siglos. Este silbato en forma de perro pertenece a la cultura Quimbaya, maestros de la tierra en el corazón de Colombia. Mira la firmeza de sus patas; los antiguos artesanos capturaron la esencia de un compañero leal que parece estar esperando una orden del pasado.";

        yield return new WaitForSeconds(currentAudio.length + 2f);

        // ######### Bloque 2: La Trivia de Identidad (Interactividad)

        // Establecer texto de pregunta
        UI.PreguntaTrivia.text = "¿En qué material solían fabricar los Quimbaya sus piezas más famosas, además de la cerámica?";
        respuestaCorrectaTrivia = "Tumbaga (Oro y Cobre)";
        respuestasIncorrectasTrivia.Clear();
        respuestasIncorrectasTrivia.Add("Plata pura");
        respuestasIncorrectasTrivia.Add("Hierro forjado");
        configureTriviaButtons();

        yield return waitForTriviaToBeCompleted();

        // ######### Bloque 3: El Uso Ritual

        asistente.SetExpresion(ExpresionesAsistente.idle1);

        // Esperar a que el audio se cargue de forma asincrónica
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["secuencia_2"], (clip) =>
        {
            currentAudio = clip;
        });

        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["audiovisual_2"]);
        UI.Subtitulo.text = "Pero su propósito iba más allá de lo visual. Este silbato servía para la comunicación entre antiguos pobladores, pero no era una charla cualquiera. Su sonido agudo cortaba la espesura de la selva y el vaho de las montañas, conectando aldeas o, quizás, guiando las almas en su viaje final. Lo que escuchas es la voz de una civilización que se negaba a estar aislada.";

        yield return new WaitForSeconds(currentAudio.length + 2f);

        // ######### Bloque 4: La Trivia de Función

        UI.PreguntaTrivia.text = "¿Cuál era una de las funciones principales de este silbato según los arqueólogos?";
        respuestaCorrectaTrivia = "Guía espiritual y comunicación";
        respuestasIncorrectasTrivia.Clear();
        respuestasIncorrectasTrivia.Add("Juguete para niños");
        respuestasIncorrectasTrivia.Add("Moneda de cambio");
        configureTriviaButtons();

        yield return waitForTriviaToBeCompleted();

        // ######### Bloque 5: El Hallazgo y el 'Ombligo'

        asistente.SetExpresion(ExpresionesAsistente.idle1);

        // Esperar a que el audio se cargue de forma asincrónica
        yield return controladorAudio.PlayDialogueAsync(currentExperienceData.audios["secuencia_3"], (clip) =>
        {
            currentAudio = clip;
        });

        UI.Foto.sprite = LoadSpriteFromPath(currentExperienceData.imagenes["audiovisual_3"]);
        UI.Subtitulo.text = "Su regreso a la luz fue casi un acto del destino. Fue hallado durante la construcción del campus, emergiendo de las raíces para recordarnos quiénes pisaron este suelo antes que nosotros. Por eso, hoy, el museo lo llama 'nuestro ombligo'. Es el punto de unión que nos amarra a nuestra identidad y nos recuerda que, bajo las aulas, late un corazón indígena que aún tiene mucho por decir.";

        yield return new WaitForSeconds(currentAudio.length + 2f);

        // ######### Bloque 6: Trivia Final de Cierre

        UI.PreguntaTrivia.text = "¿Por qué el museo apoda a esta pieza como 'nuestro ombligo'?";
        respuestaCorrectaTrivia = "Porque es el punto de conexión con nuestra identidad";
        respuestasIncorrectasTrivia.Clear();
        respuestasIncorrectasTrivia.Add("Por su forma redonda");
        respuestasIncorrectasTrivia.Add("Porque fue encontrado en el centro de una plaza");
        configureTriviaButtons();

        yield return waitForTriviaToBeCompleted();

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
