using UnityEngine;

public class GestorInterfazPantallasVisor3D : MonoBehaviour
{
    public static GestorInterfazPantallasVisor3D Instance;

    [SerializeField] GameObject _contenedorModelo3D;
    [SerializeField] GameObject _fondoNegro;
    [SerializeField] AudioClip _audioFractura;

    void Awake()
    {
        Instance = this;
    }

    public GameObject ContenedorModelo3D => _contenedorModelo3D;
    public GameObject FondoNegro => _fondoNegro;
    public AudioClip AudioFractura => _audioFractura;
}
