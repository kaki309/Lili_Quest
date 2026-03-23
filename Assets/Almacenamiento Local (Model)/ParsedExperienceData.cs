using System.Collections.Generic;

/// <summary>
/// Estructura de datos completamente parseada/procesada de una experiencia de museo.
/// Utilizada como retorno ÚNICO de la API pública GetExperienceData().
/// 
/// Contiene toda la información necesaria para usar una experiencia:
/// - Rutas COMPLETAS (no relativas) para modelo, secuencia, imágenes y audios
/// - Se puede usar directamente sin necesidad de llamadas adicionales
/// 
/// Ejemplo de uso:
///   var data = ControladorDatos.Instance.GetExperienceData(id);
///   File.ReadAllText(data.modeloPath);              // Ruta completa
///   File.ReadAllText(data.secuenciaPath);           // Ruta completa
///   Texture2D.LoadImage(data.imagenes["logo"]);     // Rutas completas en diccionario
/// </summary>
public class ParsedExperienceData
{
    /// <summary>
    /// Ruta COMPLETA al archivo del modelo 3D (ej: C:\\Ruta\\ExperienciasMuseo\\exp1\\miModelo.fbx)
    /// </summary>
    public string modeloPath;

    /// <summary>
    /// Ruta COMPLETA al archivo de secuencia de instrucciones (ej: C:\\Ruta\\ExperienciasMuseo\\exp1\\secuencia.txt)
    /// </summary>
    public string secuenciaPath;

    /// <summary>
    /// Diccionario de imágenes con rutas COMPLETAS: nombre -> ruta completa
    /// Ejemplo: imagenes["logo"] = C:\\Ruta\\ExperienciasMuseo\\exp1\\imagenes\\logo.png
    /// </summary>
    public Dictionary<string, string> imagenes;

    /// <summary>
    /// Diccionario de audios con rutas COMPLETAS: nombre -> ruta completa
    /// Ejemplo: audios["bienvenida"] = C:\\Ruta\\ExperienciasMuseo\\exp1\\audios\\bienvenida.mp3
    /// </summary>
    public Dictionary<string, string> audios;

    /// <summary>
    /// Constructor que inicializa los diccionarios vacíos
    /// </summary>
    public ParsedExperienceData()
    {
        modeloPath = "";
        secuenciaPath = "";
        imagenes = new Dictionary<string, string>();
        audios = new Dictionary<string, string>();
    }

    /// <summary>
    /// Constructor que inicializa a partir de ExperienceData interna con rutas completas
    /// </summary>
    public ParsedExperienceData(ExperienceData internalData, string experienceFolderPath)
    {
        // Rutas completas para modelo y secuencia
        modeloPath = System.IO.Path.Combine(experienceFolderPath, internalData.modelo);
        secuenciaPath = System.IO.Path.Combine(experienceFolderPath, internalData.secuencia);

        // Convertir arrays a diccionarios con rutas completas
        imagenes = new Dictionary<string, string>();
        foreach (var pair in internalData.imagenes)
        {
            string fullPath = System.IO.Path.Combine(experienceFolderPath, pair.value);
            imagenes[pair.key] = fullPath;
        }

        audios = new Dictionary<string, string>();
        foreach (var pair in internalData.audios)
        {
            string fullPath = System.IO.Path.Combine(experienceFolderPath, pair.value);
            audios[pair.key] = fullPath;
        }
    }
}
