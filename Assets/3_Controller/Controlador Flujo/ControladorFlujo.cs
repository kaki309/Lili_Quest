using System;
using UnityEngine;
using GLTFast;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

// ============================================================
// ENUM Y TIPOS DE DATOS
// ============================================================

/// <summary>
/// Estados posibles del flujo de interacción en Lili Quest
/// </summary>
public enum ControllerState
{
    EsperandoID = 0,            // Esperando lectura de RFID
    EsperandoInicioExperiencia = 1, // Esperando usuario inicie experiencia
    InteraccionRuptura = 2,     // Modo de interacción con ruptura
    SecuenciaNarrativa = 3,     // Presentación de info arqueológica
    Visor3D = 4                 // Visualización del modelo 3D
}

// ============================================================
// CLASE PRINCIPAL: CONTROLADOR DE FLUJO
// ============================================================

public class ControladorFlujo : MonoBehaviour
{
    // ---- SINGLETON ----
    public static ControladorFlujo Instance { get; private set; }

    // ---- PROPIEDADES DE ESTADO ----
    private ControllerState currentState = ControllerState.EsperandoID;
    public ControllerState CurrentState => currentState;
    public bool IsInitialized { get; private set; } = false;

    // ---- VARIABLES INTERNAS ----
    string lastRFIDRead = "";  // Para detectar cambios en RFID
    ParsedExperienceData currentExperienceData;
    SensorData interactionData;
    bool isInitializingState = false;
    bool isSwitchingState = false;
    bool isFlowPaused = false;
    bool isWaitingForArduino = false;


    // ============================================================
    // CICLO DE VIDA
    // ============================================================
    void Awake()
    {
        // Implementar Singleton
        if (Instance != null && Instance != this)
        {
            Debug.Log("[ControladorFlujo] Instancia ya existe. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ControladorFlujo] Singleton inicializado.");
    }
    void Start()
    {
        IsInitialized = true;
        Debug.Log("[ControladorFlujo] Inicializado.");
        InitializeEsperandoID();
    }
    void Update()
    {
        // Verificar que Arduino está listo
        if (!IsArduinoReady())
        {
            // Si no está listo iniciar una corrutina que espera hasta que lo encuentra
            // Y cambia el estado de la UI de pantalla principal
            if (!isWaitingForArduino) StartCoroutine(waitForArduino());
            return;
        }

        // Capturar datos desde arduino
        interactionData = ConectorArduino.Instance.GetSensorData();

        // Procesar lógica del estado actual mientras no esté en proceso de inicialización o cambio
        if (!isInitializingState || !isSwitchingState)
        {
            RunCurrentStateLogic();
        }
    }

    // ============================================================
    // MÁQUINA DE ESTADOS
    // ============================================================
    void RunCurrentStateLogic()
    {
        // Solamente se necesita realizar acciones constantes mientras se espera que se detecte
        // un ID válido o cuando se está esperando que la experiencia sea iniciada o terminada
        // Los demás estados se determinan y se iteran en base a eventos definidos
        switch (currentState)
        {
            case ControllerState.EsperandoID:
                UpdateEsperandoID();
                break;

            case ControllerState.Visor3D:
                UpdateVisor3D();
                break;
        }
    }

    // ============================================================
    // PREVIO A ESTADOS
    // ============================================================
    #region PREVIO A ESTADOS
    IEnumerator waitForArduino()
    {
        isWaitingForArduino = true;

        while (!IsArduinoReady()) yield return null;

        GestorInterfazPantallaInicio.Instance.textoEsperandoControles.SetActive(false);
        GestorInterfazPantallaInicio.Instance.textoEsperandoLectura.SetActive(true);
        GestorInterfazPantallaInicio.Instance.RuedaDecorativa.SetTrigger("girar");
        isWaitingForArduino = false;
    }
    #endregion

    // ============================================================
    // ESTADO: ESPERANDO ID
    // ============================================================
    #region ESTADO: ESPERANDO ID

    void InitializeEsperandoID()
    {
        isInitializingState = true;
        currentState = ControllerState.EsperandoID;
        Debug.Log("[ControladorFlujo] Inicializando estado: EsperandoID");
        lastRFIDRead = "";
        isInitializingState = false;
    }

    void UpdateEsperandoID()
    {
        // No ejecutar código si no hay cambios en el RFID
        if (string.IsNullOrEmpty(interactionData.RFID) || interactionData.RFID == lastRFIDRead) return;

        Debug.Log($"[ControladorFlujo] RFID leído: {interactionData.RFID}");
        lastRFIDRead = interactionData.RFID;
        string[] availableIds = ControladorDatos.Instance.GetExperienceIds();

        // Validar RFID
        foreach (string id in availableIds)
        {
            if (interactionData.RFID == id)
            {
                Debug.Log("[ControladorFlujo] ID válido detectado. Transitando a EsperandoInicioExperiencia...");
                TransitionToEsperandoInicioExperiencia();
                break;
            }
            else
            {
                Debug.Log("[ControladorFlujo] ID inválido. Permaneciendo en EsperandoID");
            }
        }
    }
    void TransitionToEsperandoInicioExperiencia()
    {
        ExitEsperandoID();
        currentState = ControllerState.EsperandoInicioExperiencia;

        currentExperienceData = ControladorDatos.Instance.GetExperienceData(lastRFIDRead);
        InitializeEsperandoInicioExperiencia();
    }
    void ExitEsperandoID()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoID");
    }
    #endregion

    // ============================================================
    // ESTADO: ESPERANDO INICIO EXPERIENCIA
    // ============================================================
    #region ESTADO: ESPERANDO INICIO EXPERIENCIA

    void InitializeEsperandoInicioExperiencia()
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: EsperandoInicioExperiencia");

        GestorInterfazPantallaInicio.Instance.RuedaDecorativa.SetTrigger("girarRapido");
        AudioController.Instance.PlaySFX(GestorInterfazPantallaInicio.Instance.SfxRueda);
        // Desactivar texto de espera de lectura
        GestorInterfazPantallaInicio.Instance.textoEsperandoLectura.SetActive(false);
        // Activar botón para iniciar la experiencia
        GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia.gameObject.SetActive(true);

        GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia.onClick.AddListener(TransitionToInteraccionRuptura);

        EventSystem.current.SetSelectedGameObject(GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia.gameObject);

        ConectorArduino.onButtonClicked += performClickOnCurrentSelected;

        isInitializingState = false;
    }

    void TransitionToInteraccionRuptura()
    {
        isSwitchingState = true;
        ConectorArduino.onButtonClicked -= performClickOnCurrentSelected;
        ExitEsperandoInicioExperiencia();
        currentState = ControllerState.InteraccionRuptura;
        Debug.Log("[ControladorFlujo] Transición a: InteraccionRuptura");
        LanzadorEscenas.Instance.cargarEscenaYEjecutar(EscenasSistema.Visor3D, (onDone) =>
        {
            StartCoroutine(InitializeInteraccionRuptura(onDone));
        });
        isSwitchingState = false;
    }
    void ExitEsperandoInicioExperiencia()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoInicioExperiencia");
    }
    #endregion

    // ============================================================
    // ESTADO: INTERACCIÓN Y RUPTURA
    // ============================================================
    #region ESTADO: INTERACCIÓN Y RUPTURA
    IEnumerator InitializeInteraccionRuptura(Action onDone)
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: InteraccionRuptura");

        if (!DoesCurrentExperienceModelExists()) yield break;

        // Esperamos que el gestor de interfaz esté creado en escena
        yield return waitForObjectToBeOnScene<GestorInterfazPantallasVisor3D>();

        // Desactivamos el movimiento de cámara -> ES TEMPORAL
        MovimientoCamara movimientoCamara = Camera.main.GetComponent<MovimientoCamara>();
        movimientoCamara.enabled = false;

        // Instanciamos el modelo 3D
        GameObject container = GestorInterfazPantallasVisor3D.Instance.ContenedorModelo3D;
        LoadModelAsync(container, currentExperienceData.modeloPath);
        while (isFlowPaused) yield return null;

        // Añadimos el script de ruptura al objeto instanciado y lo configuramos
        GameObject model = container.transform.GetChild(0).gameObject;
        model.AddComponent<Rigidbody>().useGravity = false;
        model.AddComponent<BoxCollider>();
        // Utilizamos el componente Fractura puesto en el contenedor que ya está previamente configurado con las opciones de ruptura, y lo copiamos directamente a nuestro modelo 3D
        container.GetComponent<Fractura>().CopyFractureComponent(model);
        // Generamos las fracturas
        model.GetComponent<Fractura>().CauseFracture();
        // Actualizamos las referencias en la cámara
        movimientoCamara.SetObjetivo(model);
        // Desactivamos panel de salir (No se permite en este momento)
        GestorInterfazPantallasVisor3D.Instance.PanelSalir.SetActive(false);
        // Activamos pantalla negra detrás del asistente
        GestorInterfazPantallasVisor3D.Instance.FondoNegro.SetActive(true);

        // Avisamos a la pantalla de carga que ya terminó el proceso
        onDone?.Invoke();
        yield return new WaitForSeconds(2f);

        // Ejecutamos secuencia de introducción del asistente
        yield return ControladorAsistente.Instance.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.IntroducciónAntesDeRuptura());
        yield return new WaitForSeconds(0.6f);
        // Desactivamos pantalla negra
        GestorInterfazPantallasVisor3D.Instance.FondoNegro.SetActive(false);
        // Activamos movimiento de cámara nuevamente
        movimientoCamara.enabled = true;

        isInitializingState = false;
    }
    /// <summary>
    /// Método llamado desde el script movimientoCamara para avisar ruptura del modelo    
    /// </summary>
    public void FragmentModel() => StartCoroutine(TransitionToSecuenciaNarrativa());
    IEnumerator TransitionToSecuenciaNarrativa()
    {
        isSwitchingState = true;

        // Ejecutamos corrutina de salida y esperamos su terminación (por el yield return)
        yield return ExitInteraccionRuptura();

        Debug.Log("[ControladorFlujo] Transición a: SecuenciaNarrativa");
        currentState = ControllerState.SecuenciaNarrativa;
        StartCoroutine(InitializeSecuenciaNarrativa());

        if (LanzadorEscenas.Instance != null)
        {
            LanzadorEscenas.Instance.cargarEscena(EscenasSistema.Narrativa);
        }
        isSwitchingState = false;
    }
    IEnumerator ExitInteraccionRuptura()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: InteraccionRuptura");
        // Esperamos al efecto de ruptura
        yield return new WaitForSeconds(3.5f);
        // Pantalla negra
        GestorInterfazPantallasVisor3D.Instance.FondoNegro.SetActive(true);
        // Secuencia de asistente
        yield return ControladorAsistente.Instance.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.RupturaModelo());
    }
    #endregion

    // ============================================================
    // ESTADO: SECUENCIA NARRATIVA 
    // ============================================================
    #region ESTADO: SECUENCIA NARRATIVA 

    IEnumerator InitializeSecuenciaNarrativa()
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: SecuenciaNarrativa");

        yield return StartCoroutine(waitForObjectToBeOnScene<GestorInterfazPantallaNarrativa>());
        isInitializingState = false;
    }
    /// <summary>
    /// Método llamado desde el script ControladorNarrativa para avisar terminación de toda la secuencia
    /// </summary>
    public void FinishNarrativaState() => TransitionToVisor3D();
    void TransitionToVisor3D()
    {
        ExitSecuenciaNarrativa();
        currentState = ControllerState.Visor3D;
        Debug.Log("[ControladorFlujo] Transición a: Visor3D");
        LanzadorEscenas.Instance.cargarEscenaYEjecutar(EscenasSistema.Visor3D, (onDone) => StartCoroutine(InitializeVisor3D(onDone)));
    }
    void ExitSecuenciaNarrativa()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: SecuenciaNarrativa");
    }
    #endregion

    // ============================================================
    // ESTADO: VISOR 3D 
    // ============================================================
    #region ESTADO: VISOR 3D 
    IEnumerator InitializeVisor3D(Action onDone)
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: Visor3D");

        if (!DoesCurrentExperienceModelExists()) yield break;

        // Esperamos que el gestor de interfaz esté creado en escena
        yield return waitForObjectToBeOnScene<GestorInterfazPantallasVisor3D>();

        // Desactivamos movimiento de cámara y fractura
        MovimientoCamara movimientoCamara = Camera.main.GetComponent<MovimientoCamara>();
        movimientoCamara.enabled = false;
        movimientoCamara.activateFracture = false;

        // Instanciamos el modelo 3D
        GameObject container = GestorInterfazPantallasVisor3D.Instance.ContenedorModelo3D;
        LoadModelAsync(container, currentExperienceData.modeloPath);
        while (isFlowPaused) yield return null;

        // Obtenemos el modelo instanciado y actualizamos referencia en cámara
        GameObject model = container.transform.GetChild(0).gameObject;
        movimientoCamara.SetObjetivo(model);
        // CONFIGURAMOS CANVAS POP UP
        GestorInterfazPantallasVisor3D.Instance.PanelSalir.SetActive(false);

        // Limpiar listeners anteriores para evitar acumulación
        GestorInterfazPantallasVisor3D.Instance.BotonCancelar.onClick.RemoveAllListeners();
        GestorInterfazPantallasVisor3D.Instance.BotonReiniciar.onClick.RemoveAllListeners();

        // Agregar nuevos listeners
        GestorInterfazPantallasVisor3D.Instance.BotonCancelar.onClick.AddListener(ClosePauseMenu);
        GestorInterfazPantallasVisor3D.Instance.BotonReiniciar.onClick.AddListener(ReturnToStart);
        // Activamos pantalla negra detrás del asistente
        GestorInterfazPantallasVisor3D.Instance.FondoNegro.SetActive(true);

        // Avisamos a la pantalla de carga que ya terminó el proceso
        onDone?.Invoke();
        yield return new WaitForSeconds(2f);

        // Ejecutamos secuencia de introducción del asistente
        yield return ControladorAsistente.Instance.PlaySequence(ConfiguracionAsistente.Instance.Secuencias.Visor3DLibre());
        yield return new WaitForSeconds(0.6f);
        // Desactivamos pantalla negra
        GestorInterfazPantallasVisor3D.Instance.FondoNegro.SetActive(false);
        // Activamos movimiento de cámara nuevamente
        movimientoCamara.enabled = true;

        // Agregamos click al conector para mostrar menu de pausa
        ConectorArduino.onButtonClicked += ShowPauseMenu;

        // Salimos de la inicialización
        isInitializingState = false;
        yield break;
    }
    void UpdateVisor3D()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPauseMenu();
        }
    }
    private void ExitVisor3D()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: Visor3D");

        // Quitamos click al conector para mostrar menu de pausa
        ConectorArduino.onButtonClicked -= ShowPauseMenu;

        // Limpiar listeners de botones para evitar ejecuciones fantasma
        GestorInterfazPantallasVisor3D.Instance.BotonCancelar.onClick.RemoveAllListeners();
        GestorInterfazPantallasVisor3D.Instance.BotonReiniciar.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Muestra el Canvas de menú de pausa en Visor3D
    /// </summary>
    void ShowPauseMenu()
    {
        // Quitamos click al conector para evitar ejecuciones dobles
        ConectorArduino.onButtonClicked -= ShowPauseMenu;
        MovimientoCamara movimientoCamara = Camera.main.GetComponent<MovimientoCamara>();
        movimientoCamara.enabled = false;
        GestorInterfazPantallasVisor3D.Instance.PanelSalir.SetActive(true);
        Debug.Log("[ControladorFlujo] Canvas de pausa mostrado");
    }

    /// <summary>
    /// Cierra el Canvas de menú de pausa en Visor3D (Botón: Mantener en pantalla)
    /// </summary>
    void ClosePauseMenu()
    {
        // Agregamos click al conector para mostrar menu de pausa nuevamente
        ConectorArduino.onButtonClicked += ShowPauseMenu;
        MovimientoCamara movimientoCamara = Camera.main.GetComponent<MovimientoCamara>();
        movimientoCamara.enabled = true;
        GestorInterfazPantallasVisor3D.Instance.PanelSalir.SetActive(false);
        Debug.Log("[ControladorFlujo] Canvas de pausa cerrado");
    }

    /// <summary>
    /// Regresa al inicio del sistema desde Visor3D (Botón: Volver al inicio)
    /// </summary>
    public void ReturnToStart()
    {
        isSwitchingState = true;
        Debug.Log("[ControladorFlujo] Regresando al inicio desde Visor3D");
        ExitVisor3D();
        ResetToStartState();
        isSwitchingState = false;
    }
    #endregion
    // ============================================================
    // MÉTODOS PÚBLICOS
    // ============================================================
    #region MÉTODOS PÚBLICOS
    public ParsedExperienceData GetCurrentExperienceData()
    {
        return currentExperienceData;
    }
    #endregion
    // ============================================================
    // MÉTODOS PRIVADOS
    // ============================================================
    #region MÉTODOS PRIVADOS

    /// <summary>
    /// Verifica que Arduino está conectado Y en el estado EsperandoRFID
    /// </summary>
    /// <returns>True si Arduino está listo para enviar datos, False en otro caso</returns>
    private bool IsArduinoReady()
    {
        return ConectorArduino.Instance != null && ConectorArduino.Instance.IsConnected;
    }

    /// <summary>
    /// Carga un modelo glTF/glB desde una ruta externa dentro de un placeholder específico.
    /// También establece el flujo del sistema como PAUSADO mediante la variable isFlowPaused hasta que el modelo se ha instanciado.
    /// </summary>
    async void LoadModelAsync(GameObject placeholder, string modelPath)
    {
        isFlowPaused = true;
        Debug.Log($"[ControladorFlujo] Cargando modelo: {modelPath}");

        var gltf = new GltfImport();

        if (await gltf.Load(new Uri(modelPath)))
        {
            await gltf.InstantiateMainSceneAsync(placeholder.transform);
            Debug.Log("[ControladorFlujo] Modelo instanciado correctamente");
        }
        else
        {
            Debug.LogError($"[ControladorFlujo] Fallo al cargar: {modelPath}");
        }

        isFlowPaused = false;
    }

    /// <summary>
    /// Inicia una corutina que busca constantemente un objeto en escena según el tipo pasado.
    /// </summary>
    IEnumerator waitForObjectToBeOnScene<T>() where T : UnityEngine.Object
    {
        T obj = null;

        while (obj == null)
        {
            obj = GameObject.FindObjectOfType<T>();
            yield return null;
        }
    }
    bool DoesCurrentExperienceModelExists()
    {
        if (currentExperienceData == null || string.IsNullOrEmpty(currentExperienceData.modeloPath))
        {
            Debug.LogError("[ControladorFlujo] La información de la experiencia o el modelo 3D no es válida");
            return false;
        }
        else { return true; }
    }
    void ResetToStartState()
    {
        Debug.Log("[ControladorFlujo] Reiniciando sistema");

        // Reiniciar Arduino a lectura de RFID
        ConectorArduino.Instance.RequestReset();
        Debug.Log("[ControladorFlujo] Solicitado a Arduino: Reset");

        if (LanzadorEscenas.Instance != null)
        {
            LanzadorEscenas.Instance.cargarEscena(EscenasSistema.Inicio);
        }

        // Cambiar al estado inicial
        InitializeEsperandoID();
    }
    void performClickOnCurrentSelected()
    {
        EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
    }

    #endregion
}
