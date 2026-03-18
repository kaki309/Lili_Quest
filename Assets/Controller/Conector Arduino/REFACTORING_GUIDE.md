# 📋 Guía de Refactorización - ConectorArduino

## Cambios Principales

### 1. **Arquitectura de Máquina de Estados**
```csharp
public enum ArduinoState
{
    Inicializando,      // Buscando Puerto → Arduino
    EsperandoRFID,      // Esperando lectura de RFID
    LeyendoDatos        // Capturando Joystick/POT/Botón
}
```

**Por qué:**
- Representación explícita del flujo del sistema
- Otros controladores saben exactamente en qué etapa está el proceso
- Evita lógica condicional esparcida por el código

---

### 2. **Tipos de Datos Tipados**
#### Antes:
```csharp
private Dictionary<string, string> internalData;
// Acceso: internalData["JOYSTICK"] → string sin estructura
```

#### Después:
```csharp
private SensorData sensorData;

public class SensorData
{
    public string RFID;
    public JoystickData JOYSTICK;  // Estructura específica
    public string POT;
    public string BUTTON;
}

public class JoystickData
{
    public string X;  // Parsed de "x-y"
    public string Y;
}
```

**Por qué:**
- **IntelliSense mejorado:** El IDE sugiere propiedades automáticamente
- **Seguridad de tipos:** Errores en compile-time en lugar de runtime
- **Parseado automático:** JoystickData convierte "120-95" en X="120", Y="95"
- **Debugging:** Valores estructurados son más legibles

---

### 3. **API Pública Clara**

#### Para LEER datos:
```csharp
// ❌ Antiguo (inseguro)
var rawJoystick = ConectorArduino.Instance.Data["JOYSTICK"];

// ✅ Nuevo (seguro)
SensorData datos = ConectorArduino.Instance.GetSensorData();
Debug.Log($"Joystick X: {datos.JOYSTICK.X}");
Debug.Log($"RFID: {datos.RFID}");
Debug.Log($"Botón: {datos.BUTTON}");
```

#### Para CAMBIAR estado:
```csharp
// ❌ Antiguo (no hay forma de hacerlo)
// No hay manera de solicitar cambio de estado desde otros controladores

// ✅ Nuevo
public void RequestState(ArduinoState newState)
{
    // Automáticamente envía comandos a Arduino
}
```

**Ejemplo de uso:**
```csharp
public class ControladorJuego : MonoBehaviour
{
    public void VolverAlMenu()
    {
        // Avisar a Arduino que vuelva a esperar RFID
        ConectorArduino.Instance.RequestState(ArduinoState.EsperandoRFID);
    }

    public void IniciarJuego()
    {
        // Indicar que queremos leer sensores
        ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
    }
}
```

---

### 4. **Separación de Responsabilidades**

El código ahora está organizado en 4 capas:

| Capa | Responsabilidad | Métodos |
|------|-----------------|---------|
| **Conexión** | Buscar Arduino, handshake, reconectar | `SearchAndConnectArduino()` |
| **Lectura** | Leer bytes del puerto serial | `ReadDataLoop()` |
| **Procesamiento** | Deserializar JSON, actualizar datos | `ProcessIncomingData()` |
| **Control** | Máquina de estados, cambios | `RequestState()`, `SendCommandToArduino()` |

**Por qué:**
- Cada método tiene una única responsabilidad
- Fácil de testear (si fuera necesario)
- Cambios futuros no afectan otras capas

---

### 5. **Propiedades de Acceso Público**

```csharp
// Estado actual
public ArduinoState CurrentState => currentState;

// Datos actuales (tipados y seguros)
public SensorData GetSensorData() => sensorData;

// Estado de conexión
public bool IsConnected => serial != null && serial.IsOpen;

// Debug: si está buscando puerto
public bool isSearching { get; private set; }
```

**Ventaja:** Lista completa de propiedades públicas en una mirada, sin acceso a detalles internos.

---

## 📝 Ejemplos de Uso Desde Otros Controladores

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

---

## 🔧 Cambios en Arduino (recomendados)

El firmware de Arduino debería estar atento a los comandos que Unity envía:

```cpp
void loop() {
    if (Serial.available()) {
        String cmd = Serial.readStringUntil('\n');
        
        if (cmd == "Reset Connection") {
            setMode(RFID_WAITING);  // Volver a esperar RFID
        }
        else if (cmd == "Start Reading") {
            setMode(SENSOR_READING);  // Empezar a leer sensores
        }
    }
    
    // Enviar datos según el modo actual
    if (currentMode == RFID_WAITING) {
        sendRFIDData();
    }
    else if (currentMode == SENSOR_READING) {
        sendSensorData();
    }
}
```

---

## 📊 Mapa de Estados

```
┌─────────────────────────────────────────┐
│       INICIALIZANDO                     │
│  (Buscando puerto COM)                  │
│  ↓ (Auto al conectar)                   │
└─────────────┬──────────────────────────┘
              │
              ↓
    ┌─────────────────────────────┐
    │   ESPERANDO RFID            │
    │  (Aguardando etiqueta)      │
    │  ← RequestState()            │
    │  ↓ (Al detectar RFID)        │
    └──────────┬────────────────┬─┘
               │                │
         Estado Manual     ┌─────┴────────┐
                           ↓
              ┌──────────────────────────┐
              │   LEYENDO DATOS          │
              │  (Joystick, POT, BTN)    │
              │  ← RequestState()        │
              │  ↓ (Volver a RFID)       │
              └──────┬───────────────────┘
                     │
       RequestState(EsperandoRFID)
                     │
                     ↓
              ESPERANDO RFID
```

---

## ✅ Ventajas de la Refactorización

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Tipado** | Diccionario de strings | Clases tipadas con IntelliSense |
| **Estados** | Lógica implícita | Máquina de estados explícita |
| **API** | Acceso al diccionario interno | Interface clara con métodos |
| **Debugging** | Escribir llaves manualmente | Propiedades autocompletas |
| **Cambio de modo** | No posible | `RequestState()` |
| **Mensajes** | Acoplados | Constantes centralizadas |
| **Organización** | Líneal | Por capas de responsabilidad |

---

## 🚀 Próximas Mejoras (Futuro)

1. **Sistema de eventos:**
   ```csharp
   public static event System.Action<ArduinoState> OnStateChanged;
   public static event System.Action<SensorData> OnDataUpdated;
   ```

2. **Validación de datos:**
   ```csharp
   public bool IsValidSensorData() 
   {
       return !string.IsNullOrEmpty(sensorData.RFID);
   }
   ```

3. **Historial de sensores:**
   ```csharp
   private Queue<SensorData> dataHistory;
   ```

4. **Configuración en Inspector:**
   ```csharp
   [SerializeField] private ArduinoState startingState = ArduinoState.Inicializando;
   ```

---

Generated with ❤️ for clean architecture
