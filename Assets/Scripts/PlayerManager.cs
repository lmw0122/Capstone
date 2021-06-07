using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.UI;
// player�� �Ӽ����� �����ϴ� ��ũ��Ʈ 
public class PlayerManager : MonoBehaviourPun 
{
    public GameObject nick;
    public Text nickText;
    public GameObject chatBox;
    public float remainTime;
    private float rotationSpeed = 5.0f;

    
    private Vector3 movePosition;
    // Start is called before the first frame update
    void Start()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
        nick = GameObject.Find("Canvas/Nickname");
        nickText = nick.GetComponent<Text>();
        chatBox = GameObject.Find("Canvas/Chat");

        if (photonView.IsMine) // �� �÷��̾ �� �÷��̾��� 
        {
            nickText.text = LoginManager.nickname;
            nickText.color = Color.blue;
        }
        else // ���� ������ �� �濡 �ִ� �÷��̾�鿡 ����
        {
            nickText.text = LoginManager.nickname;
            nickText.color = Color.green;
        }
        
        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
    }

    public void showChat(string message)
    {
        int j = message.Length;
        Debug.Log("message : " + message);
        Debug.Log("j : " + j);

        if (j >= 10)
        {
            for (int i = 0; i < j; i++)
            {
                if (i % 11 == 10)
                {
                    message = message.Insert(i, "\n");
                    j++;
                }
                
            }
        }

        Debug.Log("Message is : " + message);
        chatBox.SetActive(true);
        chatBox.GetComponentInChildren<Text>().text = message;
        Debug.Log(chatBox.GetComponentInChildren<Text>().text);

        remainTime = 1800 * Time.deltaTime;
    }
    [PunRPC]
    void sendRPC()
    {
        GameObject.Find("GameManager").GetComponent<GManager>().SendURL();
    }
    [PunRPC]
    void loadByRemote()
    {
        GameObject.Find("GameManager").GetComponent<GManager>().loadPrefabs();
    }
    // Update is called once per frame
    void Update()
    {
        if (remainTime > 0)
            remainTime = remainTime - Time.deltaTime;

        if(remainTime <= 0)
        {
            chatBox.SetActive(false);
            remainTime = 0;
        }
        if (!photonView.IsMine) // �� �÷��̾ ���ؼ��� �ڵ��Ұ��̶� �������� �����Ѵ� 
        {
            return;
        }
        else
        {
            nick.transform.position = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0, 5f, 0));
            chatBox.transform.position = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(-3f, 5f, 0));

            // JoystickManager ��ũ��Ʈ���� ���� movePosition���� ����ϴµ� �װ� �� �����Ӹ��� �������ִ� �ڵ� 
            transform.position += JoystickManager.movePosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, JoystickManager.angle, 0), rotationSpeed * Time.deltaTime);
            
        }
    }

    

}
