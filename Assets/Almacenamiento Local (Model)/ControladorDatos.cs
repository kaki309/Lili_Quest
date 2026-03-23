using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// ============================================================================
// CONTROLADOR PRINCIPAL
// ============================================================================
// Nota: Las clases de datos (StringStringPair, ExperienceData) están en archivos separados

public class ControladorDatos : MonoBehaviour
{
    private static ControladorDatos _instance;
    public static ControladorDatos Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ControladorDatos>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ControladorDatos");
                    _instance = obj.AddComponent<ControladorDatos>();
                }
            }
            return _instance;
        }
    }

    // En memoria: diccionario ID -> ExperienceData
    private Dictionary<string, ExperienceData> _database;
    private const string EXTERNAL_FOLDER_NAME = "ExperienciasMuseo";
    private const string JSON_FILENAME = "SerializadorDatos.json";
    private const string STREAMING_ASSETS_SUBFOLDER = "DefaultContent";

    private string _externalDataPath;
    private string _streamingAssetsPath;

    // ========================================================================
    // CICLO DE VIDA
    // ========================================================================

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    // ========================================================================
    // INICIALIZACIÓN
    // ========================================================================

    public void Initialize()
    {
        try
        {
            _database = new Dictionary<string, ExperienceData>();
            _externalDataPath = GetExternalDataPath();
            _streamingAssetsPath = GetStreamingAssetsDefaultPath();

            Debug.Log($"[ControladorDatos] Ruta externa: {_externalDataPath}");
            Debug.Log($"[ControladorDatos] Ruta StreamingAssets: {_streamingAssetsPath}");

            // Verificar si carpeta externa existe y contiene datos válidos
            if (DirectoryExists(_externalDataPath) && ValidateSerializadorDatos(_externalDataPath))
            {
                Debug.Log("[ControladorDatos] Cargando datos desde carpeta externa...");
                LoadDatabase(_externalDataPath);
            }
            else
            {
                Debug.Log("[ControladorDatos] Carpeta externa no válida. Inicializando con datos base...");
                InitializeWithDefaultData();
            }

            ValidateLoadedData();
            Debug.Log($"[ControladorDatos] Inicialización completada. Experiencias disponibles: {_database.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error en Initialize: {ex.Message}");
        }
    }

    private void InitializeWithDefaultData()
    {
        try
        {
            // Crear carpeta externa si no existe
            if (!DirectoryExists(_externalDataPath))
            {
                Directory.CreateDirectory(_externalDataPath);
                Debug.Log($"[ControladorDatos] Carpeta externa creada: {_externalDataPath}");
            }

            // Copiar datos base desde StreamingAssets a carpeta externa
            if (DirectoryExists(_streamingAssetsPath))
            {
                CopyDirectory(_streamingAssetsPath, _externalDataPath);
                Debug.Log("[ControladorDatos] Datos base copiados a carpeta externa");
            }
            else
            {
                Debug.LogWarning($"[ControladorDatos] Carpeta StreamingAssets no encontrada: {_streamingAssetsPath}");
            }

            // Cargar desde carpeta externa
            LoadDatabase(_externalDataPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error al inicializar con datos base: {ex.Message}");
        }
    }

    // ========================================================================
    // CARGA DE JSON
    // ========================================================================

    private void LoadDatabase(string basePath)
    {
        try
        {
            string jsonPath = Path.Combine(basePath, JSON_FILENAME);

            if (!FileExists(jsonPath))
            {
                Debug.LogWarning($"[ControladorDatos] JSON no encontrado: {jsonPath}");
                _database = new Dictionary<string, ExperienceData>();
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            _database = new Dictionary<string, ExperienceData>();

            // Parsear JSON manualmente: es un diccionario {id: {data}}
            // Usando SimpleJSON o parseando con cuidado
            try
            {
                // Extraer pares clave-valor del JSON
                var dict = ParseJsonDictionary(jsonContent);
                
                foreach (var kvp in dict)
                {
                    string experienceId = kvp.Key;
                    string experienceJson = kvp.Value;
                    
                    try
                    {
                        ExperienceData data = JsonUtility.FromJson<ExperienceData>(experienceJson);
                        if (data != null)
                        {
                            _database[experienceId] = data;
                            Debug.Log($"[ControladorDatos] Experiencia cargada: {experienceId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ControladorDatos] Error deserializando experiencia {experienceId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControladorDatos] Error parseando JSON: {ex.Message}");
            }

            Debug.Log($"[ControladorDatos] JSON cargado correctamente. Experiencias: {_database.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error cargando JSON: {ex.Message}");
            _database = new Dictionary<string, ExperienceData>();
        }
    }

    private Dictionary<string, string> ParseJsonDictionary(string jsonContent)
    {
        var result = new Dictionary<string, string>();

        jsonContent = jsonContent.Trim();
        if (!jsonContent.StartsWith("{") || !jsonContent.EndsWith("}"))
        {
            Debug.LogError("[ControladorDatos] JSON no es un objeto válido");
            return result;
        }

        // Remover corchetes externos
        jsonContent = jsonContent.Substring(1, jsonContent.Length - 2).Trim();

        int i = 0;
        while (i < jsonContent.Length)
        {
            // Buscar comilla de inicio de clave
            while (i < jsonContent.Length && jsonContent[i] != '"')
            {
                i++;
            }

            if (i >= jsonContent.Length) break;

            // Extraer clave (entre comillas)
            i++; // saltar la comilla de apertura
            int keyStart = i;
            while (i < jsonContent.Length && jsonContent[i] != '"')
            {
                i++;
            }

            string key = jsonContent.Substring(keyStart, i - keyStart);
            i++; // saltar la comilla de cierre

            // Buscar el ':'
            while (i < jsonContent.Length && jsonContent[i] != ':')
            {
                i++;
            }
            i++; // saltar el ':'

            // Buscar el inicio del valor (puede ser { o [)
            while (i < jsonContent.Length && char.IsWhiteSpace(jsonContent[i]))
            {
                i++;
            }

            if (i >= jsonContent.Length) break;

            // Extraer el valor (objeto completo)
            int valueStart = i;
            int braceDepth = 0;

            while (i < jsonContent.Length)
            {
                if (jsonContent[i] == '{')
                {
                    braceDepth++;
                }
                else if (jsonContent[i] == '}')
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        i++;
                        break;
                    }
                }
                i++;
            }

            string value = jsonContent.Substring(valueStart, i - valueStart).Trim();
            result[key] = value;

            // Buscar la siguiente coma o fin del objeto
            while (i < jsonContent.Length && (jsonContent[i] == ',' || char.IsWhiteSpace(jsonContent[i])))
            {
                i++;
            }
        }

        return result;
    }

    private string FindExperienceId(string basePath, ExperienceData data)
    {
        // Este método ya no es necesario, pero lo mantenemos por compatibilidad
        var folders = Directory.GetDirectories(basePath);
        
        foreach (var folderPath in folders)
        {
            string folderName = new DirectoryInfo(folderPath).Name;
            
            string modelPath = Path.Combine(folderPath, data.modelo);
            string sequencePath = Path.Combine(folderPath, data.secuencia);
            string imagesPath = Path.Combine(folderPath, "imagenes");
            string audiosPath = Path.Combine(folderPath, "audios");

            if (FileExists(modelPath) && FileExists(sequencePath) &&
                DirectoryExists(imagesPath) && DirectoryExists(audiosPath))
            {
                return folderName;
            }
        }

        return null;
    }

    private void ValidateLoadedData()
    {
        foreach (var kvp in _database)
        {
            string experienceId = kvp.Key;
            ExperienceData data = kvp.Value;
            string experiencePath = Path.Combine(_externalDataPath, experienceId);

            if (!ValidateExperienceFolder(experiencePath, data))
            {
                Debug.LogWarning($"[ControladorDatos] Validación fallida para experiencia: {experienceId}");
            }
        }
    }

    private bool ValidateExperienceFolder(string folderPath, ExperienceData data)
    {
        try
        {
            if (!DirectoryExists(folderPath)) return false;
            
            if (!FileExists(Path.Combine(folderPath, data.modelo))) return false;
            if (!FileExists(Path.Combine(folderPath, data.secuencia))) return false;
            if (!DirectoryExists(Path.Combine(folderPath, "imagenes"))) return false;
            if (!DirectoryExists(Path.Combine(folderPath, "audios"))) return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ========================================================================
    // API PÚBLICA - LECTURA (Para otros controladores)
    // ========================================================================

    public string[] GetExperienceIds()
    {
        return _database.Keys.ToArray();
    }

    /// <summary>
    /// Obtiene TODA la información necesaria de una experiencia en una única llamada.
    /// Incluye rutas COMPLETAS para modelo, secuencia, imágenes y audios.
    /// 
    /// Esta es la API pública principal para acceder a datos de experiencias.
    /// 
    /// Ejemplo:
    ///   var data = ControladorDatos.Instance.GetExperienceData(id);
    ///   
    ///   // Rutas completas, listas para usar:
    ///   File.ReadAllText(data.modeloPath);
    ///   File.ReadAllText(data.secuenciaPath);
    ///   File.ReadAllBytes(data.imagenes["logo"]);
    ///   File.ReadAllBytes(data.audios["bienvenida"]);
    /// 
    /// Retorna null si la experiencia no existe.
    /// </summary>
    public ParsedExperienceData GetExperienceData(string id)
    {
        if (!_database.ContainsKey(id))
        {
            Debug.LogWarning($"[ControladorDatos] Experiencia no encontrada: {id}");
            return null;
        }

        string experiencePath = Path.Combine(_externalDataPath, id);
        return new ParsedExperienceData(_database[id], experiencePath);
    }

    // ========================================================================
    // NOTA: Los métodos GetModelPath(), GetSequencePath(), GetImagePaths()
    // y GetAudioPaths() ya NO son necesarios.
    // 
    // Toda esa información ahora se obtiene en una única llamada a través de
    // GetExperienceData(id), que retorna ParsedExperienceData con todas las
    // rutas completas listas para usar.
    // ========================================================================

    // ========================================================================
    // API PÚBLICA - HELPERS DE CARGA
    // ========================================================================

    public Texture2D LoadImage(string imagePath)
    {
        try
        {
            if (!FileExists(imagePath))
            {
                Debug.LogWarning($"[ControladorDatos] Imagen no encontrada: {imagePath}");
                return null;
            }

            byte[] imageData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageData);
            return texture;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error cargando imagen: {ex.Message}");
            return null;
        }
    }

    public AudioClip LoadAudio(string audioPath)
    {
        try
        {
            if (!FileExists(audioPath))
            {
                Debug.LogWarning($"[ControladorDatos] Audio no encontrado: {audioPath}");
                return null;
            }

            // Para cargar AudioClips desde archivos externos, se necesita UnityWebRequest
            // Por ahora retornamos una advertencia indicando que se necesita implementación específica
            Debug.LogWarning($"[ControladorDatos] Load de audio requiere implementación con UnityWebRequest: {audioPath}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error cargando audio: {ex.Message}");
            return null;
        }
    }

    // ========================================================================
    // API PÚBLICA - ADMIN
    // ========================================================================

    public bool CreateExperience(
        string id,
        ExperienceData data,
        Dictionary<string, string> imagePaths,
        Dictionary<string, string> audioPaths,
        string sequencePath,
        string modelPath)
    {
        try
        {
            // Validaciones
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[ControladorDatos] ID no puede ser nulo");
                return false;
            }

            if (_database.ContainsKey(id))
            {
                Debug.LogError($"[ControladorDatos] La experiencia '{id}' ya existe");
                return false;
            }

            if (data == null)
            {
                Debug.LogError("[ControladorDatos] ExperienceData no puede ser nulo");
                return false;
            }

            if (!FileExists(modelPath) || !FileExists(sequencePath))
            {
                Debug.LogError("[ControladorDatos] Archivos de modelo o secuencia no existen");
                return false;
            }

            // Crear carpeta de experiencia
            string experienceFolderPath = Path.Combine(_externalDataPath, id);
            if (DirectoryExists(experienceFolderPath))
            {
                Debug.LogError($"[ControladorDatos] La carpeta '{id}' ya existe");
                return false;
            }

            Directory.CreateDirectory(experienceFolderPath);
            Directory.CreateDirectory(Path.Combine(experienceFolderPath, "imagenes"));
            Directory.CreateDirectory(Path.Combine(experienceFolderPath, "audios"));

            Debug.Log($"[ControladorDatos] Carpeta de experiencia creada: {experienceFolderPath}");

            // Copiar modelo y secuencia
            string destModelPath = Path.Combine(experienceFolderPath, data.modelo);
            string destSequencePath = Path.Combine(experienceFolderPath, data.secuencia);

            File.Copy(modelPath, destModelPath, true);
            File.Copy(sequencePath, destSequencePath, true);

            Debug.Log("[ControladorDatos] Modelo y secuencia copiados");

            // Copiar imágenes
            if (imagePaths != null)
            {
                foreach (var kvp in imagePaths)
                {
                    string sourceImagePath = kvp.Value;
                    if (FileExists(sourceImagePath))
                    {
                        string fileName = Path.GetFileName(sourceImagePath);
                        string destImagePath = Path.Combine(experienceFolderPath, "imagenes", fileName);
                        File.Copy(sourceImagePath, destImagePath, true);
                    }
                    else
                    {
                        Debug.LogWarning($"[ControladorDatos] Imagen no encontrada: {sourceImagePath}");
                    }
                }
            }

            // Copiar audios
            if (audioPaths != null)
            {
                foreach (var kvp in audioPaths)
                {
                    string sourceAudioPath = kvp.Value;
                    if (FileExists(sourceAudioPath))
                    {
                        string fileName = Path.GetFileName(sourceAudioPath);
                        string destAudioPath = Path.Combine(experienceFolderPath, "audios", fileName);
                        File.Copy(sourceAudioPath, destAudioPath, true);
                    }
                    else
                    {
                        Debug.LogWarning($"[ControladorDatos] Audio no encontrado: {sourceAudioPath}");
                    }
                }
            }

            // Actualizar data con rutas relativas correctas
            var imageArray = new List<StringStringPair>();
            if (imagePaths != null)
            {
                foreach (var kvp in imagePaths)
                {
                    string fileName = Path.GetFileName(kvp.Value);
                    imageArray.Add(new StringStringPair(kvp.Key, Path.Combine("imagenes", fileName).Replace("\\", "/")));
                }
            }
            data.imagenes = imageArray.ToArray();

            var audioArray = new List<StringStringPair>();
            if (audioPaths != null)
            {
                foreach (var kvp in audioPaths)
                {
                    string fileName = Path.GetFileName(kvp.Value);
                    audioArray.Add(new StringStringPair(kvp.Key, Path.Combine("audios", fileName).Replace("\\", "/")));
                }
            }
            data.audios = audioArray.ToArray();

            // Agregar a base de datos en memoria
            _database[id] = data;

            // Persistir JSON
            SaveDatabase();

            Debug.Log($"[ControladorDatos] Experiencia creada exitosamente: {id}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error creando experiencia: {ex.Message}");
            return false;
        }
    }

    // ========================================================================
    // PERSISTENCIA
    // ========================================================================

    private void SaveDatabase()
    {
        try
        {
            // Convertir diccionario a JSON manualmente
            // Formato: { "id1": {...}, "id2": {...} }
            var jsonParts = new List<string>();

            foreach (var kvp in _database)
            {
                string id = kvp.Key;
                ExperienceData data = kvp.Value;

                // Serializar la experiencia
                string dataJson = JsonUtility.ToJson(data, true);
                
                // Agregar con la clave ID
                jsonParts.Add($"  \"{id}\": {dataJson}");
            }

            string json = "{\n" + string.Join(",\n", jsonParts) + "\n}";

            string jsonPath = Path.Combine(_externalDataPath, JSON_FILENAME);
            File.WriteAllText(jsonPath, json);

            Debug.Log($"[ControladorDatos] JSON guardado: {jsonPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error guardando JSON: {ex.Message}");
        }
    }

    // ========================================================================
    // HELPERS PRIVADOS
    // ========================================================================

    private string GetExternalDataPath()
    {
        // Retorna: [ProjectRoot]/ExperienciasMuseo/
        string dataPath = Application.dataPath;  // Ej: C:\Proyecto\Assets
        string projectRoot = Directory.GetParent(dataPath).FullName;  // C:\Proyecto
        return Path.Combine(projectRoot, EXTERNAL_FOLDER_NAME);
    }

    private string GetStreamingAssetsDefaultPath()
    {
        // Retorna: Assets/StreamingAssets/DefaultContent
        return Path.Combine(Application.streamingAssetsPath, STREAMING_ASSETS_SUBFOLDER);
    }

    private bool DirectoryExists(string path)
    {
        return !string.IsNullOrEmpty(path) && Directory.Exists(path);
    }

    private bool FileExists(string path)
    {
        return !string.IsNullOrEmpty(path) && File.Exists(path);
    }

    private bool ValidateSerializadorDatos(string basePath)
    {
        try
        {
            string jsonPath = Path.Combine(basePath, JSON_FILENAME);
            if (!FileExists(jsonPath)) return false;

            string content = File.ReadAllText(jsonPath);
            return !string.IsNullOrEmpty(content);
        }
        catch
        {
            return false;
        }
    }

    private void CopyDirectory(string sourceDir, string destDir)
    {
        try
        {
            // Crear directorio destino si no existe
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Copiar archivos (excluyendo archivos .meta de Unity)
            var files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                
                // Saltar archivos .meta (metadatos de Unity del editor)
                if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }

            // Copiar subdirectorios
            var dirs = Directory.GetDirectories(sourceDir);
            foreach (string dir in dirs)
            {
                string dirName = new DirectoryInfo(dir).Name;
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir);
            }

            Debug.Log($"[ControladorDatos] Directorio copiado: {sourceDir} -> {destDir}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ControladorDatos] Error copiando directorio: {ex.Message}");
        }
    }
}
