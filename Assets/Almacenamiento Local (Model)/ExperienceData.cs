using System;

[System.Serializable]
public class ExperienceData
{
    public string modelo;
    public string secuencia;
    public StringStringPair[] imagenes;
    public StringStringPair[] audios;

    public ExperienceData()
    {
        modelo = "";
        secuencia = "";
        imagenes = new StringStringPair[0];
        audios = new StringStringPair[0];
    }
}
