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
    [SerializeField] TMP_Text _preguntaTrivia;
    [SerializeField] Button[] _botonesTrivia;
    
    int botonRespuestaCorrectaIndex;
    Button _botonCorrecto;
    Button[] _botonesIncorrecto = new Button[0];

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        botonRespuestaCorrectaIndex = -1;
    }
    public GameObject EncuadreFoto => _encuadreFoto;
    public Image Foto => _foto;
    public TMP_Text ReferenciaInfo => _referenciaInfo;
    public TMP_Text Subtitulo => _subtitulo;
    public GameObject CanvasTrivia => _canvasTrivia;
    public Image LaIaInTrivia => _laiaInTrivia;
    public TMP_Text PreguntaTrivia => _preguntaTrivia;
    public void SetDistribucionBotones()
    {
        botonRespuestaCorrectaIndex = Random.Range(0, _botonesTrivia.Length);
        _botonCorrecto = _botonesTrivia[botonRespuestaCorrectaIndex];
        _botonesIncorrecto = new Button[_botonesTrivia.Length - 1];
        int indexIncorrecto = 0;
        for (int i = 0; i < _botonesTrivia.Length; i++)
        {
            if (i == botonRespuestaCorrectaIndex) continue;
            _botonesIncorrecto[indexIncorrecto] = _botonesTrivia[i];
            indexIncorrecto++;
        }
    }
    public Button[] BotonesTrivia => _botonesTrivia;
    public Button BotonCorrecto => _botonCorrecto;
    public Button[] BotonesIncorrectos => _botonesIncorrecto;
}
