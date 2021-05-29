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

    GameObject toCreate; 

    

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Create start");
        toCreate = GManager.toCreate;
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
                Instantiate(GManager.toCreate, new Vector3(hit.point.x, 0.1f, hit.point.z), Quaternion.identity);
                preCam.enabled = false;
                mainCam.enabled = true;
                mainCanvas.SetActive(true);
                createCanvas.SetActive(false);
                GManager.toCreate = null;

            }
        }

    }
}
