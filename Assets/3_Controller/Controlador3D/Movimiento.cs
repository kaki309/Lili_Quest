using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movimiento : MonoBehaviour
{
    public float velRotacion = 100;
    public float velZoom = 5f;

    [Header("Camara")]
    public Transform camara;

    [Header("Limite de giro acumulado")]
    public float limiteRotacion = 1080f;

    private float acumuladoY = 0f;

    private bool yaFracturo = false;

    public Fractura fractureScript;

    void Start()
    {
        if (ConectorArduino.Instance != null)
        {
            ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
        }
    }

    void Update()
    {
        if (ConectorArduino.Instance == null || !ConectorArduino.Instance.IsConnected)
            return;

        var data = ConectorArduino.Instance.GetSensorData();

        float joyX = (data.JOYSTICK.X - 512f) / 512f;
        float joyY = (data.JOYSTICK.Y - 512f) / 512f;

        if (camara != null)
        {
            float rotY = -joyX * velRotacion * Time.deltaTime;
            float rotX = joyY * velRotacion * Time.deltaTime;

            // ROTACIÓN ESTABLE (como tu script que sí funcionaba)
            camara.RotateAround(transform.position, Vector3.up, rotY);
            camara.RotateAround(transform.position, camara.right, rotX);

            // ACUMULAR SOLO PARA LÍMITE
            acumuladoY += rotY;
        }

        // =========================
        // FRACTURA
        // =========================
        if (!yaFracturo && Mathf.Abs(acumuladoY) >= limiteRotacion)
        {
            yaFracturo = true;

            Debug.Log("FRACTURA POR GIRO ACUMULADO");

            if (fractureScript != null)
            {
                fractureScript.CauseFracture();
            }
        }

        // =========================
        // ZOOM
        // =========================
        if (float.TryParse(data.POT, out float pot))
        {
            float zoom = (pot - 512f) / 512f;

            if (camara != null)
            {
                camara.position += camara.forward * zoom * velZoom * Time.deltaTime;
            }
        }
    }
}