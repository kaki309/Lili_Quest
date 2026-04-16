using UnityEngine;

public class GestorInterfazPantallasVisor3D : MonoBehaviour
{
    public static GestorInterfazPantallasVisor3D Instance;

    [SerializeField] GameObject _contenedorModelo3D;
    [SerializeField] GameObject _recuadroLaia;
    [SerializeField] AudioSource audioSource;

    void Awake()
    {
        Instance = this;
    }

    public GameObject ContenedorModelo3D => _contenedorModelo3D;
    public AudioSource AudioSource => audioSource;
    public GameObject RecuadroLaia => _recuadroLaia;
}
