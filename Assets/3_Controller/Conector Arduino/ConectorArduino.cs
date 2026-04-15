using System;
using System.IO.Ports;
using System.Collections;
using UnityEngine;

// ============================================================
// ENUMS Y TIPOS DE DATOS
// ============================================================

/// <summary>
/// Estados posibles del conector Arduino en el flujo de interacción
/// </summary>
public enum ArduinoState
{
    Inicializando = 0,    // Buscando puerto y estableciendo conexión
    EsperandoRFID = 1,    // Esperando lectura de RFID para iniciar interacción
    LeyendoDatos = 2      // Leyendo datos de sensores (joystick, pot, botón)
}

/// <summary>
/// Estructura para datos de joystick (parseado de "x-y")
/// </summary>
[Serializable]
public class JoystickData
{
    public int X;
    public int Y;

    public JoystickData(string raw)
    {
        string[] parts = raw.Split('-');
        X = int.TryParse(parts.Length > 0 ? parts[0] : "0", out int parsedX) ? parsedX : 0;
        Y = int.TryParse(parts.Length > 1 ? parts[1] : "0", out int parsedY) ? parsedY : 0;
    }

    public override string ToString() => $"({X}, {Y})";
}

/// <summary>
/// Estructura tipada para los datos de sensores que llegan desde Arduino
/// </summary>
[Serializable]
public class SensorData
{
    public string RFID;
    public JoystickData JOYSTICK;
    public string POT;
    public string BUTTON; // "P" = presionado, "S" = sin presionar

    public SensorData()
    {
        RFID = null;
        JOYSTICK = new JoystickData("0-0");
        POT = null;
        BUTTON = null;
    }

    public override string ToString() => 
        $"RFID: {RFID}, Joystick: {JOYSTICK}, POT: {POT}, Button: {BUTTON}";
}

/// <summary>
/// JSON temporal para desserialización desde Arduino
/// </summary>
[Serializable]
public class Payload
{
    public string RFID;
    public string JOYSTICK;
    public string POT;
    public string BUTTON;
}


// ============================================================
// CLASE PRINCIPAL: CONECTOR DE ARDUINO
// ============================================================

public class ConectorArduino : MonoBehaviour
{
    public static ConectorArduino Instance { get; private set; }

    [Header("Serial Settings")]
    public int baudRate = 9600;
    public float scanInterval = 3f;

    // ---- ESTADO ----
    private ArduinoState currentState = ArduinoState.Inicializando;
    public ArduinoState CurrentState => currentState;

    // ---- DATOS ----
    private SensorData sensorData = new SensorData();
    public SensorData GetSensorData() => sensorData;

    // ---- PUERTO SERIAL ----
    private SerialPort serial;
    public bool IsConnected => serial != null && serial.IsOpen;
    public bool isSearching { get; private set; } = false;

    // ---- MENSAJES DE PROTOCOLO (Arduino esperando estos literales) ----
    private const string IDENTIFICATION_MSG = "soy controles lili quest";      // Arduino envía al inicializar
    private const string RESPONSE_MSG = "te encontre";             // Respuesta que se envía al arduino
    private const string RESET_CONNECTION_MSG = "enviar rfid"; // Comando para volver a esperanza RFID
    private const string START_SENDING_DATA_MSG = "enviar datos de control"; // Comando para solicitar envío de datos de interacción

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentState = ArduinoState.Inicializando;
        StartCoroutine(ConnectionLoop());
    }

    private void OnApplicationQuit()
    {
        TryCloseSerial();
    }

    // ============================================================
    // LOOP PRINCIPAL DE CONEXIÓN
    // ============================================================

    /// <summary>
    /// Loop principal que mantiene la conexión y sincroniza con Arduino.
    /// Si se desconecta, reintenta automáticamente.
    /// </summary>
    private IEnumerator ConnectionLoop()
    {
        isSearching = true;

        while (true)
        {
            if (!IsConnected)
            {
                currentState = ArduinoState.Inicializando;
                yield return StartCoroutine(SearchAndConnectArduino());
            }

            yield return new WaitForSeconds(scanInterval);
        }
    }

    // ============================================================
    // BÚSQUEDA Y CONEXIÓN AL PUERTO SERIAL
    // ============================================================

    /// <summary>
    /// Escanea todos los puertos COM disponibles hasta encontrar Arduino.
    /// Valida identidad mediante handshake (IDENTIFICATION_MSG / RESPONSE_MSG)
    /// </summary>
    private IEnumerator SearchAndConnectArduino()
    {
        Debug.Log("[ConectorArduino] Buscando puerto...");

        string[] ports = SerialPort.GetPortNames();
        foreach (string port in ports)
        {
            Debug.Log($"[ConectorArduino] Probando puerto: {port}");

            SerialPort testPort = new SerialPort(port, baudRate);
            testPort.ReadTimeout = 500;
            testPort.NewLine = "\n";

            try
            {
                testPort.Open();
            }
            catch
            {
                continue; // Fallo al abrir → siguiente puerto
            }

            bool found = false;
            float timer = 0f;

            // Esperar respuesta de identidad durante 1.5s
            while (timer < 1.5f)
            {
                timer += Time.deltaTime;

                try
                {
                    if (testPort.BytesToRead > 0)
                    {
                        string line = testPort.ReadLine().Trim();
                        if (line == IDENTIFICATION_MSG)
                        {
                            Debug.Log($"[ConectorArduino] Encontrado en {port}");
                            found = true;

                            // Handshake: confirmar conexión
                            testPort.WriteLine(RESPONSE_MSG);
                            testPort.BaseStream.Flush();

                            serial = testPort;
                            currentState = ArduinoState.EsperandoRFID; // Estado inicial después de conexión
                            StartCoroutine(ReadDataLoop());
                            break;
                        }
                    }
                }
                catch { }

                yield return null;
            }

            if (!found)
            {
                testPort.Close();
            }
            else
            {
                yield break; // Conexión exitosa, salir del loop de búsqueda
            }
        }

        Debug.LogWarning("[ConectorArduino] No encontrado. Reintentando...");
    }

    // ============================================================
    // LOOP DE LECTURA DE DATOS
    // ============================================================

    /// <summary>
    /// Loop continuo que lee datos del puerto serial y los procesa.
    /// Se ejecuta solo cuando hay conexión establecida.
    /// </summary>
    private IEnumerator ReadDataLoop()
    {
        Debug.Log("[ConectorArduino] Iniciando lectura de datos...");

        while (IsConnected)
        {
            try
            {
                if (serial.BytesToRead > 0)
                {
                    string rawLine = serial.ReadLine();
                    ProcessIncomingData(rawLine);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ConectorArduino] Error de lectura: {ex.Message}");
                TryCloseSerial();
                yield break;
            }

            yield return null;
        }
    }

    // ============================================================
    // PROCESAMIENTO DE DATOS
    // ============================================================

    /// <summary>
    /// Deserializa JSON desde Arduino y actualiza sensorData.
    /// Los valores se guardan como strings para máxima compatibilidad.
    /// </summary>
    private void ProcessIncomingData(string rawLine)
    {
        try
        {
            // Deserializar JSON que viene de Arduino
            Payload payload = JsonUtility.FromJson<Payload>(rawLine);

            // Actualizar datos en la estructura tipada
            sensorData.RFID = payload.RFID;
            if (!string.IsNullOrEmpty(payload.JOYSTICK))
                sensorData.JOYSTICK = new JoystickData(payload.JOYSTICK);
            sensorData.POT = payload.POT;
            sensorData.BUTTON = payload.BUTTON;
        }
        catch
        {
            Debug.LogWarning($"[ConectorArduino] Paquete JSON inválido: {rawLine}");
        }
    }

    // ============================================================
    // API PÚBLICA PARA CAMBIAR ESTADO
    // ============================================================

    /// <summary>
    /// Solicita un cambio de estado en Arduino.
    /// Envía los comandos correspondientes según el nuevo estado deseado.
    /// 
    /// Ejemplo de uso desde otros controladores:
    ///   ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
    /// </summary>
    public void RequestState(ArduinoState newState)
    {
        if (newState == currentState)
        {
            Debug.LogWarning($"[ConectorArduino] Ya en estado {currentState}");
            return;
        }

        if (!IsConnected)
        {
            Debug.LogError("[ConectorArduino] No hay conexión. No se puede cambiar estado.");
            return;
        }

        currentState = newState;
        Debug.Log($"[ConectorArduino] Cambiando a estado: {currentState}");

        // Enviar comando a Arduino según el nuevo estado
        switch (newState)
        {
            case ArduinoState.EsperandoRFID:
                // Volver al inicio: Arduino a modo espera con RFID
                SendCommandToArduino(RESET_CONNECTION_MSG);
                break;

            case ArduinoState.LeyendoDatos:
                // Cambiar a lectura de sensores
                SendCommandToArduino(START_SENDING_DATA_MSG);
                break;

            case ArduinoState.Inicializando:
                Debug.LogWarning("[ConectorArduino] No se puede solicitar Inicializando manualmente");
                break;
        }
    }

    // ============================================================
    // COMUNICACIÓN CON ARDUINO
    // ============================================================

    /// <summary>
    /// Envía un comando al puerto serial hacia Arduino.
    /// Asegura que el mensaje se envíe correctamente.
    /// </summary>
    private void SendCommandToArduino(string command)
    {
        if (!IsConnected)
        {
            Debug.LogError("[ConectorArduino] Intento de envío sin conexión");
            return;
        }

        try
        {
            serial.WriteLine(command);
            serial.BaseStream.Flush();
            Debug.Log($"[ConectorArduino] Comando enviado: {command}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConectorArduino] Error al enviar comando: {ex.Message}");
        }
    }

    /// <summary>
    /// Cierra la conexión serial de forma segura.
    /// Se llama al desconectar o al salir de la aplicación.
    /// </summary>
    private void TryCloseSerial()
    {
        if (serial != null)
        {
            try
            {
                // Notificar a Arduino que vamos a cerrar
                if (serial.IsOpen)
                {
                    SendCommandToArduino(RESET_CONNECTION_MSG);
                }
                serial.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ConectorArduino] Error al cerrar puerto: {ex.Message}");
            }
            finally
            {
                serial = null;
            }
        }
    }
}
