using System;
using UnityEngine;
using GLTFast;
using UnityEngine.UI;
using System.Collections;
using System.CodeDom;

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
    bool hasFragmentedModel = false;


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
        // No procesar si Arduino no está conectado
        if (!IsArduinoReady()) return;
        // Capturar datos desde arduino
        interactionData = ConectorArduino.Instance.GetSensorData();

        // Procesar lógica del estado actual mientras no esté en proceso de inicialización
        if (!isInitializingState)
        {
            RunCurrentStateLogic();
        }

        // Revisar si la experiencia se está cancelando al cambiar de RFID
        //CheckIfExperienceIsInterrupted();
    }

    // ============================================================
    // MÁQUINA DE ESTADOS
    // ============================================================
    void RunCurrentStateLogic()
    {
        switch (currentState)
        {
            case ControllerState.EsperandoID:
                UpdateEsperandoID();
                break;

            case ControllerState.EsperandoInicioExperiencia:
                UpdateEsperandoInicioExperiencia();
                break;

            case ControllerState.InteraccionRuptura:
                UpdateInteraccionRuptura();
                break;

            case ControllerState.SecuenciaNarrativa:
                UpdateSecuenciaNarrativa();
                break;

            case ControllerState.Visor3D:
                UpdateVisor3D();
                break;

            default:
                Debug.LogWarning("[ControladorFlujo] Estado no reconocido: " + currentState);
                break;
        }
    }

    #region ESTADO: ESPERANDO ID

    void InitializeEsperandoID()
    {
        isInitializingState = true;
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

        // Solicitar a Arduino que cambie a modo LeyendoDatos
        ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
        Debug.Log("[ControladorFlujo] Solicitado a Arduino: LeyendoDatos");

        currentExperienceData = ControladorDatos.Instance.GetExperienceData(lastRFIDRead);
        InitializeEsperandoInicioExperiencia();
    }
    void ExitEsperandoID()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoID");
    }
    #endregion

    #region ESTADO: ESPERANDO INICIO EXPERIENCIA

    void InitializeEsperandoInicioExperiencia()
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: EsperandoInicioExperiencia");

        // Cargar el modelo 3D desde la ruta externa de forma asincrónica
        GameObject container = GestorInterfazPantallaInicio.Instance.ContenedorModelo3D;
        if (currentExperienceData != null && !string.IsNullOrEmpty(currentExperienceData.modeloPath))
        {
            LoadModelAsync(container, currentExperienceData.modeloPath);
        }
        else
        {
            Debug.LogError("[ControladorFlujo] No se pudo cargar el modelo 3D de la experiencia");
        }

        // Desactivar texto de espera de lectura
        GestorInterfazPantallaInicio.Instance.textoEsperandoLectura.SetActive(false);

        // Activar botón para iniciar la experiencia
        Button startButton = GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia;
        startButton.gameObject.SetActive(true);
        startButton.onClick.AddListener(TransitionToInteraccionRuptura);
        isInitializingState = false;
    }
    void UpdateEsperandoInicioExperiencia()
    {
        if (interactionData.ButtonPressed)
        {
            GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia.onClick.Invoke();
            TransitionToInteraccionRuptura();
        }
    }
    void TransitionToInteraccionRuptura()
    {
        ExitEsperandoInicioExperiencia();
        currentState = ControllerState.InteraccionRuptura;
        Debug.Log("[ControladorFlujo] Transición a: InteraccionRuptura");
        LanzadorEscenas.Instance.cargarEscenaYEjecutar(EscenasSistema.Visor3D, (onDone) => InitializeInteraccionRuptura(onDone));
        //InitializeInteraccionRuptura();
    }
    void ExitEsperandoInicioExperiencia()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoInicioExperiencia");
    }
    #endregion

    #region ESTADO: INTERACCIÓN Y RUPTURA
    void InitializeInteraccionRuptura(Action onDone = null)
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: InteraccionRuptura");

        StartCoroutine(asyncGetGameObjectOfType<GestorInterfazPantallasVisor3D>((obj) =>
        {
            Camera camara = Camera.main;
            // Desactivamos el movimiento de cámara -> ES TEMPORAL
            camara.GetComponent<MovimientoCamara>().enabled = false;
            // Instanciamos el modelo 3D
            GameObject container = GestorInterfazPantallasVisor3D.Instance.ContenedorModelo3D;
            if (currentExperienceData != null && !string.IsNullOrEmpty(currentExperienceData.modeloPath))
            {
                LoadModelAsync(container, currentExperienceData.modeloPath, () =>
                {
                    // Callback para cuando se instancie el modelo

                    // Añadimos el script de ruptura al objeto
                    // y configuramos el objeto
                    GameObject model = container.transform.GetChild(0).gameObject;
                    model.AddComponent<Rigidbody>().useGravity = false;
                    model.AddComponent<BoxCollider>();
                    // Utilizamos el componente Fractura puesto en el HolderModelo3D para copiarlo directamente a nuestro modelo 3D y tenerlo ya preconfigurado.
                    container.GetComponent<Fractura>().CopyFractureComponent(model);
                    model.GetComponent<Fractura>().CauseFracture();
                    // Actualizamos las referencias en la cámara
                    camara.GetComponent<MovimientoCamara>().SetObjetivo(model);
                    //
                    //
                    // El movimiento de cámara se activa nuevamente mediante los CALLBACKS de la fractura del modelo.
                    //
                    //
                    // Salimos de la inicialización
                    onDone?.Invoke();
                    isInitializingState = false;
                });
            }
            else
            {
                Debug.LogError("[ControladorFlujo] No se pudo cargar el modelo 3D de la experiencia");
            }
        }));
    }

    void UpdateInteraccionRuptura()
    {
        // Si el modelo aún no ha sido roto, o ya se está cambiando de estado entonces no ejecute nada
        if (!hasFragmentedModel || isSwitchingState) return;
        // Iniciar la transición
        StartCoroutine(TransitionToSecuenciaNarrativa());
    }
    IEnumerator TransitionToSecuenciaNarrativa()
    {
        isSwitchingState = true;
        ExitInteraccionRuptura();

        // Sincronizar MANUALMENTE este tiempo con lo que tarde las interacciones que realice el asistente al ocurrir la ruptura del modelo
        yield return new WaitForSeconds(5f);

        Debug.Log("[ControladorFlujo] Transición a: SecuenciaNarrativa");
        currentState = ControllerState.SecuenciaNarrativa;
        InitializeSecuenciaNarrativa();

        if (LanzadorEscenas.Instance != null)
        {
            LanzadorEscenas.Instance.cargarEscena(EscenasSistema.Narrativa);
        }
        isSwitchingState = false;
        yield break;
    }
    void ExitInteraccionRuptura()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: InteraccionRuptura");

        // Restaurar bandera para habilitar nuevas interacciones con LiliQuest en una única sesión (Es decir no se ha cerrado el programa).
        hasFragmentedModel = false;

        // Aquí deben ir las interacciones del asistente que se realizarán cuando ocurra la ruptura del modelo
    }
    #endregion

    #region ESTADO: SECUENCIA NARRATIVA 

    private void InitializeSecuenciaNarrativa()
    {
        Debug.Log("[ControladorFlujo] Inicializando estado: SecuenciaNarrativa");
        // TODO: Implementar lógica de narrativa en fase futura
        // - Cambio de escena correspondiente
    }

    private void UpdateSecuenciaNarrativa()
    {
        // TODO: Implementar lógica de actualización para SecuenciaNarrativa
        // - Presentación de contenido histórico
        // - Reprodución de diálogos del asistente
        // - Detección de fin de secuencia
    }
    private void TransitionToVisor3D()
    {
        ExitSecuenciaNarrativa();
        currentState = ControllerState.Visor3D;
        InitializeVisor3D();
        Debug.Log("[ControladorFlujo] Transición a: Visor3D");

        if (LanzadorEscenas.Instance != null)
        {
            LanzadorEscenas.Instance.cargarEscena(EscenasSistema.Visor3D);
        }
    }
    private void ExitSecuenciaNarrativa()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: SecuenciaNarrativa");
    }
    #endregion

    #region ESTADO: VISOR 3D 

    private void InitializeVisor3D()
    {
        isInitializingState = true;
        Debug.Log("[ControladorFlujo] Inicializando estado: Visor3D");

        StartCoroutine(asyncGetGameObjectOfType<GestorInterfazPantallasVisor3D>((obj) =>
        {
            Camera camara = Camera.main;
            // Activamos el movimiento de cámara y desactivamos ruptura de modelo
            camara.GetComponent<MovimientoCamara>().enabled = true;
            camara.GetComponent<MovimientoCamara>().activateFracture = false;
            // Instanciamos el modelo 3D
            GameObject container = GestorInterfazPantallasVisor3D.Instance.ContenedorModelo3D;
            if (currentExperienceData != null && !string.IsNullOrEmpty(currentExperienceData.modeloPath))
            {
                LoadModelAsync(container, currentExperienceData.modeloPath, () =>
                {
                    // Callback para cuando se instancie el modelo

                    // Obtenemos el modelo instanciado
                    GameObject model = container.transform.GetChild(0).gameObject;
                    // Actualizamos la referencia en la cámara
                    camara.GetComponent<MovimientoCamara>().SetObjetivo(model);
                    // Salimos de la inicialización
                    isInitializingState = false;
                });
            }
            else
            {
                Debug.LogError("[ControladorFlujo] No se pudo cargar el modelo 3D de la experiencia");
            }
        }));
    }

    private void UpdateVisor3D()
    {

    }

    private void ExitVisor3D()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: Visor3D");
    }
    #endregion

    #region MÉTODOS PÚBLICOS

    /// <summary>
    /// Reset abruptos: Vuelve al estado inicial y reinicia Arduino a lectura de RFID
    /// Puede ser llamado por cualquier controlador (menú, pausas de emergencia, etc.)
    /// </summary>
    public void ResetToStartState()
    {
        Debug.Log("[ControladorFlujo] Reset abrupto a EsperandoID. Reiniciando flujo...");

        if (currentState == ControllerState.EsperandoID) return;

        // Salir del estado actual
        switch (currentState)
        {
            case ControllerState.EsperandoInicioExperiencia:
                ExitEsperandoInicioExperiencia();
                break;
            case ControllerState.InteraccionRuptura:
                ExitInteraccionRuptura();
                break;
            case ControllerState.SecuenciaNarrativa:
                ExitSecuenciaNarrativa();
                break;
            case ControllerState.Visor3D:
                ExitVisor3D();
                break;
        }

        // Reiniciar Arduino a lectura de RFID
        ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
        Debug.Log("[ControladorFlujo] Solicitado a Arduino: EsperandoRFID (Reset)");

        if (LanzadorEscenas.Instance != null)
        {
            LanzadorEscenas.Instance.cargarEscena(EscenasSistema.Inicio);
        }

        // Cambiar al estado inicial
        currentState = ControllerState.EsperandoID;
        InitializeEsperandoID();
    }
    public void SetModelFragmentedState(bool value)
    {
        hasFragmentedModel = value;
        GestorInterfazPantallasVisor3D.Instance.AudioSource.Play();
    }
    public void finishNarrativaState()
    {
        TransitionToVisor3D();
    }
    #endregion

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
    /// </summary>
    async void LoadModelAsync(GameObject placeholder, string modelPath, Action callback = null)
    {
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

        callback?.Invoke();
    }
    IEnumerator asyncGetGameObjectOfType<T>(Action<T> callback) where T : UnityEngine.Object
    {
        T obj = null;

        while (obj == null)
        {
            obj = GameObject.FindObjectOfType<T>();
            yield return null;
        }
        // Pasamos el objeto al callback
        callback?.Invoke(obj);
    }
    void CheckIfExperienceIsInterrupted()
    {
        if (string.IsNullOrEmpty(interactionData.RFID) || interactionData.RFID == lastRFIDRead) return;
        ResetToStartState();
    }

    #endregion
}
