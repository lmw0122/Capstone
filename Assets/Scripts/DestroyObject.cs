using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public Camera mainCam;
    public Camera preCam;
    public GameObject destroyCanvas;
    public GameObject mainCanvas;
    public GameObject gameManager;
    GameObject toDestroy;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Destroy start");
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
                if(toDestroy.tag == "prefabs")
                {
                    Destroy(toDestroy);
                    gameManager.GetComponent<GManager>().savePrefabs();
                    preCam.enabled = false;
                    mainCam.enabled = true;
                    mainCanvas.SetActive(true);
                    destroyCanvas.SetActive(false);

                }


            }
        }
    }
}
