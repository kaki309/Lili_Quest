<div align="center">

# 🔍 LiliQuest

### Sistema Multimedia Interactivo para la Difusión Digital del Silbato en Forma de Perro de la Cultura Quimbaya

**Museo Etnográfico y Arqueológico Lili — Universidad Autónoma de Occidente**

---

![Unity](https://img.shields.io/badge/Unity-2022.3-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Arduino](https://img.shields.io/badge/Arduino-00979D?style=for-the-badge&logo=arduino&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows)

</div>

---

## 📋 Descripción del Sistema

**LiliQuest** es un Sistema Multimedia Interactivo (SMI) diseñado para la difusión digital del *Silbato en forma de perro de la Cultura Quimbaya*, pieza arqueológica alojada en el Museo Etnográfico y Arqueológico Lili de la Universidad Autónoma de Occidente.

El sistema combina una interfaz gráfica desarrollada en Unity con una interfaz física construida sobre Arduino, generando una experiencia gamificada, educativa e inmersiva para los visitantes del museo. A través de LiliQuest, el usuario puede explorar un modelo 3D de la pieza arqueológica, recorrer una secuencia narrativa sobre su historia e interactuar con trivias que le permiten «reconstruir» la pieza digitalmente, reforzando el mensaje sobre la fragilidad e importancia del patrimonio cultural.

### Flujo de la experiencia

```
Inicio / Detección de controles
        ↓
Lectura RFID de la réplica física
        ↓
Visor 3D — Bienvenida con asistente virtual Laia
        ↓
Fractura narrativa de la pieza (mecánica intencional)
        ↓
Secuencia narrativa — 3 temáticas históricas
        ↓
Trivia interactiva × 3 (reconstrucción por fragmentos)
        ↓
Visor 3D libre — Pieza reconstruida
```

---

## 👥 Equipo de Desarrollo

| Nombre | Código |
|---|---|
| Andrés Gabriel Fernández Romero | 2225751 |
| Nicolas Díaz Guerrero | 2225275 |
| María Paula Becerra Henao | 2226053 |
| David Santiago Roa | 2215926 |
| Boris Alejandro Garces Hoyos | 2225026 |

**Asignatura:** Diseño Multimedia 2 — Ingeniería Multimedia  
**Facultad:** Ingeniería y Ciencias Básicas — Universidad Autónoma de Occidente

---

## 🛠️ Tecnologías Utilizadas

### Motor y Lenguaje Principal

| Tecnología | Rol en el sistema |
|---|---|
| **Unity** | Motor principal del sistema. Gestiona la interfaz gráfica, la visualización de modelos 3D, el sistema de escenas, las físicas y la lógica de la experiencia interactiva. |
| **C#** | Lenguaje de scripting dentro de Unity. Implementa toda la lógica del sistema bajo principios de Programación Orientada a Objetos (POO), alineada con el patrón MVC. |

### Hardware e Interfaz Física

| Tecnología | Rol en el sistema |
|---|---|
| **Arduino (microcontrolador)** | Captura los datos de los controles físicos del usuario y los transmite al sistema Unity mediante comunicación serial (puerto USB). |
| **Arduino IDE** | Entorno de desarrollo oficial de Arduino. Permite escribir, compilar y cargar el programa en el microcontrolador usando un lenguaje derivado de C++. |
| **Joystick analógico** | Control principal de navegación en menús y manipulación del modelo 3D. |
| **Potenciómetro** | Control de zoom en el visor 3D (giro a la derecha = acercamiento; giro a la izquierda = alejamiento). |
| **Botón pulsador** | Confirmación de selecciones e interacciones dentro de la experiencia. |
| **Lector RFID** | Identificación de la réplica física del silbato mediante tarjetas RFID, sin contacto directo. |

### Almacenamiento de Datos

| Tecnología | Rol en el sistema |
|---|---|
| **Sistema de archivos local (JSON + carpetas)** | Estructura y gestiona el contenido multimedia del sistema (modelo 3D, imágenes, secuencia narrativa, datos serializados). Separa los datos (Modelo) de la interfaz (Vista), según el patrón MVC. |

---

## 🏛️ Arquitectura del Sistema

### Patrón Arquitectónico: MVC (Modelo – Vista – Controlador)

LiliQuest implementa el patrón **Modelo-Vista-Controlador (MVC)**, que permite separar claramente la gestión de datos, la interfaz visual y la lógica de control. Esta separación facilita el desarrollo, el mantenimiento y la escalabilidad del sistema, especialmente en aplicaciones interactivas que integran hardware externo.

```
┌─────────────────────────────────────────────────────────┐
│                    PATRÓN MVC                           │
│                                                         │
│  ┌───────────────┐    ┌───────────────┐    ┌──────────┐ │
│  │     VISTA     │    │  CONTROLADOR  │    │  MODELO  │ │
│  │               │    │               │    │          │ │
│  │ · Interfaz    │◄──►│ · Game        │◄──►│ · JSON   │ │
│  │   Gráfica     │    │   Controller  │    │   Files  │ │
│  │   (Unity UI)  │    │ · Conector    │    │ · Local  │ │
│  │               │    │   Arduino     │    │   Storage│ │
│  │ · Interfaz    │    │ · Controlador │    │ · Asset  │ │
│  │   Física      │    │   3D          │    │   Refs   │ │
│  │   (Arduino)   │    │ · Controlador │    │          │ │
│  │               │    │   Narrativa   │    │          │ │
│  │               │    │ · Controlador │    │          │ │
│  │               │    │   Asistente   │    │          │ │
│  └───────────────┘    └───────────────┘    └──────────┘ │
└─────────────────────────────────────────────────────────┘
```

**Mapeo de componentes:**

- **Vista** → Interfaz gráfica (pantallas Unity) + Interfaz física (joystick, potenciómetro, botón, lector RFID, réplica 3D impresa).
- **Controlador** → Game Controller: gestiona la lógica interna, recibe y procesa señales del Arduino, actualiza el Modelo y la Vista en tiempo real.
- **Modelo** → Almacenamiento local: sistema de carpetas + archivos JSON que estructuran el contenido multimedia y lo entregan al Controlador cuando se solicita.

---

### Estructura del Proyecto (coherencia con MVC)

```
LiliQuest/
│
├── Assets/
│   │
│   ├── 📁 Model/                          ← MODELO
│   │   ├── Data/
│   │   │   ├── content_registry.json      # Registro serializado de contenidos
│   │   │   └── narrative_sequence.json    # Secuencia narrativa de las 3 temáticas
│   │   ├── Media/
│   │   │   ├── Images/                    # Fotografías e imágenes del sistema
│   │   │   └── 3DModels/                  # Modelo 3D del silbato (.fbx / .obj)
│   │   └── Scripts/
│   │       ├── DataController.cs          # Administra el acceso al contenido
│   │       └── ContentSerializer.cs       # Mantiene referencias y rutas del contenido
│   │
│   ├── 📁 View/                           ← VISTA
│   │   ├── Scenes/                        # Escenas de Unity (Inicio, Visor3D, Narrativa, Trivia)
│   │   ├── UI/
│   │   │   ├── Prefabs/                   # Prefabs de elementos de interfaz reutilizables
│   │   │   └── Sprites/                   # Recursos visuales (íconos, fondos, botones)
│   │   └── Scripts/
│   │       ├── UIElementManager.cs        # Gestiona referencias y modificaciones de UI en runtime
│   │       └── SceneLauncher.cs           # Controla la carga y transición entre escenas
│   │
│   ├── 📁 Controller/                     ← CONTROLADOR
│   │   └── Scripts/
│   │       ├── FlowController.cs          # Componente central: gestiona los estados del sistema
│   │       ├── ArduinoConnector.cs        # Lee datos del puerto serial y los traduce a Unity
│   │       ├── Model3DController.cs       # Gestiona la manipulación del modelo 3D
│   │       ├── NarrativeController.cs     # Gestiona el flujo de la secuencia histórica
│   │       └── AssistantController.cs     # Gestiona diálogos e imagen del asistente Laia
│   │
│   └── 📁 Arduino/                        ← FIRMWARE INTERFAZ FÍSICA
│       └── LiliQuest_Controls/
│           └── LiliQuest_Controls.ino     # Sketch Arduino: captura y envío serial de controles
│
├── ProjectSettings/                       # Configuración del proyecto Unity
├── Packages/                              # Dependencias del proyecto
└── README.md
```

> **Coherencia arquitectónica:** La estructura de carpetas refleja directamente los tres componentes del patrón MVC. Cada script pertenece inequívocamente a una capa (Model, View o Controller), lo que facilita la navegación del código, el trabajo en equipo y la escalabilidad futura del sistema.

---

## 🚀 Instrucciones de Ejecución

### Requisitos previos

- Sistema operativo **Windows** (10 u 11)
- Al menos un **puerto USB** disponible
- **Placa de Controles LiliQuest** (hardware físico del proyecto)
- **Réplica física** del silbato en forma de perro (con tarjeta RFID integrada)

### Pasos para ejecutar el demo

1. Acceder a la carpeta de descarga del demo:

   > 📁 [**Descargar Demo LiliQuest**](https://uao-my.sharepoint.com/:f:/g/personal/andres_gab_fernandez_uao_edu_co/IgBh_nMCRKanR5ER8VB31RloAamOZymdZyvRCdQNP7dvjXg?e=H24pqW)
   
   *(Enlace público — no requiere cuenta institucional)*

2. Descargar el archivo `LiliQuest.rar` desde la carpeta.

3. Hacer clic derecho sobre el archivo descargado y seleccionar **"Extraer aquí"**.

4. Conectar la **Placa de Controles LiliQuest** al computador mediante el cable USB.

5. Abrir la carpeta extraída, localizar el archivo **`LiliQuest.exe`** y hacer doble clic para ejecutar.

   > ⚠️ **Nota:** Si Windows muestra una advertencia de seguridad (SmartScreen), seleccionar **"Más información"** → **"Ejecutar de todas formas"**. El archivo no representa ningún riesgo.

6. Al iniciar, el sistema detectará automáticamente la placa. Cuando aparezca el mensaje **"Acerca la pieza al lector"**, acercar la réplica física del silbato al lector RFID de la placa para iniciar la experiencia.

### Controles de la Placa

| Control | Función |
|---|---|
| **Joystick** | Navegación en menús y manipulación del modelo 3D |
| **Potenciómetro** | Zoom en el visor 3D (derecha = acercar / izquierda = alejar) |
| **Botón** | Confirmar selecciones e interactuar con la experiencia |

> **Para cerrar la aplicación:** Presionar la tecla Windows y cerrar la ventana del programa. El sistema opera en modo kiosco y no dispone de botón de cierre en la interfaz.

---

## 🎬 Recursos del Proyecto

| Recurso | Enlace |
|---|---|
| 🎥 Video de presentación del SMMV LiliQuest | [Ver en YouTube](https://youtu.be/0kIXJgRrw4Y) |
| 📁 Demo descargable | [Carpeta SharePoint](https://uao-my.sharepoint.com/:f:/g/personal/andres_gab_fernandez_uao_edu_co/IgBh_nMCRKanR5ER8VB31RloAamOZymdZyvRCdQNP7dvjXg?e=H24pqW) |
| 🗺️ Mapa general de Historias de Usuario | [Ver diagrama](https://uao-my.sharepoint.com/:b:/g/personal/andres_gab_fernandez_uao_edu_co/IQBKShjB-8TVRbFO-ZT6Qt_xATpAb3VTz44MQ-RMHJa6tsc) |
| 🏗️ Modelo C4 de la arquitectura | [Ver diagrama](https://uao-my.sharepoint.com/:b:/g/personal/andres_gab_fernandez_uao_edu_co/IQC9lRrzqmtJTJybzfFPPSN_AbfBFE5_2lurnZSiXGRsfuk?e=1HyKgQ) |
| 📄 Documento de preproducción | [Ver documento](https://uao-my.sharepoint.com/:w:/g/personal/andres_gab_fernandez_uao_edu_co/IQC6sN1gnwhdRI104Uh4BfbJAdampKAVo_Oti88Izwn29aw?e=RvTQ6v) |

---

## 🔗 Repositorio

> 📌 **Repositorio GitHub del proyecto:**
>
> `https://github.com/kaki309/Lili_Quest`



---

<div align="center">

*Universidad Autónoma de Occidente — Facultad de Ingeniería y Ciencias Básicas*  
*Ingeniería Multimedia — Diseño Multimedia 2 — 2026*

</div>
