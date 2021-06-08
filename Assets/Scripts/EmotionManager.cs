using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class EmotionManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private RectTransform rect_Back;
    [SerializeField]
    private RectTransform rect_Joystick;

    public GManager gManager;

    private float radius;
    [SerializeField] private float moveSpeed;

    public static int num = 1;
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

        if (value.x >= 0)
        {
            if (value.y >= 0)
            {
                if (value.x >= value.y)
                    num = 1;
                else
                    num = 2;
            }
            else
            {
                if (value.x >= -(value.y))
                    num = 1;
                else
                    num = 4;
            }
        }
        else
        {
            if (value.y >= 0)
            {
                if (value.x >= -(value.y))
                    num = 2;
                else
                    num = 3;
            }
            else
            {
                if (value.x >= value.y)
                    num = 4;
                else
                    num = 3;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        rect_Joystick.localPosition = Vector3.zero;
        gManager.GetComponent<GManager>().SetAnimation("animation" + num);
    }
}
