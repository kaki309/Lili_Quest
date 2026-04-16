using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCamara : MonoBehaviour
{
    [Header("Movimiento de cámara")]
    [SerializeField] float velRotacion = 1;
    [SerializeField] float velZoom = 5f;
    [SerializeField] float distanciaMin = 5;
    [SerializeField] float distanciaMax = 15f;

    [Header("Fractura de modelo (Si aplica)")]
    [SerializeField] bool activateFracture = false;
    [SerializeField] float limiteRotacion = 1080f;
    Transform objetivo;
    Vector3 puntoFijo;
    Fractura fractureScript;
    public float acumuladoY = 0f;
    bool yaFracturo = false;

    void Start()
    {
        if (objetivo != null)
        {
            puntoFijo = objetivo.position;
        }
    }

    void Update()
    {
        if (ConectorArduino.Instance == null || !ConectorArduino.Instance.IsConnected) return;

        var data = ConectorArduino.Instance.GetSensorData();

        float joyX = data.JOYSTICK.X;
        float joyY = data.JOYSTICK.Y;

        // DATOS DE ARDUINO REAL (Supuestamente)
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

        // =========================
        // FRACTURA
        // =========================
        if (!activateFracture) return;

        if (!yaFracturo && Mathf.Abs(acumuladoY) >= limiteRotacion)
        {
            if (fractureScript != null)
            {
                fractureScript.ApplyExplosionForce();
            }
            yaFracturo = true;
            ControladorFlujo.Instance.SetModelFragmentedState(true);
        }
    }

    public void SetObjetivo(GameObject obj)
    {
        objetivo = obj.transform;
        fractureScript = obj.GetComponent<Fractura>();
    }
}