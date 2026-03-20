using UnityEngine;

/// <summary>
/// EJEMPLO: Controlador de flujo del sistema que interactúa con ConectorArduino.
/// Demuestra cómo cambiar estados y leer datos desde otros scripts.
/// </summary>
public class ControladorFlujoEjemplo : MonoBehaviour
{
    [SerializeField] private float tiempoEsperaEntreLecturas = 0.1f;

    private void Start()
    {
        Debug.Log("[ControladorFlujo] Sistema iniciado");
        // Arduino comienza en Inicializando automáticamente
    }

    private void Update()
    {
        // Ejemplo 1: Leer estado actual
        if (ConectorArduino.Instance.CurrentState == ArduinoState.EsperandoRFID)
        {
            DetectarRFID();
        }
        else if (ConectorArduino.Instance.CurrentState == ArduinoState.LeyendoDatos)
        {
            LeerDatos();
        }
    }

    // ============================================================
    // EJEMPLO 1: DETECCIÓN DE RFID (Esperando RFID)
    // ============================================================

    private void DetectarRFID()
    {
        SensorData datos = ConectorArduino.Instance.GetSensorData();

        // Verificar si hay lectura de RFID
        if (datos.RFID != null && datos.RFID != "")
        {
            Debug.Log($"[ControladorFlujo] ✓ RFID Detectado: {datos.RFID}");

            // Procesar según el RFID
            ProcesarRFID(datos.RFID);

            // Cambiar a estado de lectura de sensores
            ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
        }
    }

    private void ProcesarRFID(string rfidTag)
    {
        // Aquí hay lógica para determinar qué hacer según el RFID
        // Ejemplo: si es "TAG_001", cargar nivel 1; si es "TAG_002", etc.
        
        switch (rfidTag)
        {
            case "TAG_001":
                Debug.Log("Iniciando Nivel 1...");
                break;
            case "TAG_002":
                Debug.Log("Iniciando Nivel 2...");
                break;
            default:
                Debug.Log($"RFID desconocido: {rfidTag}");
                break;
        }
    }

    // ============================================================
    // EJEMPLO 2: LECTURA DE SENSORES (Leyendo Datos)
    // ============================================================

    private void LeerDatos()
    {
        SensorData datos = ConectorArduino.Instance.GetSensorData();

        // Ejemplo: Usar datos del joystick
        if (datos.JOYSTICK != null)
        {
            // Acceder a las coordenadas parseadas
            string x = datos.JOYSTICK.X;
            string y = datos.JOYSTICK.Y;
            
            // Aquí controlamos el movimiento del jugador, por ejemplo
            // MoverPersonaje(x, y);
        }

        // Ejemplo: Usar botón
        if (datos.BUTTON == "P")  // Presionado
        {
            // Realizar acción
            RealizarAccion();
        }

        // Ejemplo: Usar potenciómetro (volumen, brillo, etc.)
        if (datos.POT != null)
        {
            // AjustarVolumen(datos.POT);
        }
    }

    private void MoverPersonaje(string x, string y)
    {
        // Lógica de movimiento aquí
        // Convierte strings a float si lo necesitas:
        // float xValue = float.Parse(x);
        // float yValue = float.Parse(y);
    }

    private void RealizarAccion()
    {
        Debug.Log("[ControladorFlujo] Acción ejecutada por botón");
        // Lógica de acción aquí
    }

    // ============================================================
    // EJEMPLO 3: VOLVER AL INICIO (Cambiar Estado)
    // ============================================================

    /// <summary>
    /// Método público que otros scripts pueden llamar para reiniciar
    /// Ejemplo: VolverAlMenu().
    /// </summary>
    public void VolverAlMenuPrincipal()
    {
        Debug.Log("[ControladorFlujo] Volviendo al menú principal...");

        // Cambiar Arduino a estado de espera de RFID
        ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);

        // Hacer el cambio de escena, limpiar datos, etc.
        // SceneManager.LoadScene("MenuPrincipal");
    }

    // ============================================================
    // EJEMPLO 4: VERIFICAR ESTADO (Para debugging)
    // ============================================================

    private void DebugPrintStatus()
    {
        ArduinoState estado = ConectorArduino.Instance.CurrentState;
        bool conectado = ConectorArduino.Instance.IsConnected;
        bool buscando = ConectorArduino.Instance.isSearching;

        Debug.Log("[Estado Arduino]\n" +
                  $"Estado Actual: {estado}\n" +
                  $"Conectado: {conectado}\n" +
                  $"Buscando: {buscando}");
    }
}
