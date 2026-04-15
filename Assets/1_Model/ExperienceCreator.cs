using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Helper estático para la creación de nuevas experiencias de museo.
/// Encapsula toda la lógica de copia de archivos, validaciones y actualización de datos.
/// 
/// Uso:
///   bool success = ExperienceCreator.CreateExperience(
///       id: "mi_experiencia",
///       basePath: "/ruta/ExperienciasMuseo",
///       data: experienceData,
///       ... otros parámetros
///   );
/// </summary>
public static class ExperienceCreator
{
    /// <summary>
    /// Crea una nueva experiencia con todos sus archivos y datos asociados.
    /// </summary>
    /// <param name="id">ID único de la experiencia</param>
    /// <param name="basePath">Ruta base donde se creará la carpeta de la experiencia</param>
    /// <param name="data">Estructura de datos de la experiencia</param>
    /// <param name="imagePaths">Diccionario de imágenes a copiar (name -> path)</param>
    /// <param name="audioPaths">Diccionario de audios a copiar (name -> path)</param>
    /// <param name="sequencePath">Ruta del archivo de secuencia</param>
    /// <param name="modelPath">Ruta del archivo del modelo 3D</param>
    /// <returns>true si se creó exitosamente, false si hubo algún error</returns>
    public static bool CreateExperience(
        string id,
        string basePath,
        ExperienceData data,
        Dictionary<string, string> imagePaths,
        Dictionary<string, string> audioPaths,
        string sequencePath,
        string modelPath)
    {
        try
        {
            // ====================================================================
            // VALIDACIONES INICIALES
            // ====================================================================

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[ExperienceCreator] ID no puede ser nulo");
                return false;
            }

            if (data == null)
            {
                Debug.LogError("[ExperienceCreator] ExperienceData no puede ser nulo");
                return false;
            }

            if (!File.Exists(modelPath) || !File.Exists(sequencePath))
            {
                Debug.LogError("[ExperienceCreator] Archivos de modelo o secuencia no existen");
                return false;
            }

            // ====================================================================
            // CREAR ESTRUCTURA DE CARPETAS
            // ====================================================================

            string experienceFolderPath = Path.Combine(basePath, id);
            if (Directory.Exists(experienceFolderPath))
            {
                Debug.LogError($"[ExperienceCreator] La carpeta '{id}' ya existe");
                return false;
            }

            Directory.CreateDirectory(experienceFolderPath);
            Directory.CreateDirectory(Path.Combine(experienceFolderPath, "imagenes"));
            Directory.CreateDirectory(Path.Combine(experienceFolderPath, "audios"));

            Debug.Log($"[ExperienceCreator] Carpeta de experiencia creada: {experienceFolderPath}");

            // ====================================================================
            // COPIAR ARCHIVOS DE MODELO Y SECUENCIA
            // ====================================================================

            string destModelPath = Path.Combine(experienceFolderPath, data.modelo);
            string destSequencePath = Path.Combine(experienceFolderPath, data.secuencia);

            File.Copy(modelPath, destModelPath, true);
            File.Copy(sequencePath, destSequencePath, true);

            Debug.Log("[ExperienceCreator] Modelo y secuencia copiados");

            // ====================================================================
            // COPIAR ARCHIVOS MULTIMEDIA
            // ====================================================================

            CopyMediaFiles(experienceFolderPath, "imagenes", imagePaths);
            CopyMediaFiles(experienceFolderPath, "audios", audioPaths);

            // ====================================================================
            // ACTUALIZAR REFERENCIAS EN LA ESTRUCTURA DE DATOS
            // ====================================================================

            data.imagenes = ConvertPathsToStringPairs(imagePaths, "imagenes");
            data.audios = ConvertPathsToStringPairs(audioPaths, "audios");

            Debug.Log($"[ExperienceCreator] Experiencia '{id}' creada exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ExperienceCreator] Error creando experiencia: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Copia archivos multimedia desde la fuente al destino.
    /// </summary>
    private static void CopyMediaFiles(string baseFolderPath, string mediaFolder, Dictionary<string, string> mediaFiles)
    {
        if (mediaFiles == null || mediaFiles.Count == 0)
            return;

        string destFolderPath = Path.Combine(baseFolderPath, mediaFolder);

        foreach (var kvp in mediaFiles)
        {
            string sourceFilePath = kvp.Value;
            if (File.Exists(sourceFilePath))
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string destFilePath = Path.Combine(destFolderPath, fileName);
                File.Copy(sourceFilePath, destFilePath, true);
            }
            else
            {
                Debug.LogWarning($"[ExperienceCreator] Archivo {mediaFolder} no encontrado: {sourceFilePath}");
            }
        }
    }

    /// <summary>
    /// Convierte un diccionario de rutas absolutas a un array de StringStringPair con rutas relativas.
    /// </summary>
    private static StringStringPair[] ConvertPathsToStringPairs(Dictionary<string, string> paths, string mediaFolder)
    {
        if (paths == null || paths.Count == 0)
            return new StringStringPair[0];

        var result = new List<StringStringPair>();

        foreach (var kvp in paths)
        {
            string fileName = Path.GetFileName(kvp.Value);
            string relativePath = Path.Combine(mediaFolder, fileName).Replace("\\", "/");
            result.Add(new StringStringPair(kvp.Key, relativePath));
        }

        return result.ToArray();
    }
}
