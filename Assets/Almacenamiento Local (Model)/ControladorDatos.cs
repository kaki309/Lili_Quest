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
    public static ControladorDatos Instance {get; private set;}

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
            if (Directory.Exists(_externalDataPath) && JsonFileIsValid(_externalDataPath))
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
            if (!Directory.Exists(_externalDataPath))
            {
                Directory.CreateDirectory(_externalDataPath);
                Debug.Log($"[ControladorDatos] Carpeta externa creada: {_externalDataPath}");
            }

            // Copiar datos base desde StreamingAssets a carpeta externa
            if (Directory.Exists(_streamingAssetsPath))
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

            if (!File.Exists(jsonPath))
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
            if (!Directory.Exists(folderPath)) return false;
            
            if (!File.Exists(Path.Combine(folderPath, data.modelo))) return false;
            if (!File.Exists(Path.Combine(folderPath, data.secuencia))) return false;
            if (!Directory.Exists(Path.Combine(folderPath, "imagenes"))) return false;
            if (!Directory.Exists(Path.Combine(folderPath, "audios"))) return false;

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
    /// Ejemplo:
    ///   var data = ControladorDatos.Instance.GetExperienceData(id);
    ///   
    ///   // Rutas completas, listas para usar:
    ///   File.ReadAllText(data.modeloPath);
    ///   File.ReadAllBytes(data.imagenes["logo"]);
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
    // API PÚBLICA - ADMIN
    // ========================================================================

    /// <summary>
    /// Crea una nueva experiencia delegando al helper ExperienceCreator.
    /// Después de crear los archivos, actualiza la base de datos en memoria.
    /// 
    /// Uso:
    ///   bool success = ControladorDatos.Instance.CreateExperience(
    ///       id, data, imagePaths, audioPaths, sequencePath, modelPath
    ///   );
    /// </summary>
    public bool CreateExperience(
        string id,
        ExperienceData data,
        Dictionary<string, string> imagePaths,
        Dictionary<string, string> audioPaths,
        string sequencePath,
        string modelPath)
    {
        // Validación: la experiencia no debe existir
        if (_database.ContainsKey(id))
        {
            Debug.LogError($"[ControladorDatos] La experiencia '{id}' ya existe");
            return false;
        }

        // Delegar creación de archivos y estructura al helper
        bool creationSuccess = ExperienceCreator.CreateExperience(
            id,
            _externalDataPath,
            data,
            imagePaths,
            audioPaths,
            sequencePath,
            modelPath
        );

        if (!creationSuccess)
        {
            return false;
        }

        // Agregar a base de datos en memoria
        _database[id] = data;

        // Persistir JSON
        SaveDatabase();

        Debug.Log($"[ControladorDatos] Experiencia '{id}' registrada en base de datos");
        return true;
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

    private bool JsonFileIsValid(string basePath)
    {
        try
        {
            string jsonPath = Path.Combine(basePath, JSON_FILENAME);
            if (!File.Exists(jsonPath)) return false;

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
