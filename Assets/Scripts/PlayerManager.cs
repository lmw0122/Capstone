using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.UI;
// player�� �Ӽ����� �����ϴ� ��ũ��Ʈ 
public class PlayerManager : MonoBehaviourPun 
{
    
    //public GameObject other;
    [SerializeField] private Text myName;
    
    private float rotationSpeed = 5.0f;

    
    private Vector3 movePosition;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine) // �� �÷��̾ �� �÷��̾��� 
        {
            myName.text = LoginManager.nickname;
            myName.color = Color.blue;
        }
        else // ���� ������ �� �濡 �ִ� �÷��̾�鿡 ����
        {
            myName.text = LoginManager.nickname;
            myName.color = Color.red;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) // �� �÷��̾ ���ؼ��� �ڵ��Ұ��̶� �������� �����Ѵ� 
        {
            return;
        }
        else
        {
            // JoystickManager ��ũ��Ʈ���� ���� movePosition���� ����ϴµ� �װ� �� �����Ӹ��� �������ִ� �ڵ� 
            transform.position += JoystickManager.movePosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, JoystickManager.angle, 0), rotationSpeed * Time.deltaTime);
            
        }
    }

    

}
