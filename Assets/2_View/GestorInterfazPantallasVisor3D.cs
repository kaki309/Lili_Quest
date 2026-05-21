using UnityEngine;
using UnityEngine.UI;

public class GestorInterfazPantallasVisor3D : MonoBehaviour
{
    public static GestorInterfazPantallasVisor3D Instance;

    [SerializeField] GameObject _contenedorModelo3D;
    [SerializeField] GameObject _fondoNegro;
    [SerializeField] AudioClip _audioFractura;
    [SerializeField] GameObject _panelSalir;
    [SerializeField] Button _botonCancelar;
    [SerializeField] Button _botonReiniciar;

    void Awake()
    {
        Instance = this;
    }

    public GameObject ContenedorModelo3D => _contenedorModelo3D;
    public GameObject FondoNegro => _fondoNegro;
    public AudioClip AudioFractura => _audioFractura;
    public GameObject PanelSalir => _panelSalir;
    public Button BotonCancelar => _botonCancelar;
    public Button BotonReiniciar => _botonReiniciar;
}
