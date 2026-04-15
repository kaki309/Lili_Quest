using UnityEngine;

public class GestorInterfazPantallaRuptura : MonoBehaviour
{
    public static GestorInterfazPantallaRuptura Instance;

    void Start()
    {
        Instance = this;
    }
}
