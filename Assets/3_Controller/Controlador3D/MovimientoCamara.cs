using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCamara : MonoBehaviour
{
    public float velRotacion = 1;
    public float velZoom = 5f;

    [Header("Objeto objetivo")]
    public Transform objetivo;

    [Header("Limite de giro acumulado")]
    public float limiteRotacion = 1080f;

    [Header("Distancia de zoom")]
    public float distanciaMin = 5;
    public float distanciaMax = 15f;

    private float acumuladoY = 0f;
    private bool yaFracturo = false;

    private Vector3 puntoFijo;

    public Fractura fractureScript;

    void Start()
    {
        if (ConectorArduino.Instance != null)
        {
            ConectorArduino.Instance.RequestState(ArduinoState.LeyendoDatos);
        }

        if (objetivo != null)
        {
            puntoFijo = objetivo.position;
        }
    }

    void Update()
    {
        if (ConectorArduino.Instance == null || !ConectorArduino.Instance.IsConnected)
            return;

        var data = ConectorArduino.Instance.GetSensorData();

        float joyX = data.JOYSTICK.X;
        float joyY = data.JOYSTICK.Y;

        // DATOS DE ARUIDNO REAL (Supuestamente)
        //float joyX = (data.JOYSTICK.X - 512f) / 512f;
        //float joyY = (data.JOYSTICK.Y - 512f) / 512f;


        ///DATOS PARA EL SIULADOR
        float rotY = -joyX * velRotacion * Time.deltaTime;
        float rotX = joyY * velRotacion * Time.deltaTime;

        //  LA CÁMARA SE MUEVE 
        transform.RotateAround(puntoFijo, Vector3.up, rotY);
        transform.RotateAround(puntoFijo, transform.right, rotX);

        acumuladoY += rotY;

        // =========================
        // FRACTURA
        // =========================
        if (!yaFracturo && Mathf.Abs(acumuladoY) >= limiteRotacion)
        {
            yaFracturo = true;

            Debug.Log("FRACTURA POR GIRO");

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

            // Dirección hacia el objetivo
            Vector3 direccion = (puntoFijo - transform.position).normalized;

            float distancia = Vector3.Distance(transform.position, puntoFijo);

            // Limitar zoom para no atravesar el objeto
            if ((zoom > 0 && distancia > distanciaMin) ||
                (zoom < 0 && distancia < distanciaMax))
            {
                transform.position += direccion * zoom * velZoom * Time.deltaTime;
            }
        }
    }
}