using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GestorElementosUI : MonoBehaviour
{
    public static GestorElementosUI Instance;
    [SerializeField] Camera camara;
    [SerializeField] Button[] botones;
    [SerializeField] Image imagen;
    [SerializeField] TMP_Text subtitulo;
    [SerializeField] GameObject contenedorModelo3D;

    void Start()
    {
        Instance = this;
        if (!camara) camara = Camera.main;
    }

    public Camera Camara => camara;
    public Button[] Botones => botones;
    public Image Imagen => imagen;
    public TMP_Text Subtitulo => subtitulo;
    public GameObject ContenedorModelo3D => contenedorModelo3D;
}
