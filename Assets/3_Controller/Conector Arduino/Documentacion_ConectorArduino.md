# � Documentación - ConectorArduino

## Descripción General

`ConectorArduino` es el componente encargado de gestionar toda la comunicación serial con el chip Arduino en el proyecto "Lili Quest". Se implementa como un **Singleton** que maneja:

- **Búsqueda automática** del puerto COM donde está conectado Arduino mediante handshake
- **Máquina de estados** que sincroniza el flujo de interacción entre Arduino y Unity
- **Lectura y deserialización** de datos de sensores (RFID, Joystick, Potenciómetro, Botón)
- **Envío de comandos** a Arduino para cambiar su modo de operación

### Características principales

✅ **Singleton automático** — Una única instancia accesible desde cualquier script  
✅ **Máquina de estados explícita** — 3 estados claros: Inicializando → EsperandoRFID → LeyendoDatos  
✅ **Datos tipados** — IntelliSense automático en el IDE  
✅ **API segura** — Acceso protegido a datos internos  
✅ **Reconexión automática** — Si Arduino se desconecta, reintenta continuamente  
✅ **Parseado automático** — El Joystick "120-95" se convierte en X=120, Y=95 (valores numéricos)

---

## Tipos de Datos

### ArduinoState

Define los tres estados posibles en los que se encuentra el sistema durante el flujo de interacción.

```csharp
public enum ArduinoState
{
    Inicializando = 0,  // Buscando puerto COM y estableciendo conexión
    EsperandoRFID = 1,  // Aguardando lectura de etiqueta RFID
    LeyendoDatos = 2    // Capturando datos de Joystick, Potenciómetro y Botón
}
```

| Estado | Descripción | Arduino Envía | Arduino Espera |
|--------|-------------|---------------|---|
| **Inicializando** | Búsqueda del puerto serial | `"soy controles lili quest"` | `"te encontre"` |
| **EsperandoRFID** | Sistema en reposo, aguardando RFID | Lectura de RFID | Comando para activar |
| **LeyendoDatos** | Sistema activo, capturando sensores | JSON con todos los datos | Comando para volver a espera |

---

### SensorData

Contenedor tipado para los datos actuales de los sensores. Se actualiza cada vez que Arduino envía nuevo JSON.

```csharp
public class SensorData
{
    public string RFID;          // Identificador de etiqueta RFID (ej: "TAG_001")
    public JoystickData JOYSTICK; // Coordenadas X e Y del joystick (estructura)
    public string POT;            // Valor del potenciómetro (ej: "512")
    public string BUTTON;         // Estado del botón: "P" (presionado) o "S" (sin presionar)
}
```

**Ejemplo de acceso:**
```csharp
SensorData datos = ConectorArduino.Instance.GetSensorData();
Debug.Log($"RFID: {datos.RFID}");
Debug.Log($"Joystick X: {datos.JOYSTICK.X}, Y: {datos.JOYSTICK.Y}");  // X e Y ya son int
Debug.Log($"Potenciómetro: {datos.POT}");
Debug.Log($"Botón: {datos.BUTTON}");

// Comparaciones numéricas directas sin necesidad de conversión
if (datos.JOYSTICK.X > 200)
{
    Debug.Log("Joystick desplazado a la derecha");
}

int joystickMagnitud = (int)Mathf.Sqrt(datos.JOYSTICK.X * datos.JOYSTICK.X + datos.JOYSTICK.Y * datos.JOYSTICK.Y);
```

---

### JoystickData

Estructura que parsamaticamente el dato del joystick que llega desde Arduino en formato `"x-y"` a valores numéricos.

```csharp
public class JoystickData
{
    public int X;  // Coordenada X (parseada automáticamente a entero)
    public int Y;  // Coordenada Y (parseada automáticamente a entero)
    
    // Constructor interno - convierte "120-95" en X=120, Y=95
    public JoystickData(string raw)
    {
        string[] parts = raw.Split('-');
        X = int.TryParse(parts.Length > 0 ? parts[0] : "0", out int parsedX) ? parsedX : 0;
        Y = int.TryParse(parts.Length > 1 ? parts[1] : "0", out int parsedY) ? parsedY : 0;
    }
}
```

**Nota:** Los valores X e Y se convierten automáticamente a enteros (`int`). No necesitas hacer parsing manual en otros scripts — puedes usar directamente los valores numéricos.

---

### Payload (Interno)

Clase temporal utilizada internamente para deserializar el JSON que llega desde Arduino. **No la uses directamente en otros scripts.**

```csharp
[Serializable]
public class Payload
{
    public string RFID;    // Desde Arduino: {"RFID":"TAG_001", ...}
    public string JOYSTICK;
    public string POT;
    public string BUTTON;
}
```

---

## API Pública

### Propiedades

| Propiedad | Acceso | Tipo | Descripción |
|-----------|--------|------|-------------|
| `Instance` | Lectura | ConectorArduino | Singleton — instancia única |
| `CurrentState` | Lectura | ArduinoState | Estado actual del sistema |
| `IsConnected` | Lectura | bool | `true` si puerto serial está abierto |
| `isSearching` | Lectura | bool | `true` mientras busca Arduino |
| `baudRate` | Lectura/Escritura | int | velocidad baudios (defecto: 9600) |
| `scanInterval` | Lectura/Escritura | float | segundos entre intentos de conexión (defecto: 3f) |

### Métodos Públicos

#### GetSensorData()

Retorna los datos actuales de sensores en un objeto `SensorData` tipado.

```csharp
public SensorData GetSensorData() => sensorData;
```

**Retorna:** `SensorData` con RFID, JOYSTICK, POT, BUTTON  
**Cuándo llamarlo:** Continuamente en `Update()` para leer valores actuales

**Ejemplo:**
```csharp
void Update()
{
    SensorData datos = ConectorArduino.Instance.GetSensorData();
    
    if (datos.BUTTON == "P")
    {
        // Botón presionado
    }
}
```

---

#### RequestState(ArduinoState newState)

Solicita un cambio de estado en Arduino. Automáticamente envía el comando correspondiente al firmware.

```csharp
public void RequestState(ArduinoState newState)
```

| Parámetro | Descripción |
|-----------|-------------|
| `newState` | Nuevo estado deseado |

**Comportamiento:**
- Si ya está en ese estado, hace nada (con warning en log)
- Si no hay conexión, retorna error
- Cambia `CurrentState` y envía comando a Arduino

| Nuevo Estado | Comando Enviado | Qué indica |
|--------------|-----------------|-----------|
| `EsperandoRFID` | `"enviar rfid"` | Volver a esperar etiqueta RFID |
| `LeyendoDatos` | `"enviar datos de control"` | Comenzar a enviar datos de sensores |

**Ejemplo:**
```csharp
public void VolverAlMenu()
{
    ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
}

public void IniciarJuego()
{
    ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
}
```

---

## Ciclo de Vida y Estados

### Diagrama de Flujo

```
┌─────────────────────────────────────────────┐
│       INICIALIZANDO                         │
│  (Buscando puerto COM)                      │
│  Arduino envía: "soy controles lili quest"  │
│  Unity responde: "te encontre"              │
│         ↓ (Auto)                            │
└─────────────┬──────────────────────────┘
              │
              ↓
    ┌──────────────────────────────────┐
    │   ESPERANDO RFID                 │
    │  (Sistema en reposo)             │
    │  Arduino envía: Datos RFID       │
    │  Unity lee: datos.RFID           │
    │         ↓ (manual RequestState)  │
    └──────────┬──────────┬────────────┘
               │          │
        Procesamiento   Timeout
               │
               ↓
    ┌──────────────────────────────────┐
    │   LEYENDO DATOS                  │
    │  (Sistema activo)                │
    │  Arduino envía: All sensors      │
    │  Unity lee: JOYSTICK, POT, BTN   │
    │         ↓ (manual RequestState)  │
    └──────────┬──────────────────────┘
               │
        RequestState(EsperandoRFID)
               │
               ↓
        ESPERANDO RFID
```

### Auto-Inicialización

Al ejecutar Unity:
1. **Awake()** → Instancia el Singleton
2. **Start()** → Inicia `ConnectionLoop()`
3. **ConnectionLoop** → Ejecuta `SearchAndConnectArduino()` automáticamente
4. Arduino se encuentra → Pasa a `EsperandoRFID`
5. Se inicia `ReadDataLoop()` → Lee datos continuamente

---

## Ejemplos de Uso

### Ejemplo 1: Leer datos continuamente
```csharp
void Update()
{
    // Solo si Arduino está en modo lectura
    if (ConectorArduino.Instance.CurrentState != ArduinoState.LeyendoDatos)
        return;

    SensorData datos = ConectorArduino.Instance.GetSensorData();
    
    // Usar datos
    if (datos.BUTTON == "P")  // Presionado
        Debug.Log("¡Botón presionado!");
    
    Debug.Log($"Posición Joystick: X={datos.JOYSTICK.X}, Y={datos.JOYSTICK.Y}");
}
```

### Ejemplo 2: Transición de estados
```csharp
public class GestorFlujo : MonoBehaviour
{
    private void Start()
    {
        // Arduino comienza en Inicializando automáticamente
        // Cuando detecta RFID, debe cambiar a LeyendoDatos
    }

    public void RFIDDetected(string tagID)
    {
        Debug.Log($"RFID detectado: {tagID}");
        // Cambiar a modo lectura de sensores
        ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
    }

    public void VolverAlReposoDeRFID()
    {
        Debug.Log("Volviendo a esperar RFID...");
        ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
    }
}
```

### Ejemplo 3: Verificación de estado
```csharp
void OnGUI()
{
    GUILayout.Label($"Estado: {ConectorArduino.Instance.CurrentState}");
    GUILayout.Label($"Conectado: {ConectorArduino.Instance.IsConnected}");
    GUILayout.Label($"Buscando: {ConectorArduino.Instance.isSearching}");
}
```

### Ejemplo 1: Detectar RFID en EsperandoRFID

```csharp
public class DetectorRFID : MonoBehaviour
{
    private void Update()
    {
        // Solo procesar si estamos esperando RFID
        if (ConectorArduino.Instance.CurrentState != ArduinoState.EsperandoRFID)
            return;

        SensorData datos = ConectorArduino.Instance.GetSensorData();

        // Verificar si hay nueva lectura de RFID
        if (!string.IsNullOrEmpty(datos.RFID) && datos.RFID != ultmoRFID)
        {
            ultmoRFID = datos.RFID;
            Debug.Log($"✓ RFID Detectado: {datos.RFID}");
            
            // Cambiar a lectura de sensores
            ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
        }
    }
    
    private string ultmoRFID = "";
}
```

---

### Ejemplo 2: Leer Sensores en LeyendoDatos

```csharp
public class ControladorJugador : MonoBehaviour
{
    private void Update()
    {
        // Solo si estamos capturando datos
        if (ConectorArduino.Instance.CurrentState != ArduinoState.LeyendoDatos)
            return;

        SensorData datos = ConectorArduino.Instance.GetSensorData();

        // Usar datos del joystick para movimiento
        if (datos.JOYSTICK != null)
        {
            float xValue = float.Parse(datos.JOYSTICK.X);
            float yValue = float.Parse(datos.JOYSTICK.Y);
            MoverPersonaje(xValue, yValue);
        }

        // Detectar botón presionado
        if (datos.BUTTON == "P")
        {
            RealizarAccion();
        }

        // Usar potenciómetro (volumen, brillo, etc.)
        if (!string.IsNullOrEmpty(datos.POT))
        {
            float potValue = float.Parse(datos.POT);
            AjustarVolumen(potValue);
        }
    }

    private void MoverPersonaje(float x, float y) { /* ... */ }
    private void RealizarAccion() { /* ... */ }
    private void AjustarVolumen(float pot) { /* ... */ }
}
```

---

### Ejemplo 3: Cambio de Estados Explícito

```csharp
public class GestorFlujo : MonoBehaviour
{
    public void IniciarNivel()
    {
        Debug.Log("Iniciando nivel...");
        ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
    }

    public void VolverAlMenuPrincipal()
    {
        Debug.Log("Volviendo al menú...");
        ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
        // SceneManager.LoadScene("MenuPrincipal");
    }
}
```

---

### Ejemplo 4: Debug y Monitoreo

```csharp
void OnGUI()
{
    GUILayout.BeginArea(new Rect(10, 10, 300, 150));
    GUILayout.Label("=== Estado Arduino ===");
    GUILayout.Label($"Estado: {ConectorArduino.Instance.CurrentState}");
    GUILayout.Label($"Conectado: {ConectorArduino.Instance.IsConnected}");
    GUILayout.Label($"Buscando: {ConectorArduino.Instance.isSearching}");
    
    if (ConectorArduino.Instance.IsConnected)
    {
        SensorData d = ConectorArduino.Instance.GetSensorData();
        GUILayout.Label($"RFID: {d.RFID}");
        GUILayout.Label($"Joystick: {d.JOYSTICK}");
        GUILayout.Label($"POT: {d.POT}");
        GUILayout.Label($"BTN: {d.BUTTON}");
    }
    GUILayout.EndArea();
}
```

---

## Arquitectura Interna

### Responsabilidades por Capa

| Capa | Responsabilidad | Métodos |
|------|-----------------|---------|
| **Conexión** | Buscar puerto COM, handshake | `SearchAndConnectArduino()` |
| **Lectura** | Leer datos del puerto serial | `ReadDataLoop()` |
| **Procesamiento** | Deserializar JSON, actualizar datos | `ProcessIncomingData()` |
| **Control** | Máquina de estados, envío de comandos | `RequestState()`, `SendCommandToArduino()` |
| **Ciclo de Vida** | Reconexión automática, limpieza | `ConnectionLoop()`, `TryCloseSerial()` |

---

## Especificaciones Técnicas

### Protocolo de Comunicación

**Handshake Inicial:**
1. Arduino envía: `"Museo Digital"`
2. Unity responde: `"Te encontre"`
3. Se establece conexión

**Transferencia de Datos:**
- Arduino envía JSON cada frame (configurable)
- Formato: `{"RFID":"TAG_001","JOYSTICK":"120-95","POT":"512","BUTTON":"P"}`
- Cada línea termina con `\n`

**Comandos de Unity a Arduino:**
- `"enviar rfid"` — Volver a estado inicial (esperando RFID)
- `"enviar datos de control"` — Comenzar a enviar datos de sensores

### Configuración

| Parámetro | Valor por Defecto | Rango Recomendado | Descripción |
|-----------|-------------------|-------------------|-------------|
| `baudRate` | 9600 | 9600-115200 | Velocidad de comunicación serial |
| `scanInterval` | 3f | 1f-5f | Segundos entre intentos de conexión |

Ambos son editables en el Inspector de Unity.

---

## Protocolo Arduino (Recomendaciones)

El firmware de Arduino debe estar atento a los comandos que envía Unity:

```cpp
// Variables globales
const char RESET_CMD[] = "enviar rfid";
const char START_CMD[] = "enviar datos de control";

enum ArduinoMode { RFID_WAITING = 0, SENSOR_READING = 1 };
ArduinoMode currentMode = RFID_WAITING;

void setup() {
    Serial.begin(9600);
    Serial.println("soy controles lili quest");  // Identificación
    delay(100);
}

void loop() {
    // Procesar comandos de Unity
    if (Serial.available()) {
        String cmd = Serial.readStringUntil('\n');
        
        if (cmd == RESET_CMD) {
            currentMode = RFID_WAITING;
            Serial.println("soy controles lili quest");  // Re-identificación
        }
        else if (cmd == START_CMD) {
            currentMode = SENSOR_READING;
        }
    }
    
    // Enviar datos según el modo
    if (currentMode == RFID_WAITING) {
        leerRFID();
    } 
    else if (currentMode == SENSOR_READING) {
        leerTodosSensores();
    }
}

void leerRFID() {
    // Lógica para leer solo RFID
    // Enviar JSON: {"RFID":"TAG_001","JOYSTICK":"0-0","POT":"0","BUTTON":"S"}
}

void leerTodosSensores() {
    // Lógica para leer todos los sensores
    // Enviar JSON con datos actuales
    String json = buildJSON(rfidValue, xValue, yValue, potValue, btnValue);
    Serial.println(json);
}
```

---

## Notas Técnicas

- **Singleton:** El componente se autocrea en la primera escena y persiste entre cambios de escena (`DontDestroyOnLoad`)
- **Thread-Safety:** Todas las corrutinas se ejecutan en el main thread de Unity, no hay acceso simultáneo
- **Strings:**  Los valores se mantienen como strings para máxima compatibilidad con Arduino y serialización JSON
- **Reconexión:** Si Arduino se desconecta repentinamente, el loop de conexión lo detecta automáticamente y reintenta
- **Limpieza:** Al salir de la aplicación, se envía `RESET_CONNECTION_MSG` a Arduino antes de cerrar el puerto

---

## Troubleshooting

### Arduino no se encuentra

**Síntomas:** Log muestra `"[Arduino] No encontrado. Reintentando..."`

**Causas posibles:**
1. Puerto COM incorrecto o desconectado
2. Firmware Arduino no envía `"soy controles lili quest"`
3. Baudrate diferente en Arduino (defecto esperado: 9600)
4. Cable USB con problemas

**Soluciones:**
- Verificar puerto COM en Device Manager (Windows)
- Probar el código de prueba en Arduino IDE directamente
- Cambiar `baudRate` en el Inspector si Arduino usa velocidad diferente

---

### Datos inconsistentes o null

**Síntomas:** `datos.RFID` está siempre null o vacío

**Causas posibles:**
1. Arduino aún está en estado `Inicializando` (conexión reciente)
2. Formato JSON del Arduino no coincide con `Payload`
3. Arduino no está en modo correcto

**Soluciones:**
- Esperar a que `CurrentState` cambie a `EsperandoRFID`
- Verificar que Arduino envía JSON con claves exactas: `RFID`, `JOYSTICK`, `POT`, `BUTTON`
- Validar JSON con formato exacto: `{"RFID":"TAG_001",...}`

---

### Desconexiones frecuentes

**Síntomas:** Arduino se reconecta continuamente

**Causas posibles:**
1. Problemas con el cable USB
2. Alimentación insuficiente a Arduino
3. Interferencias electromagnéticas

**Soluciones:**
- Usar cable USB de calidad y bien conectado
- Probar con fuente de alimentación externa
- Alejar de fuentes de ruido electromagnético

---

## Recursos Relacionados

- **ControladorFlujoEjemplo.cs** — Ejemplos de integración en otros controladores
- **ConectorArduino.cs** — Código fuente del componente

---

Generated with 💻 for Lili Quest Project
