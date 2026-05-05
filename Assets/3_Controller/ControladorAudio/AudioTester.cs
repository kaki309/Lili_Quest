using UnityEngine;
 
/// <summary>
/// Script de prueba para el AudioController.
/// C = simula fractura (destrucción)
/// D = simula construcción exitosa
/// </summary>
public class AudioTester : MonoBehaviour
{
    void Update()
    {
        // Simula que la pieza se destruyó / fracturó
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(" Pieza fracturada — sonando PlayError()");
            AudioController.Instance.PlayError();
        }
 
        // Simula que la pieza fue construida correctamente
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Pieza construida — sonando PlayCorrect() + PlaySuccessJingle()");
            AudioController.Instance.PlayCorrect();
          
        }
    }
}
 
