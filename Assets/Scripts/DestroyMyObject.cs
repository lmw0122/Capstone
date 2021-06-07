using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMyObject : MonoBehaviour
{
    public Camera mainCam;
    public Camera preCam;
    public GameObject destroyCanvas;
    public GameObject mainCanvas;
    public GameObject gameManager;
    public static GameObject toDestroy;
    public static Vector3 toDestroyPosition;
    public GameObject alertCanvas;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Destroy start");
    }
    public GameObject getToDestroy()
    {
        return toDestroy;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Position : " + hit.point);
                toDestroy = hit.collider.gameObject;
                Debug.Log("toDestroy name : " + toDestroy.name);
                destroyCanvas.SetActive(false);
                alertCanvas.SetActive(true);
            }
        }
    }
}
