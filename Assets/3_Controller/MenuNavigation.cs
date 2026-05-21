using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    private const float DEADZONE = 0.25f;  // 25% de dead zone

    [Header("Botón inicial")]
    public Button firstButton;

    private bool lastButtonState = false;  // Para detectar transiciones del botón (edge detection)
    private bool canMoveVertical = true;
    private bool canMoveHorizontal = true;

    void Start()
    {
        // Inicializar lastButtonState como true para evitar ejecuciones accidentales
        // si el botón ya estaba presionado al entrar a esta pantalla.
        // Esto fuerza al usuario a soltar y volver a presionar para ejecutar una acción.
        lastButtonState = true;
    }
    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    void Update()
    {
        NavigateUI();
        SubmitButton();
    }

    void NavigateUI()
    {
        SensorData data = ConectorArduino.Instance.GetSensorData();

        // Mapeo de joystick: 0-1023 -> -1 a 1 (512 es neutral)
        float joyX = (data.JOYSTICK.X - 512f) / 512f;
        float joyY = (data.JOYSTICK.Y - 512f) / 512f;

        // Aplicar dead zone (40%)
        bool joyYInDeadZone = Mathf.Abs(joyY) < DEADZONE;
        bool joyXInDeadZone = Mathf.Abs(joyX) < DEADZONE;

        // ============================================
        // NAVEGACIÓN VERTICAL
        // ============================================
        if (canMoveVertical && !joyYInDeadZone)
        {
            if (joyY > 0)
            {
                MoveSelection(Vector3.up);
            }
            else if (joyY < 0)
            {
                MoveSelection(Vector3.down);
            }
        }

        // Reset cuando el joystick vuelve al dead zone
        if (joyYInDeadZone)
        {
            canMoveVertical = true;
        }

        // ============================================
        // NAVEGACIÓN HORIZONTAL
        // ============================================
        if (canMoveHorizontal && !joyXInDeadZone)
        {
            if (joyX > 0)
            {
                MoveSelection(Vector3.right);
            }
            else if (joyX < 0)
            {
                MoveSelection(Vector3.left);
            }
        }

        // Reset cuando el joystick vuelve al dead zone
        if (joyXInDeadZone)
        {
            canMoveHorizontal = true;
        }
    }

    void MoveSelection(Vector3 direction)
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == null)
            return;

        Selectable currentSelectable = current.GetComponent<Selectable>();

        Selectable next = null;

        if (direction == Vector3.up)
        {
            next = currentSelectable.FindSelectableOnUp();
            if (next != null) canMoveVertical = false;
        }
        else if (direction == Vector3.down)
        {
            next = currentSelectable.FindSelectableOnDown();
            if (next != null) canMoveVertical = false;
        }
        else if (direction == Vector3.right)
        {
            next = currentSelectable.FindSelectableOnRight();
            if (next != null) canMoveHorizontal = false;
        }
        else if (direction == Vector3.left)
        {
            next = currentSelectable.FindSelectableOnLeft();
            if (next != null) canMoveHorizontal = false;
        }

        if (next != null)
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
        }
    }

    void SubmitButton()
    {
        SensorData data = ConectorArduino.Instance.GetSensorData();

        bool currentButtonState = data.ButtonPressed;

        // GetKeyDown ya hace edge detection automáticamente
        bool isPressed = Input.GetKeyDown(KeyCode.Return);

        // Edge detection para Arduino: solo ejecutar cuando detectamos transición (no presionado → presionado)
        if (currentButtonState && !lastButtonState)
        {
            isPressed = true;
        }

        if (isPressed)
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null)
            {
                Button button = current.GetComponent<Button>();

                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
        }

        // Guardar estado actual para la próxima iteración
        lastButtonState = currentButtonState;
    }
}