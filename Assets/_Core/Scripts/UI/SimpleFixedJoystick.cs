using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class SimpleFixedJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Referências")]
    [SerializeField] private RectTransform background; // objeto filho Background
    [SerializeField] private RectTransform handle;     // objeto filho Handle

    [Header("Configurações")]
    [SerializeField] private float movementRange = 80f;   // raio máximo do handle
    [Range(0f, 0.9f)]
    [SerializeField] private float deadZone = 0.15f;

    private Gamepad virtualGamepad;

    private void Awake()
    {
        // Cria um gamepad virtual. O PlayerInput dos heróis lê isso
        // automaticamente através do binding <Gamepad>/leftStick.
        virtualGamepad = InputSystem.AddDevice<Gamepad>("VirtualGamepad");
    }

    private void OnDestroy()
    {
        if (virtualGamepad != null && virtualGamepad.added)
            InputSystem.RemoveDevice(virtualGamepad);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Nada a fazer aqui — o background já está visível e fixo.
        // Apenas garante que o handle comece no centro.
        handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Calcula onde o dedo está em relação ao CENTRO do background.
        // Como o background é estático, isso funciona de forma confiável.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera,
                out Vector2 localPoint))
            return;

        // Limita o deslocamento ao raio definido.
        Vector2 delta = Vector2.ClampMagnitude(localPoint, movementRange);
        handle.anchoredPosition = delta;

        // Normaliza para -1..1 e aplica deadzone.
        Vector2 inputVector = delta / movementRange;
        if (inputVector.magnitude < deadZone)
            inputVector = Vector2.zero;

        // Envia o estado para o gamepad virtual.
        InputSystem.QueueStateEvent(virtualGamepad, new GamepadState { leftStick = inputVector });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Zera o input e reseta a posição do handle.
        InputSystem.QueueStateEvent(virtualGamepad, new GamepadState { leftStick = Vector2.zero });
        handle.anchoredPosition = Vector2.zero;
    }
}