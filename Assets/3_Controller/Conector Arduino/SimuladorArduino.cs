using System;
using System.IO.Ports;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// SCRIPT DE SIMULACIÓN - Simula una placa Arduino para testing del ConectorArduino
/// 
/// Propósito: Emular el comportamiento de Arduino enviando mensajes por puerto serial
/// para verificar que ConectorArduino funciona correctamente.
/// 
/// Características:
/// - GUI automática para seleccionar puerto serial
/// - Control manual mediante botones para enviar datos
/// - Simula respuestas a comandos del ConectorArduino
/// - Monitorea cambios de estado desde ConectorArduino
///
/// Requisitos:
/// - Puerto serial virtual configurado (pair virtual)
/// - ConectorArduino escuchando en el mismo puerto
/// 
/// Observar en la consola de Unity los logs de [SimuladorArduino] para verificar
/// la comunicación y estado de la conexión.
/// </summary>
/// 
public class SimuladorArduino : MonoBehaviour
{
    private const int BAUD_RATE = 9600;
    private const string IDENTIFICATION_MESSAGE = "soy controles lili quest";

    // ========== PUERTO SERIAL ==========
    private SerialPort serialPort;
    private bool isConnected = false;
    private bool handshakeDone = false;
    private bool handshakeSent = false;
    private string connectedPort = "";

    // ========== DATOS ACTUALES DESDE CONECTOR ARDUINO ==========
    private ArduinoState estadoActual = ArduinoState.Inicializando;
    private bool estaConectado = false;
    private string rfidLeido = "";
    private int joystickX = 0;
    private int joystickY = 0;
    private string potLeido = "";
    private string botonLeido = "";
    private bool buttonPressPending = false;

    // ========== GUI ==========
    private string[] availablePorts = new string[0];
    private Vector2 scrollPosition = Vector2.zero;
    private Vector2 historyScrollPosition = Vector2.zero;
    private string statusMessage = "Iniciando...";
    private string rfidInputField = "77579063";
    private string joystickXInput = "512";
    private string joystickYInput = "512";
    private string potentiometerInput = "512";
    private string buttonInput = "S";
    private System.Collections.Generic.List<string> messageHistory = new System.Collections.Generic.List<string>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {   
        Debug.Log("[SimuladorArduino] 🤖 Simulador de Arduino iniciado");
        RefreshAvailablePorts();
    }

    private void Update()
    {
        // ========== ACTUALIZAR DATOS DE ConectorArduino ==========
        if (ConectorArduino.Instance != null)
        {
            estadoActual = ConectorArduino.Instance.CurrentState;
            estaConectado = ConectorArduino.Instance.IsConnected;

            SensorData datos = ConectorArduino.Instance.GetSensorData();
            if (datos != null)
            {
                rfidLeido = datos.RFID ?? "";
                joystickX = datos.JOYSTICK.X;
                joystickY = datos.JOYSTICK.Y;
                potLeido = datos.POT ?? "";
                botonLeido = datos.BUTTON ?? "";
            }
        }

        // ========== ESCANEAR PUERTOS PERIÓDICAMENTE ==========
        // El escaneo se realiza una sola vez en Start() o manualmente con el botón

        // ========== MONITOREAR RESPUESTAS DE UNITY ==========
        if (isConnected && serialPort != null && serialPort.BytesToRead > 0)
        {
            try
            {
                string line = serialPort.ReadLine().Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    Debug.Log($"[SimuladorArduino] 📩 Recibido desde Unity: \"{line}\"");
                    statusMessage = $"Comando recibido: {line}";

                    // Agregar al historial de mensajes
                    messageHistory.Add($"[{System.DateTime.Now:HH:mm:ss}] {line}");

                    // ========== CONFIRMACIÓN DE HANDSHAKE ==========
                    // Cuando ConectorArduino confirma el handshake respondiendo con "te encontre"
                    if (handshakeSent && line == "te encontre")
                    {
                        handshakeDone = true;
                        handshakeSent = false;
                        Debug.Log("[SimuladorArduino] ✓ ¡Handshake confirmado! ConectorArduino respondió 'te encontre'");
                        statusMessage = "✓ Handshake confirmado exitosamente";
                    }

                    // Responder a comandos
                    if (line == "enviar rfid")
                    {
                        Debug.Log("[SimuladorArduino] → Comando: ENVIAR RFID");
                    }
                    else if (line == "enviar datos de control")
                    {
                        Debug.Log("[SimuladorArduino] → Comando: ENVIAR DATOS DE CONTROL");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SimuladorArduino] ⚠️ Error al leer respuesta: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Interfaz gráfica (OnGUI es llamado automáticamente por Unity)
    /// </summary>
    private void OnGUI()
    {
        // ========== DISEÑO DE DOS COLUMNAS ==========
        float columnWidth = (Screen.width - 30) / 2;
        float columnHeight = Screen.height - 20;

        // ========== COLUMNA IZQUIERDA ==========
        GUILayout.BeginArea(new Rect(10, 10, columnWidth, columnHeight));
        GUILayout.Label("═══ SIMULADOR ARDUINO ═══", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });

        GUILayout.Space(10);

        // ========== SECCIÓN: CONEXIÓN ==========
        GUILayout.Label("─── CONEXIÓN ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

        if (isConnected)
        {
            GUILayout.Label($"✓ Conectado a: {connectedPort}", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
            GUILayout.Label($"✓ Handshake: {(handshakeDone ? "✓ Completado" : (handshakeSent ? "⏳ Esperando..." : "Pendiente"))}",
                           new GUIStyle(GUI.skin.label) { normal = { textColor = handshakeDone ? Color.green : (handshakeSent ? Color.yellow : Color.red) } });

            GUILayout.Space(5);

            // Botón para enviar handshake (solo si no está completado o esperando)
            if (!handshakeDone && !handshakeSent)
            {
                if (GUILayout.Button("🔐 Enviar Handshake", GUILayout.Height(30)))
                {
                    SendIdentificationMessage();
                    handshakeSent = true;
                    statusMessage = "Handshake enviado, esperando confirmación desde ConectorArduino...";
                }
            }
            else if (handshakeSent)
            {
                GUILayout.Label("⏳ Esperando respuesta 'te encontre' desde ConectorArduino...",
                               new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });

                if (GUILayout.Button("🔄 Reintentar Handshake", GUILayout.Height(30)))
                {
                    handshakeSent = false;
                    SendIdentificationMessage();
                    handshakeSent = true;
                    statusMessage = "Handshake reenviado, esperando confirmación...";
                }
            }

            if (GUILayout.Button("❌ Desconectar", GUILayout.Height(30)))
            {
                DisconnectFromPort();
            }
        }
        else
        {
            GUILayout.Label("Puertos disponibles:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            if (availablePorts.Length == 0)
            {
                GUILayout.Label("⚠️ No hay puertos disponibles", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

                foreach (string port in availablePorts)
                {
                    if (GUILayout.Button($"🔌 {port}", GUILayout.Height(30)))
                    {
                        ConnectToPort(port);
                    }
                }

                GUILayout.EndScrollView();
            }

            if (GUILayout.Button("🔄 Actualizar Puertos", GUILayout.Height(25)))
            {
                RefreshAvailablePorts();
                statusMessage = "Puertos actualizados";
            }
        }

        GUILayout.Space(10);

        // ========== SECCIÓN: ESTADO ==========
        GUILayout.Label("─── ConectorArduino ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        GUILayout.Label($"Estado: {estadoActual}");
        GUILayout.Label($"Conectado a Unity: {(estaConectado ? "✓ Sí" : "✗ No")}",
                       new GUIStyle(GUI.skin.label) { normal = { textColor = estaConectado ? Color.green : Color.red } });

        GUILayout.Space(10);

        // ========== SECCIÓN: DATOS LEÍDOS ==========
        GUILayout.Label("─── DATOS RECIBIDOS ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        GUILayout.Label($"RFID: {rfidLeido}");
        GUILayout.Label($"Joystick: X={joystickX}, Y={joystickY}");
        GUILayout.Label($"Potenciómetro: {potLeido}");
        GUILayout.Label($"Botón: {botonLeido}");

        GUILayout.Space(10);

        // ========== SECCIÓN: ENVÍO DE DATOS ==========
        if (handshakeDone)
        {
            GUILayout.Label("─── ENVÍO DE DATOS ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            // Envío de RFID
            GUILayout.Label("RFID a enviar:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            rfidInputField = GUILayout.TextField(rfidInputField, GUILayout.Height(25));

            if (GUILayout.Button($"📤 Enviar RFID: {rfidInputField}", GUILayout.Height(30)))
            {
                SendRFIDData(rfidInputField);
            }

            GUILayout.Space(10);
            GUILayout.Label("─── SENSORES CONTROLADOS ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            GUILayout.Label("Joystick X (0-1023):", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            joystickXInput = GUILayout.TextField(joystickXInput, GUILayout.Height(25));

            GUILayout.Label("Joystick Y (0-1023):", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            joystickYInput = GUILayout.TextField(joystickYInput, GUILayout.Height(25));

            GUILayout.Label("Potenciómetro (0-1023):", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            potentiometerInput = GUILayout.TextField(potentiometerInput, GUILayout.Height(25));

            GUILayout.Label("Botón (S=Sin presión, P=Pulsado):", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            buttonInput = GUILayout.TextField(buttonInput, 1, GUILayout.Height(25));

            if (GUILayout.Button("📤 Enviar Sensores Controlados", GUILayout.Height(30)))
            {
                SendControlledSensorData(rfidInputField, joystickXInput, joystickYInput, potentiometerInput, buttonInput);
            }

            GUILayout.Space(10);
            GUILayout.Label("─── SENSORES ALEATORIOS ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            if (GUILayout.Button("🎲 Enviar Sensores Aleatorios", GUILayout.Height(30)))
            {
                SendRandomSensorData();
            }

            if (GUILayout.Button(buttonPressPending ? "🔘 Pulsación simulada: P" : "🔘 Simular pulsación", GUILayout.Height(30)))
            {
                buttonPressPending = true;
                statusMessage = "Pulsación simulada armada para el próximo envío";
            }

            GUILayout.Space(5);

            // Solicitar cambios de estado usando API pública de ConectorArduino
            GUILayout.Label("Cambiar Estado:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 RFID", GUILayout.Height(30)))
            {
                ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
                statusMessage = "Solicitado cambio a EsperandoRFID";
            }
            if (GUILayout.Button("📊 Sensores", GUILayout.Height(30)))
            {
                ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
                statusMessage = "Solicitado cambio a LeyendoDatos";
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        // ========== SECCIÓN: ESTADO DE OPERACIÓN ==========
        GUILayout.Label("─── ÚLTIMA ACCIÓN ───", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        GUILayout.Label($"{statusMessage}");

        GUILayout.EndArea();

        // ========== COLUMNA DERECHA: HISTORIAL ==========
        GUILayout.BeginArea(new Rect(10 + columnWidth + 10, 10, columnWidth, columnHeight));
        GUILayout.Label("═══ Mensajes recibidos por serial ═══", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });

        GUILayout.Space(10);

        // Panel de historial scrollable
        historyScrollPosition = GUILayout.BeginScrollView(historyScrollPosition);

        if (messageHistory.Count == 0)
        {
            GUILayout.Label("Sin mensajes recibidos aún", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
        }
        else
        {
            foreach (string message in messageHistory)
            {
                GUILayout.Label(message, new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 10 });
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(5);
        if (GUILayout.Button("🗑️ Limpiar", GUILayout.Height(25)))
        {
            messageHistory.Clear();
            statusMessage = "Historial limpiado";
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Obtiene la lista de puertos seriales disponibles
    /// </summary>
    private void RefreshAvailablePorts()
    {
        try
        {
            availablePorts = SerialPort.GetPortNames();
            if (availablePorts.Length == 0)
            {
                Debug.LogWarning("[SimuladorArduino] ⚠️ No hay puertos seriales disponibles");
            }
            else
            {
                Debug.Log($"[SimuladorArduino] 📡 Puertos disponibles: {string.Join(", ", availablePorts)}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimuladorArduino] ❌ Error al escanear puertos: {ex.Message}");
        }
    }

    /// <summary>
    /// Conecta al puerto serial especificado
    /// </summary>
    private void ConnectToPort(string port)
    {
        if (isConnected)
        {
            Debug.LogWarning("[SimuladorArduino] ⚠️ Ya hay una conexión activa");
            return;
        }

        Debug.Log($"[SimuladorArduino] 🔌 Intentando conectar a puerto: {port}");
        statusMessage = $"Conectando a {port}...";

        try
        {
            serialPort = new SerialPort(port, BAUD_RATE);
            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;
            serialPort.NewLine = "\n";

            serialPort.Open();
            isConnected = true;
            connectedPort = port;
            Debug.Log($"[SimuladorArduino] ✓ Puerto {port} abierto exitosamente");
            statusMessage = $"Conectado a {port}. Haz clic en 'Enviar Handshake' para continuar";
        }
        catch (UnauthorizedAccessException ex)
        {
            isConnected = false;
            Debug.LogError($"[SimuladorArduino] ❌ ACCESO DENEGADO en {port}: {ex.Message}");
            statusMessage = $"Acceso denegado en {port}";
            Debug.LogError("[SimuladorArduino] → Soluciones: 1) Ejecuta Unity como Admin 2) Verifica que no hay otra app usando el puerto");
        }
        catch (Exception ex)
        {
            isConnected = false;
            Debug.LogError($"[SimuladorArduino] ❌ Error al abrir puerto {port}: {ex.Message}");
            statusMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Handshake manual - Llamado por botón en GUI
    /// Este método ya no es una corrutina automática, sino que se ejecuta bajo demanda del usuario
    /// </summary>

    /// <summary>
    /// Envía el mensaje de identificación al puerto
    /// </summary>
    private void SendIdentificationMessage()
    {
        if (!isConnected || serialPort == null)
            return;

        try
        {
            serialPort.WriteLine(IDENTIFICATION_MESSAGE);
            serialPort.BaseStream.Flush();
            Debug.Log($"[SimuladorArduino] 📤 Enviado: \"{IDENTIFICATION_MESSAGE}\"");
            statusMessage = "Mensaje de identificación enviado";
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimuladorArduino] ❌ Error al enviar identificación: {ex.Message}");
            statusMessage = $"Error al enviar: {ex.Message}";
        }
    }

    /// <summary>
    /// Desconecta del puerto serial
    /// </summary>
    private void DisconnectFromPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                serialPort.Dispose();
                Debug.Log("[SimuladorArduino] 👋 Puerto serial cerrado");
                statusMessage = "Desconectado";
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SimuladorArduino] ⚠️ Error al cerrar puerto: {ex.Message}");
            }
        }

        isConnected = false;
        handshakeDone = false;
        handshakeSent = false;
        connectedPort = "";
        serialPort = null;
    }

    /// <summary>
    /// Envía datos de RFID por puerto serial
    /// </summary>
    public void SendRFIDData(string rfidTag)
    {
        if (!isConnected || !handshakeDone)
        {
            Debug.LogWarning("[SimuladorArduino] ⚠️ No está conectado o handshake no completado");
            statusMessage = "Error: No conectado";
            return;
        }

        try
        {
            string buttonState = GetButtonStateForSend();
            string json = $"{{\"RFID\":\"{rfidTag}\",\"JOYSTICK\":\"0-0\",\"POT\":\"0\",\"BUTTON\":\"{buttonState}\"}}";
            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();
            Debug.Log($"[SimuladorArduino] 📤 RFID enviado: {json}");
            statusMessage = $"RFID enviado: {rfidTag}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimuladorArduino] ❌ Error al enviar RFID: {ex.Message}");
            statusMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Envía datos completos de sensores por puerto serial
    /// </summary>
    public void SendSensorData(string rfidTag, int joyX, int joyY, int potValue, string buttonState)
    {
        if (!isConnected || !handshakeDone)
        {
            Debug.LogWarning("[SimuladorArduino] ⚠️ No está conectado o handshake no completado");
            statusMessage = "Error: No conectado";
            return;
        }

        try
        {
            string simulatedButtonState = GetButtonStateForSend();
            string json = $"{{\"RFID\":\"{rfidTag}\",\"JOYSTICK\":\"{joyX}-{joyY}\",\"POT\":\"{potValue}\",\"BUTTON\":\"{simulatedButtonState}\"}}";
            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();
            Debug.Log($"[SimuladorArduino] 📤 Datos enviados: {json}");
            statusMessage = "Datos de sensores enviados";
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimuladorArduino] ❌ Error al enviar datos: {ex.Message}");
            statusMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Envía datos de sensores con valores controlados por el usuario
    /// Permite pruebas precisas de rangos específicos de sensores
    /// </summary>
    public void SendControlledSensorData(string rfidTag, string joyXStr, string joyYStr, string potStr, string buttonStr)
    {
        if (!isConnected || !handshakeDone)
        {
            Debug.LogWarning("[SimuladorArduino] ⚠️ No está conectado o handshake no completado");
            statusMessage = "Error: No conectado";
            return;
        }

        try
        {
            // Convertir y validar valores
            if (!int.TryParse(joyXStr, out int joyX) || joyX < 0 || joyX > 1023)
            {
                statusMessage = "Error: Joystick X debe estar entre 0-1023";
                Debug.LogWarning($"[SimuladorArduino] ⚠️ {statusMessage}");
                return;
            }

            if (!int.TryParse(joyYStr, out int joyY) || joyY < 0 || joyY > 1023)
            {
                statusMessage = "Error: Joystick Y debe estar entre 0-1023";
                Debug.LogWarning($"[SimuladorArduino] ⚠️ {statusMessage}");
                return;
            }

            if (!int.TryParse(potStr, out int pot) || pot < 0 || pot > 1023)
            {
                statusMessage = "Error: Potenciómetro debe estar entre 0-1023";
                Debug.LogWarning($"[SimuladorArduino] ⚠️ {statusMessage}");
                return;
            }

            // Validar botón (solo S o P permitidos)
            buttonStr = buttonStr.ToUpper();
            if (buttonStr != "S" && buttonStr != "P")
            {
                buttonStr = "S"; // Valor por defecto si es inválido
                Debug.LogWarning("[SimuladorArduino] ⚠️ Botón inválido, usando 'S' por defecto");
            }

            // Enviar datos
            string json = $"{{\"RFID\":\"{rfidTag}\",\"JOYSTICK\":\"{joyX}-{joyY}\",\"POT\":\"{pot}\",\"BUTTON\":\"{buttonStr}\"}}";
            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();
            Debug.Log($"[SimuladorArduino] 📤 Sensores controlados enviados: {json}");
            statusMessage = $"Sensores controlados: JX={joyX}, JY={joyY}, POT={pot}, BTN={buttonStr}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimuladorArduino] ❌ Error al enviar sensores controlados: {ex.Message}");
            statusMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Envía datos de sensores con valores completamente aleatorios
    /// Útil para probar cómo ConectorArduino lee y deserializa los datos
    /// </summary>
    public void SendRandomSensorData()
    {
        if (!isConnected || !handshakeDone)
        {
            Debug.LogWarning("[SimuladorArduino] ⚠️ No está conectado o handshake no completado");
            statusMessage = "Error: No conectado";
            return;
        }

        try
        {
            // Generar valores aleatorios
            int randomJoyX = UnityEngine.Random.Range(0, 1024);          // Joystick X
            int randomJoyY = UnityEngine.Random.Range(0, 1024);          // Joystick Y
            int randomPot = UnityEngine.Random.Range(0, 1024);          // Potenciómetro
            string randomButton = GetButtonStateForSend();  // Botón: S por defecto, P solo si se simuló una pulsación
            string rfidTag = rfidInputField;                 // Usa el RFID del input

            string json = $"{{\"RFID\":\"{rfidTag}\",\"JOYSTICK\":\"{randomJoyX}-{randomJoyY}\",\"POT\":\"{randomPot}\",\"BUTTON\":\"{randomButton}\"}}";
            serialPort.WriteLine(json);
            serialPort.BaseStream.Flush();
            Debug.Log($"[SimuladorArduino] 🎲 Datos aleatorios enviados: {json}");
            statusMessage = $"Sensores aleatorios: JX={randomJoyX}, JY={randomJoyY}, POT={randomPot}, BTN={randomButton}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimuladorArduino] ❌ Error al enviar datos aleatorios: {ex.Message}");
            statusMessage = $"Error: {ex.Message}";
        }
    }

    private void OnApplicationQuit()
    {
        DisconnectFromPort();
    }

    /// <summary>
    /// Devuelve P una sola vez si hay una pulsación simulada pendiente; en caso contrario devuelve S.
    /// </summary>
    private string GetButtonStateForSend()
    {
        if (buttonPressPending)
        {
            buttonPressPending = false;
            return "P";
        }

        return "S";
    }
}
#endif