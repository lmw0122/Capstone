using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPun 
{
    
    //public GameObject other;
    [SerializeField] private Text myName;
    
    private float rotationSpeed = 5.0f;

    
    private Vector3 movePosition;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            myName.text = LoginManager.nickname;
            myName.color = Color.blue;
        }
        else
        {
            myName.color = Color.red;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        else
        {
          
            transform.position += JoystickManager.movePosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, JoystickManager.angle, 0), rotationSpeed * Time.deltaTime);
            
        }
    }

    

}
