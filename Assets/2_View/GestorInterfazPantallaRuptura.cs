using UnityEngine;

public class GestorInterfazPantallasVisor3D : MonoBehaviour
{
    public static GestorInterfazPantallasVisor3D Instance;

    [SerializeField] private GameObject _contenedorModelo3D;

    void Start()
    {
        Instance = this;
    }

    public GameObject ContenedorModelo3D => _contenedorModelo3D;
}
