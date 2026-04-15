using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorElementosUI : MonoBehaviour
{

    public static GestorElementosUI Instance;

    [SerializeField] Camera camara;
    [SerializeField] Image imagen;
    [SerializeField] TMP_Text subtitulo;
    [SerializeField] GameObject contenedorModelo3D;
    [SerializeField] Button[] botones;
    [Tooltip("Campo privado para crear un diccionario en el inspector. La API pública es 'Otros' ")]
    [SerializeField] List<EntradaDiccionario> otrasCosas;
    public Dictionary<Otrostipos, GameObject> Otros;

    [System.Serializable]
    private class EntradaDiccionario
    {
        /// <summary>
        /// El sistema "OtrosTipos" se ha implementado por cuestiones de facilidad
        /// para comunicación entre scripts.
        /// Solo puede haber un único objeto en la escena por cada tipo de objeto
        /// de las opciones disponibles en este selector.
        /// </summary>
        [Tooltip(@"El sistema ""OtrosTipos"" se ha implementado por cuestiones de facilidad para comunicación entre scripts.
        Solo puede haber un único objeto en la escena por cada tipo de objeto de las opciones disponibles en este selector.")]
        public Otrostipos tipo;
        public GameObject objeto;
    }
    public enum Otrostipos
    {
        panelDecorativoInicial,
        botonPlay,
        botonExit,
        otro
    };

    void Awake()
    {
        Instance = this;
        Otros = new Dictionary<Otrostipos, GameObject>();

        foreach (var cosa in otrasCosas)
        {
            Otros.Add(cosa.tipo, cosa.objeto);
        }
    }
    void Start()
    {
        if (!camara) camara = Camera.main;
    }

    public Camera Camara => camara;
    public Button[] Botones => botones;
    public Image Imagen => imagen;
    public TMP_Text Subtitulo => subtitulo;
    public GameObject ContenedorModelo3D => contenedorModelo3D;
}
