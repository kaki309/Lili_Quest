using System;
using System.IO.Ports;
using System.Collections;
using UnityEngine;

/// <summary>
/// SCRIPT DE TESTING - Simula una placa Arduino con ciclo de vida realista
/// 
/// Propósito: Emular el comportamiento de Arduino enviando mensajes por puerto COM5
/// para verificar que ConectorArduino funciona correctamente.
/// 
/// Ciclo de vida realista:
/// 1. Handshake inicial
/// 2. Modo RFID_ONLY durante 10 segundos (solo envía RFID)
/// 3. Solicita cambio a modo FULL_SENSORS (joystick, potenciómetro, botón)
/// 4. Modo FULL_SENSORS durante 15 segundos (envía todos los sensores)
/// 5. Solicita cambio de vuelta a RFID_ONLY
/// 6. Loop infinito
/// 
/// Requisitos:
/// - COM5 debe estar configurado como puerto serial virtual (pair virtual)
/// - ConectorArduino debe estar escuchando en COM5
/// 
/// Observar en la consola de Unity los logs de [ConectorArduino] para verificar
/// que la conexión, handshake y lectura de datos funciona correctamente.
/// </summary>

/// <summary>
/// Enum para los modos de simulación del Arduino
/// </summary>
public enum SimulatorMode
{
    RFID_ONLY,      // Solo envía RFID (primeros 10 segundos)
    FULL_SENSORS    // Envía RFID + joystick + potenciómetro + botón
}

public class ArduinoSimulator : MonoBehaviour
{
    [Header("Configuración del Puerto")]
    [SerializeField] private string portName = "COM5";
    [SerializeField] private int baudRate = 9600;

    [Header("Protocolo")]
    [SerializeField] private string identificationMessage = "soy controles lili quest";
    [SerializeField] private float delayBeforeSendingIdentification = 1f;
    [SerializeField] private float delayAfterHandshake = 2f;

    [Header("Ciclo de Vida - Transiciones de Modo")]
    [SerializeField] private float timeBeforeRequestingFullSensors = 10f;   // Segundos en RFID_ONLY antes de cambiar a FULL_SENSORS
    [SerializeField] private float timeInFullSensorsMode = 15f;            // Segundos en FULL_SENSORS antes de volver a RFID_ONLY

    [Header("Envío de Datos")]
    [SerializeField] private float sendDataInterval = 0.5f;
    private float timeSinceLastSend = 0f;
    private float timeSinceHandshake = 0f;

    [Header("Datos leidos desde ConectorArduino")]
    [SerializeField] private ArduinoState estadoActual = ArduinoState.Inicializando;
    [SerializeField] private bool estaConectado = false;
    [SerializeField] private string rfidLeido = "";
    [SerializeField] private string joystickLeido = "";
    [SerializeField] private string potLeido = "";
    [SerializeField] private string botonLeido = "";
    
    [Header("Modo Actual del Simulador")]
    [SerializeField] private SimulatorMode modoActual = SimulatorMode.RFID_ONLY;

    private SerialPort serialPort;
    private bool isConnected = false;
    private bool handshakeDone = false;

    private void Start()
    {
        Debug.Log("[ArduinoSimulator]🤖 Simulador de Arduino iniciado");
        StartCoroutine(InitializeConnection());
    }

    private void Update()
    {
        // ========== CAPTURAR DATOS DE ConectorArduino ==========
        if (ConectorArduino.Instance != null)
        {
            estadoActual = ConectorArduino.Instance.CurrentState;
            estaConectado = ConectorArduino.Instance.IsConnected;
            
            SensorData datos = ConectorArduino.Instance.GetSensorData();
            if (datos != null)
            {
                rfidLeido = datos.RFID ?? "";
                joystickLeido = datos.JOYSTICK?.ToString() ?? "";
                potLeido = datos.POT ?? "";
                botonLeido = datos.BUTTON ?? "";
            }
        }

        // ========== CICLO DE VIDA CON TRANSICIONES AUTOMÁTICAS ==========
        if (handshakeDone && isConnected)
        {
            // Incrementar contador de tiempo desde handshake
            timeSinceHandshake += Time.deltaTime;

            // Lógica de transiciones de estado
            if (modoActual == SimulatorMode.RFID_ONLY)
            {
                // En modo RFID_ONLY, esperar timeBeforeRequestingFullSensors segundos
                if (timeSinceHandshake >= timeBeforeRequestingFullSensors)
                {
                    Debug.Log($"[ArduinoSimulator] ⏱️ {timeBeforeRequestingFullSensors} segundos transcurridos en RFID_ONLY. Solicitando cambio a FULL_SENSORS...");
                    
                    // Solicitar al ConectorArduino que cambie a modo de lectura de datos
                    if (ConectorArduino.Instance != null)
                    {
                        ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
                        modoActual = SimulatorMode.FULL_SENSORS;
                        timeSinceHandshake = 0f;  // Resetear contador para la nueva fase
                        Debug.Log("[ArduinoSimulator] 🔄 Cambio de modo a FULL_SENSORS");
                    }
                }
            }
            else if (modoActual == SimulatorMode.FULL_SENSORS)
            {
                // En modo FULL_SENSORS, esperar timeInFullSensorsMode segundos
                if (timeSinceHandshake >= timeInFullSensorsMode)
                {
                    Debug.Log($"[ArduinoSimulator] ⏱️ {timeInFullSensorsMode} segundos transcurridos en FULL_SENSORS. Solicitando cambio a RFID_ONLY...");
                    
                    // Solicitar al ConectorArduino que cambie a modo de espera RFID
                    if (ConectorArduino.Instance != null)
                    {
                        ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
                        modoActual = SimulatorMode.RFID_ONLY;
                        timeSinceHandshake = 0f;  // Resetear contador para la nueva fase
                        Debug.Log("[ArduinoSimulator] 🔄 Cambio de modo a RFID_ONLY");
                    }
                }
            }

            // ========== ENVÍO DE DATOS PERIÓDICOS ==========
            timeSinceLastSend += Time.deltaTime;
            if (timeSinceLastSend >= sendDataInterval)
            {
                // Enviar datos según el modo actual
                if (modoActual == SimulatorMode.RFID_ONLY)
                {
                    SendRFIDOnly();
                }
                else if (modoActual == SimulatorMode.FULL_SENSORS)
                {
                    SendFullSensorData();
                }
                
                timeSinceLastSend = 0f;
            }
        }

        // ========== MONITOREAR RESPUESTAS DE UNITY ==========
        if (isConnected && serialPort.BytesToRead > 0)
        {
            try
            {
                string line = serialPort.ReadLine().Trim();
                Debug.Log($"[ArduinoSimulator] 📩 Recibido desde Unity: \"{line}\"");

                // Responder a comandos
                if (line == "enviar rfid")
                {
                    Debug.Log("[ArduinoSimulator] → Comando recibido: ENVIAR RFID");
                }
                else if (line == "enviar datos de control")
                {
                    Debug.Log("[ArduinoSimulator] → Comando recibido: ENVIAR DATOS CONTROL");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArduinoSimulator] ⚠️ Error al leer respuesta: {ex.Message}");
            }
        }
    }

    private IEnumerator InitializeConnection()
    {
        // Esperar antes de intentar conectar (para que Unity esté listo)
        yield return new WaitForSeconds(delayBeforeSendingIdentification);

        // Listar puertos disponibles para debugging
        string[] availablePorts = SerialPort.GetPortNames();
        if (availablePorts.Length == 0)
        {
            Debug.LogError("[ArduinoSimulator] ❌ No hay puertos seriales disponibles");
            yield break;
        }

        Debug.Log($"[ArduinoSimulator] 📡 Puertos disponibles: {string.Join(", ", availablePorts)}");

        bool connectionSuccessful = false;

        try
        {
            Debug.Log($"[ArduinoSimulator] 🔌 Intentando abrir puerto: {portName}");
            
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;
            serialPort.NewLine = "\n";
            
            serialPort.Open();
            isConnected = true;
            connectionSuccessful = true;
            Debug.Log($"[ArduinoSimulator] ✓ Puerto {portName} abierto exitosamente");
        }
        catch (UnauthorizedAccessException ex)
        {
            isConnected = false;
            Debug.LogError($"[ArduinoSimulator] ❌ ACCESO DENEGADO en {portName}: {ex.Message}");
            Debug.LogError("[ArduinoSimulator] → Soluciones: 1) Ejecuta Unity como Administrador 2) Verifica que no hay otra app usando el puerto 3) Intenta otro puerto");
        }
        catch (Exception ex)
        {
            isConnected = false;
            Debug.LogError($"[ArduinoSimulator] ❌ Error al abrir puerto {portName}: {ex.Message}");
        }

        // Continuar solo si la conexión fue exitosa
        if (!connectionSuccessful)
            yield break;

        // Enviar mensaje de identificación
        yield return new WaitForSeconds(0.5f);
        SendIdentificationMessage();

        // Esperar handshake
        yield return new WaitForSeconds(delayAfterHandshake);
        handshakeDone = true;
        Debug.Log("[ArduinoSimulator] ✓ Handshake completado. Comenzando envío de datos...");
    }

    private void SendIdentificationMessage()
    {
        if (!isConnected) return;

        try
        {
            serialPort.WriteLine(identificationMessage);
            serialPort.BaseStream.Flush();
            Debug.Log($"[ArduinoSimulator] 📤 Enviado mensaje de identificación: \"{identificationMessage}\"");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ArduinoSimulator] ❌ Error al enviar identificación: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía solo el RFID sin datos de sensores (modo RFID_ONLY)
    /// </summary>
    private void SendRFIDOnly()
    {
        if (!isConnected) return;

        try
        {
            // En modo RFID_ONLY, solo envía RFID
            string rfid = "TAG_001";
            string json = $"{{\"RFID\":\"{rfid}\",\"JOYSTICK\":\"0-0\",\"POT\":\"0\",\"BUTTON\":\"S\"}}";

            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();

            Debug.Log($"[ArduinoSimulator] 📤 [RFID_ONLY] {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ArduinoSimulator] ❌ Error al enviar RFID: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía todos los datos de sensores (modo FULL_SENSORS)
    /// </summary>
    private void SendFullSensorData()
    {
        if (!isConnected) return;

        try
        {
            // En modo FULL_SENSORS, envía RFID + joystick + potenciómetro + botón con valores aleatorios
            string rfid = "TAG_001";
            int joystickX = UnityEngine.Random.Range(0, 255);
            int joystickY = UnityEngine.Random.Range(0, 255);
            int potValue = UnityEngine.Random.Range(0, 1024);
            string buttonValue = UnityEngine.Random.value > 0.7f ? "P" : "S";

            // Construir JSON en el formato esperado
            string json = $"{{\"RFID\":\"{rfid}\",\"JOYSTICK\":\"{joystickX}-{joystickY}\",\"POT\":\"{potValue}\",\"BUTTON\":\"{buttonValue}\"}}";

            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();

            Debug.Log($"[ArduinoSimulator] 📤 [FULL_SENSORS] {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ArduinoSimulator] ❌ Error al enviar datos de sensores: {ex.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                Debug.Log("[ArduinoSimulator] 👋 Puerto serial cerrado");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArduinoSimulator] ⚠️ Error al cerrar puerto: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Método para resetear la simulación (llamable desde otros scripts)
    /// </summary>
    public void ResetSimulation()
    {
        handshakeDone = false;
        modoActual = SimulatorMode.RFID_ONLY;
        timeSinceHandshake = 0f;
        timeSinceLastSend = 0f;
        Debug.Log("[ArduinoSimulator] 🔄 Simulación reseteada. Esperando nuevo handshake...");
    }

    /// <summary>
    /// Método para cambiar el intervalo de envío de datos en tiempo real
    /// </summary>
    public void SetSendInterval(float newInterval)
    {
        sendDataInterval = Mathf.Max(0.1f, newInterval);
        Debug.Log($"[ArduinoSimulator] ⏱️ Nuevo intervalo de envío: {sendDataInterval}s");
    }
}
