using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreateObject : MonoBehaviour
{
    public Camera mainCam;
    public Camera preCam;
    public GameObject createCanvas;
    public GameObject mainCanvas;
    public GameObject alertCanvas;
    public GameObject gameManager;
    GameObject toCreate;
    public static Vector3 toCreatePosition;
    

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Create start");
        toCreatePosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if(Physics.Raycast(ray, out hit))
            {
                Debug.Log("Position : " + hit.point);
                toCreatePosition = hit.point;
                alertCanvas.SetActive(true);
                createCanvas.SetActive(false);
            }
        }

    }
}

