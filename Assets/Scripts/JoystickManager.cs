using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class JoystickManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private RectTransform rect_Back;
    [SerializeField]
    private RectTransform rect_Joystick;

    private float radius;
    public static bool isTouch = false;
    [SerializeField] private float moveSpeed;
    public static Vector3 movePosition;
    public static float angle;
    // Start is called before the first frame update
    void Start()
    {
        radius = rect_Back.rect.width * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnDrag(PointerEventData eventData)
    {
        
        Vector2 value = eventData.position - (Vector2)rect_Back.position;

        value = Vector2.ClampMagnitude(value, radius);
        rect_Joystick.localPosition = value;

        float distance = Vector2.Distance(rect_Back.position, rect_Joystick.position) / radius * 0.1f;
        value = value.normalized;
        movePosition = new Vector3(value.x * moveSpeed * distance * Time.deltaTime, 0f, value.y * moveSpeed * distance * Time.deltaTime);
        Vector3 direction = new Vector3(value.x, 0, value.y);
        if (direction != Vector3.zero)
        {
            angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouch = true;
        

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouch = false;
        rect_Joystick.localPosition = Vector3.zero;
        movePosition = Vector3.zero;
    }
}
