using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorInterfazPantallaInicio : MonoBehaviour
{
    public static GestorInterfazPantallaInicio Instance;
    [SerializeField] GameObject _textoEsperandoControles;
    [SerializeField] GameObject _textoEsperandoLectura;
    [SerializeField] Button _botonInicioExperiencia;
    [SerializeField] GameObject _ruedaDecorativa;
    [SerializeField] AudioClip _ruedaSFX;

    void Awake()
    {
        Instance = this;
    }
    public GameObject RuedaDecorativa => _ruedaDecorativa;
    public Button BotonInicioExperiencia => _botonInicioExperiencia;
    public GameObject textoEsperandoLectura => _textoEsperandoLectura;
    public GameObject textoEsperandoControles => _textoEsperandoControles;
    public AudioClip SfxRueda => _ruedaSFX;
}
