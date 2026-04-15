using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorInterfazPantallaInicio : MonoBehaviour
{
    public static GestorInterfazPantallaInicio Instance;
    [SerializeField] private GameObject _contenedorModelo3D;
    [SerializeField] private Button _botonInicioExperiencia;
    [SerializeField] private GameObject _panelDecorativo;

    void Start()
    {
        Instance = this;
    }
    public GameObject ContenedorModelo3D => _contenedorModelo3D;
    public Button BotonInicioExperiencia => _botonInicioExperiencia;
    public GameObject panelDecorativo => _panelDecorativo;
}
