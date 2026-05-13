using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorInterfazPantallaNarrativa : MonoBehaviour
{
    public static GestorInterfazPantallaNarrativa Instance;
    [Header("Multimedia Narrativa")]
    [SerializeField] GameObject _encuadreFoto;
    [SerializeField] Image _foto;
    [SerializeField] TMP_Text _referenciaInfo;
    [SerializeField] TMP_Text _subtitulo;

    [Header("Trivia")]
    [SerializeField] GameObject _canvasTrivia;
    [SerializeField] Image _laiaInTrivia;

    void Awake()
    {
        Instance = this;
    }
    public GameObject EncuadreFoto => _encuadreFoto;
    public Image Foto => _foto;
    public TMP_Text ReferenciaInfo => _referenciaInfo;
    public TMP_Text Subtitulo => _subtitulo;
    public GameObject CanvasTrivia => _canvasTrivia;
    public Image LaIaInTrivia => _laiaInTrivia;
}
