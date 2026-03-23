using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script de prueba para validar la funcionalidad del ControladorDatos.
/// Adjunta este script a cualquier GameObject y ejecuta el juego para ver los resultados en la consola.
/// </summary>
public class TestControladorDatos : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("[TEST] ========== INICIANDO PRUEBAS DE ControladorDatos ==========");
        
        var controller = ControladorDatos.Instance;
        
        if (controller == null)
        {
            Debug.LogError("[TEST] ERROR: ControladorDatos.Instance es NULL");
            return;
        }

        // ====================================================================
        // PRUEBA 1: Obtener IDs de experiencias disponibles
        // ====================================================================
        Debug.Log("\n[TEST] --- PRUEBA 1: GetExperienceIds() ---");
        try
        {
            string[] ids = controller.GetExperienceIds();
            Debug.Log($"[TEST] IDs disponibles: {ids.Length}");
            
            if (ids.Length > 0)
            {
                Debug.Log($"[TEST] ✓ IDs encontrados:");
                foreach (string id in ids)
                {
                    Debug.Log($"       - {id}");
                }
            }
            else
            {
                Debug.LogWarning("[TEST] ⚠ No hay experiencias disponibles");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TEST] ✗ Error en GetExperienceIds(): {ex.Message}");
        }

        // ====================================================================
        // PRUEBA 2: Obtener datos de experiencia específica
        // ====================================================================
        Debug.Log("\n[TEST] --- PRUEBA 2: GetExperienceData() ---");
        try
        {
            ParsedExperienceData data = controller.GetExperienceData("silbato_forma_perro_cultura_quimbaya");
            
            if (data != null)
            {
                Debug.Log("[TEST] ✓ Datos de experiencia obtenidos:");
                Debug.Log($"       - Modelo: {data.modeloPath}");
                Debug.Log($"       - Secuencia: {data.secuenciaPath}");
                Debug.Log($"       - Imágenes: {data.imagenes.Count}");
                Debug.Log($"       - Audios: {data.audios.Count}");
                
                // Demostrar acceso directo a imágenes y audios
                if (data.imagenes.Count > 0)
                {
                    Debug.Log("[TEST]   Ejemplo de acceso directo a imágenes:");
                    foreach (var kvp in data.imagenes)
                    {
                        Debug.Log($"         - data.imagenes[\"{kvp.Key}\"] = {kvp.Value}");
                    }
                }
                if (data.audios.Count > 0)
                {
                    Debug.Log("[TEST]   Ejemplo de acceso directo a audios:");
                    foreach (var kvp in data.audios)
                    {
                        Debug.Log($"         - data.audios[\"{kvp.Key}\"] = {kvp.Value}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[TEST] ⚠ ParsedExperienceData es NULL para 'silbato_forma_perro_cultura_quimbaya'");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TEST] ✗ Error en GetExperienceData(): {ex.Message}");
        }

        // ====================================================================
        // PRUEBA 3: Validar rutas completas del modelo y secuencia
        // ====================================================================
        Debug.Log("\n[TEST] --- PRUEBA 3: Validación de rutas completas (ParsedExperienceData) ---");
        try
        {
            ParsedExperienceData data = controller.GetExperienceData("silbato_forma_perro_cultura_quimbaya");
            
            if (data != null)
            {
                Debug.Log("[TEST] ✓ Validando rutas del modelo y secuencia:");
                
                bool modeloExists = System.IO.File.Exists(data.modeloPath);
                Debug.Log($"[TEST]   {(modeloExists ? "✓" : "✗")} Modelo: {data.modeloPath}");
                
                bool secuenciaExists = System.IO.File.Exists(data.secuenciaPath);
                Debug.Log($"[TEST]   {(secuenciaExists ? "✓" : "✗")} Secuencia: {data.secuenciaPath}");
            }
            else
            {
                Debug.LogWarning("[TEST] ⚠ ParsedExperienceData es NULL");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TEST] ✗ Error en validación de rutas: {ex.Message}");
        }

        // ====================================================================
        // PRUEBA 4: Validar rutas completas de imágenes
        // ====================================================================
        Debug.Log("\n[TEST] --- PRUEBA 4: Rutas completas de imágenes ---");
        try
        {
            ParsedExperienceData data = controller.GetExperienceData("silbato_forma_perro_cultura_quimbaya");
            
            if (data != null && data.imagenes.Count > 0)
            {
                Debug.Log($"[TEST] ✓ Imágenes encontradas: {data.imagenes.Count}");
                foreach (var kvp in data.imagenes)
                {
                    bool exists = System.IO.File.Exists(kvp.Value);
                    string status = exists ? "✓" : "✗";
                    Debug.Log($"[TEST]   {status} {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                Debug.Log("[TEST]   (ninguna imagen registrada aún)");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TEST] ✗ Error en validación de imágenes: {ex.Message}");
        }

        // ====================================================================
        // PRUEBA 5: Validar rutas completas de audios
        // ====================================================================
        Debug.Log("\n[TEST] --- PRUEBA 5: Rutas completas de audios ---");
        try
        {
            ParsedExperienceData data = controller.GetExperienceData("silbato_forma_perro_cultura_quimbaya");
            
            if (data != null && data.audios.Count > 0)
            {
                Debug.Log($"[TEST] ✓ Audios encontrados: {data.audios.Count}");
                foreach (var kvp in data.audios)
                {
                    bool exists = System.IO.File.Exists(kvp.Value);
                    string status = exists ? "✓" : "✗";
                    Debug.Log($"[TEST]   {status} {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                Debug.Log("[TEST]   (ningún audio registrado aún)");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TEST] ✗ Error en validación de audios: {ex.Message}");
        }

        // ====================================================================
        // PRUEBA 6: Validar estructura de carpetas
        // ====================================================================
        Debug.Log("\n[TEST] --- PRUEBA 6: Estructura de carpetas externas ---");
        try
        {
            string externalPath = GetExternalDataPath();
            Debug.Log($"[TEST] Ruta externa: {externalPath}");
            
            if (System.IO.Directory.Exists(externalPath))
            {
                Debug.Log("[TEST] ✓ Carpeta externa existe");
                
                // Validar archivos principales
                string jsonPath = System.IO.Path.Combine(externalPath, "SerializadorDatos.json");
                bool jsonExists = System.IO.File.Exists(jsonPath);
                Debug.Log($"[TEST]   {(jsonExists ? "✓" : "✗")} SerializadorDatos.json");
                
                // Validar carpetas de experiencias
                var experienceFolders = System.IO.Directory.GetDirectories(externalPath);
                Debug.Log($"[TEST]   Carpetas de experiencias: {experienceFolders.Length}");
                
                foreach (var folder in experienceFolders)
                {
                    string folderName = System.IO.Path.GetFileName(folder);
                    Debug.Log($"[TEST]     - {folderName}");
                }
            }
            else
            {
                Debug.LogWarning("[TEST] ⚠ Carpeta externa NO existe");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TEST] ✗ Error validando estructura: {ex.Message}");
        }

        Debug.Log("\n[TEST] ========== PRUEBAS COMPLETADAS ==========\n");
    }

    /// <summary>
    /// Helper para obtener ruta externa (mismo cálculo que ControladorDatos)
    /// </summary>
    private string GetExternalDataPath()
    {
        string dataPath = Application.dataPath;
        string projectRoot = System.IO.Directory.GetParent(dataPath).FullName;
        return System.IO.Path.Combine(projectRoot, "ExperienciasMuseo");
    }
}
