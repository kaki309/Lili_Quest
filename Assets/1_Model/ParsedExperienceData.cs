using System.Collections.Generic;

/// <summary>
/// Estructura de datos completamente parseada/procesada de una experiencia de museo.
/// Utilizada como retorno ÚNICO de la API pública GetExperienceData().
/// 
/// Contiene toda la información necesaria para usar una experiencia:
/// - Entrega rutas COMPLETAS para cada material
/// 
/// Ejemplo de uso:
///   var data = ControladorDatos.Instance.GetExperienceData(id);
///   File.ReadAllText(data.secuenciaPath);           // Ruta completa
///   Texture2D.LoadImage(data.imagenes["logo"]);     // Rutas completas en diccionario
/// </summary>
public class ParsedExperienceData
{
    public string modeloPath;

    public string secuenciaPath;

    public Dictionary<string, string> imagenes;

    public Dictionary<string, string> audios;

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
