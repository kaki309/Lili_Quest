using UnityEngine;

public class MovimientoCamara : MonoBehaviour
{
    private const float JOYSTICK_DEADZONE = 0.05f; // 5% de dead zone para evitar micromovimientos

    [Header("Movimiento de cámara")]
    [SerializeField] float velRotacion = 50f; // Aumentado para mejor respuesta
    [SerializeField] float velZoom = 5f;
    [SerializeField] float distanciaMin = 5;
    [SerializeField] float distanciaMax = 15f;

    [Header("Fractura de modelo (Si aplica)")]
    public bool activateFracture = false;
    [SerializeField] float limiteRotacion = 1080f;
    [SerializeField] ParticleSystem particulas;
    Transform objetivo;
    Vector3 puntoFijo;
    Fractura fractureScript;
    float acumulado = 0f;
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

        SensorData data = ConectorArduino.Instance.GetSensorData();

        // Mapeo de joystick: 0-1023 -> -1 a 1 (512 es neutral)
        float joyX = (data.JOYSTICK.X - 512f) / 512f;
        float joyY = (data.JOYSTICK.Y - 512f) / 512f;

        // Dead zone: ignorar valores muy pequeños para evitar micromovimientos
        if (Mathf.Abs(joyX) < JOYSTICK_DEADZONE) joyX = 0f;
        if (Mathf.Abs(joyY) < JOYSTICK_DEADZONE) joyY = 0f;

        // Calcular rotaciones (se invierte X para que sea intuitivo: joystick derecha = cámara gira izquierda)
        float rotY = -joyX * velRotacion * Time.deltaTime;
        float rotX = joyY * velRotacion * Time.deltaTime;

        // Aplicar rotaciones alrededor del punto objetivo
        transform.RotateAround(puntoFijo, Vector3.up, rotY);
        transform.RotateAround(puntoFijo, transform.right, rotX);

        // =========================
        // ZOOM (Potenciómetro: 0-1023 -> -1 a 1)
        // =========================
        if (float.TryParse(data.POT, out float pot))
        {
            float zoom = (pot - 512f) / 512f;

            // Dead zone para zoom también
            if (Mathf.Abs(zoom) < JOYSTICK_DEADZONE) zoom = 0f;

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

        acumulado += (Mathf.Abs(rotY) + Mathf.Abs(rotX)) / 1.5f;
        Debug.Log("Acumulado: " + acumulado);

        if (!yaFracturo && Mathf.Abs(acumulado) >= limiteRotacion)
        {
            if (fractureScript != null)
            {
                fractureScript.ApplyExplosionForce();
                particulas.Play();
                AudioClip audioFractura = GestorInterfazPantallasVisor3D.Instance.AudioFractura;
                AudioController.Instance.PlaySFX(audioFractura);
            }
            yaFracturo = true;
            ControladorFlujo.Instance.FragmentModel();
        }
    }

    public void SetObjetivo(GameObject obj)
    {
        objetivo = obj.transform;
        fractureScript = obj.GetComponent<Fractura>();
    }
}