using System;
using System.IO.Ports;
using System.Collections;
using UnityEngine;

/// <summary>
/// SCRIPT DE TESTING - Simula una placa Arduino
/// 
/// Propósito: Emular el comportamiento de Arduino enviando mensajes por puerto COM5
/// para verificar que ConectorArduino funciona correctamente.
/// 
/// Requisitos:
/// - COM5 debe estar configurado como puerto serial virtual (pair virtual)
/// - ConectorArduino debe estar escuchando en COM5
/// 
/// Observar en la consola de Unity los logs de [ConectorArduino] para verificar
/// que la conexión, handshake y lectura de datos funciona correctamente.
/// </summary>
public class ArduinoSimulator : MonoBehaviour
{
    [Header("Configuración del Puerto")]
    [SerializeField] private string portName = "COM5";
    [SerializeField] private int baudRate = 9600;

    [Header("Protocolo")]
    [SerializeField] private string identificationMessage = "soy controles lili quest";
    [SerializeField] private float delayBeforeSendingIdentification = 1f;
    [SerializeField] private float delayAfterHandshake = 2f;

    [Header("Envío de Datos")]
    [SerializeField] private float sendDataInterval = 0.5f;
    private float timeSinceLastSend = 0f;

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
        // Enviar datos periódicamente si ya completamos handshake
        if (handshakeDone && isConnected)
        {
            timeSinceLastSend += Time.deltaTime;
            if (timeSinceLastSend >= sendDataInterval)
            {
                SendSensorData();
                timeSinceLastSend = 0f;
            }
        }

        // Monitorear respuestas de Unity
        if (isConnected && serialPort.BytesToRead > 0)
        {
            try
            {
                string line = serialPort.ReadLine().Trim();
                Debug.Log($"[ArduinoSimulator] 📩 Recibido desde Unity: \"{line}\"");

                // Responder a comandos de Unity
                if (line == "enviar rfid")
                {
                    Debug.Log("[ArduinoSimulator] → Comando recibido: ENVIAR RFID. Esperando lectura de etiqueta...");
                    // En modo RFID, solo enviamos el valor de RFID (opcional para test)
                }
                else if (line == "enviar datos de control")
                {
                    Debug.Log("[ArduinoSimulator] → Comando recibido: ENVIAR DATOS. Comenzando envío de sensores...");
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

    private void SendSensorData()
    {
        if (!isConnected) return;

        try
        {
            // Generar datos de sensores simulados
            string rfid = "TAG_001";
            int joystickX = UnityEngine.Random.Range(0, 255);
            int joystickY = UnityEngine.Random.Range(0, 255);
            int potValue = UnityEngine.Random.Range(0, 1024);
            string buttonValue = UnityEngine.Random.value > 0.7f ? "P" : "S";

            // Construir JSON en el formato esperado
            string json = $"{{\"RFID\":\"{rfid}\",\"JOYSTICK\":\"{joystickX}-{joystickY}\",\"POT\":\"{potValue}\",\"BUTTON\":\"{buttonValue}\"}}";

            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();

            Debug.Log($"[ArduinoSimulator] 📤 Enviado JSON: {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ArduinoSimulator] ❌ Error al enviar datos: {ex.Message}");
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
