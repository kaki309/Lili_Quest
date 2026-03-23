using System;
using System.Collections;
using UnityEngine;
using GLTFast;
using GLTFast.Loading;

// ============================================================
// ENUM Y TIPOS DE DATOS
// ============================================================

/// <summary>
/// Estados posibles del flujo de interacción en Lili Quest
/// </summary>
public enum ControllerState
{
    EsperandoID = 0,            // Esperando lectura de RFID
    EsperandoInicioExperiencia = 1, // Esperando que el usuario inicie la experiencia
    InteraccionRuptura = 2,     // Modo de interacción y ruptura (ruptura del objeto)
    SecuenciaNarrativa = 3,     // Presentación de contenido histórico/narrativo
    Visor3D = 4                 // Visualización del modelo 3D interactivo
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

    // ---- CONFIGURACIÓN ----
    [SerializeField] private string VALID_RFID_ID = "silbato_forma_perro_cultura_quimbaya";

    // ---- VARIABLES INTERNAS ----
    private string lastRFIDRead = "";  // Para detectar cambios en RFID
    private ParsedExperienceData currentExperienceData;


    // ============================================================
    // CICLO DE VIDA
    // ============================================================

    private void Awake()
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

    private void Start()
    {
        IsInitialized = true;
        Debug.Log("[ControladorFlujo] Inicializado.");
        InitializeEsperandoID();
    }

    private void Update()
    {
        // Guardia: Verificar que Arduino está listo
        if (!IsArduinoReady()) return;  // No procesar si Arduino no está conectado
        
        // Procesar lógica del estado actual
        ProcessStateTransition();
    }


    // ============================================================
    // MÁQUINA DE ESTADOS
    // ============================================================

    private void ProcessStateTransition()
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


    // ============================================================
    // ESTADO: ESPERANDO ID
    // ============================================================

    private void InitializeEsperandoID()
    {
        Debug.Log("[ControladorFlujo] Inicializando estado: EsperandoID");
        lastRFIDRead = "";
    }

    private void UpdateEsperandoID()
    {
        // Leer datos de sensores
        SensorData sensorData = ConectorArduino.Instance.GetSensorData();

        // Detectar cambio en RFID (evitar duplicados por frame)
        if (!string.IsNullOrEmpty(sensorData.RFID) && sensorData.RFID != lastRFIDRead)
        {
            lastRFIDRead = sensorData.RFID;
            Debug.Log($"[ControladorFlujo] RFID leído: {sensorData.RFID}");

            // Validar RFID
            if (sensorData.RFID == VALID_RFID_ID)
            {
                Debug.Log("[ControladorFlujo] ID válido detectado. Transitando a EsperandoInicioExperiencia...");
                TransitionToEsperandoInicioExperiencia();
            }
            else
            {
                Debug.Log("[ControladorFlujo] ID inválido. Permaneciendo en EsperandoID");
            }
        }
    }

    private void ExitEsperandoID()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoID");
    }

    private void TransitionToEsperandoInicioExperiencia()
    {
        ExitEsperandoID();
        currentState = ControllerState.EsperandoInicioExperiencia;
        
        // Solicitar a Arduino que cambie a modo LeyendoDatos
        ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
        Debug.Log("[ControladorFlujo] Solicitado a Arduino: LeyendoDatos");
        
        currentExperienceData = ControladorDatos.Instance.GetExperienceData(lastRFIDRead);
        InitializeEsperandoInicioExperiencia();
    }


    // ============================================================
    // ESTADO: ESPERANDO INICIO EXPERIENCIA
    // ============================================================

    private void InitializeEsperandoInicioExperiencia()
    {
        Debug.Log("[ControladorFlujo] Inicializando estado: EsperandoInicioExperiencia");
        GameObject placeholder = GameObject.Find("PlaceholderModelo3D");

        if (placeholder == null)
        {
            Debug.LogError("[ControladorFlujo] No se encontró: Placeholder Modelo3D");
            return;
        }

        // Cargar el modelo 3D desde la ruta externa de forma asincrónica
        if (currentExperienceData != null && !string.IsNullOrEmpty(currentExperienceData.modeloPath))
        {
            LoadModelAsync(placeholder, currentExperienceData.modeloPath);
        }
        else
        {
            Debug.LogError("[ControladorFlujo] No hay datos de experiencia o ruta del modelo vacía");
        }
    }

    private void UpdateEsperandoInicioExperiencia()
    {
        // TODO: Obtener evento del jugador iniciando la experiencia
        // - transicionar a Interacción Ruptura
    }

    private void ExitEsperandoInicioExperiencia()
    {
        Debug.Log("[ControladorFlujo] Saliendo del estado: EsperandoInicioExperiencia");
    }

    private void TransitionToInteraccionRuptura()
    {
        ExitEsperandoInicioExperiencia();
        currentState = ControllerState.InteraccionRuptura;
        InitializeInteraccionRuptura();
        Debug.Log("[ControladorFlujo] Transición a: InteraccionRuptura");
    }

    // ============================================================
    // ESTADO: INTERACCIÓN Y RUPTURA (SKELETON)
    // ============================================================

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
    }


    // ============================================================
    // ESTADO: SECUENCIA NARRATIVA (SKELETON)
    // ============================================================

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
    }


    // ============================================================
    // ESTADO: VISOR 3D (SKELETON)
    // ============================================================

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


    // ============================================================
    // MÉTODOS PÚBLICOS
    // ============================================================

    /// <summary>
    /// Reset abruptos: Vuelve al estado inicial y reinicia Arduino a lectura de RFID
    /// Puede ser llamado por cualquier controlador (menú, pausas de emergencia, etc.)
    /// </summary>
    public void ResetToStartState()
    {
        Debug.Log("[ControladorFlujo] Reset abrupto a EsperandoID. Reiniciando flujo...");

        if (currentState==ControllerState.EsperandoID) return;
        
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


    // ============================================================
    // MÉTODOS PRIVADOS
    // ============================================================

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
}
