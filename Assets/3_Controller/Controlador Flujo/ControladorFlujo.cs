using System;
using UnityEngine;
using GLTFast;
using UnityEngine.UI;

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
        // Procesar lógica del estado actual
        RunCurrentStateLogic();
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
        Debug.Log("[ControladorFlujo] Inicializando estado: EsperandoID");
        lastRFIDRead = "";
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

        // Desactivar panel decorativo inicial
        GestorInterfazPantallaInicio.Instance.panelDecorativo.SetActive(false);

        // Activar botón para iniciar la experiencia
        Button startButton = GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia;
        startButton.gameObject.SetActive(true);
        startButton.onClick.AddListener(TransitionToInteraccionRuptura);
    }
    void UpdateEsperandoInicioExperiencia()
    {
        if (interactionData.ButtonPressed){
            GestorInterfazPantallaInicio.Instance.BotonInicioExperiencia.onClick.Invoke();
            TransitionToInteraccionRuptura();
        }
    }
    void TransitionToInteraccionRuptura()
    {
        ExitEsperandoInicioExperiencia();
        currentState = ControllerState.InteraccionRuptura;
        InitializeInteraccionRuptura();
        Debug.Log("[ControladorFlujo] Transición a: InteraccionRuptura");
        LanzadorEscenas.Instance.cargarEscena(EscenasSistema.InteraccionConRuptura);
    }
    void ExitEsperandoInicioExperiencia()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoInicioExperiencia");
    }
    #endregion

    #region ESTADO: INTERACCIÓN Y RUPTURA (SKELETON)

    private void InitializeInteraccionRuptura()
    {
        Debug.Log("[ControladorFlujo] Inicializando estado: InteraccionRuptura");
        // TODO: Implementar lógica de interacción y ruptura en fase futura
        // - Cambio de escena correspondiente
    }

    private void UpdateInteraccionRuptura()
    {
        // TODO: Implementar lógica de actualización para InteraccionRuptura
        // - Lectura de Joystick, Potenciómetro, Botón
        // - Manipulación del modelo 3D
        // - Detección de condiciones para transición a siguiente estado
    }

    private void ExitInteraccionRuptura()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: InteraccionRuptura");
    }

    private void TransitionToSecuenciaNarrativa()
    {
        ExitInteraccionRuptura();
        currentState = ControllerState.SecuenciaNarrativa;
        InitializeSecuenciaNarrativa();
        Debug.Log("[ControladorFlujo] Transición a: SecuenciaNarrativa");

        if (LanzadorEscenas.Instance != null)
        {
            LanzadorEscenas.Instance.cargarEscena(EscenasSistema.Narrativa);
        }
    }
    #endregion

    #region ESTADO: SECUENCIA NARRATIVA (SKELETON)

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

    private void ExitSecuenciaNarrativa()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: SecuenciaNarrativa");
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
    #endregion

    #region ESTADO: VISOR 3D (SKELETON)

    private void InitializeVisor3D()
    {
        Debug.Log("[ControladorFlujo] Inicializando estado: Visor3D");
        // TODO: Implementar lógica de visor 3D en fase futura
        // - Cambio de escena correspondiente
    }

    private void UpdateVisor3D()
    {
        // TODO: Implementar lógica de actualización para Visor3D
        // - Rotación/zoom del modelo 3D
        // - Interacción con puntos de interés
        // - Detección de fin de exploración
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

        // Cambiar al estado inicial
        currentState = ControllerState.EsperandoID;
        InitializeEsperandoID();
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
    private async void LoadModelAsync(GameObject placeholder, string modelPath)
    {
        Debug.Log($"[ControladorFlujo] Cargando modelo: {modelPath}");

        var gltf = new GltfImport();
        GameObject container = new GameObject("Modelo3D");
        container.transform.SetParent(placeholder.transform, false);

        if (await gltf.Load(new Uri(modelPath)))
        {
            await gltf.InstantiateMainSceneAsync(container.transform);
            Debug.Log("[ControladorFlujo] Modelo instanciado correctamente");
        }
        else
        {
            Destroy(container);
            Debug.LogError($"[ControladorFlujo] Fallo al cargar: {modelPath}");
        }
    }
    #endregion
}
