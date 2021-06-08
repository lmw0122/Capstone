using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.UI;
// player�� �Ӽ����� �����ϴ� ��ũ��Ʈ 
public class PlayerManager : MonoBehaviourPun 
{
    public GameObject tempG;
    public Text nickText;
    public GameObject chatBox;
    public Animator animator;
    public float remainTime;
    private float rotationSpeed = 5.0f;
    
    
    private Vector3 movePosition;
    // Start is called before the first frame update
    void Start()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
        animator = GetComponent<Animator>();
        if(photonView.IsMine)
        {
            tempG = this.transform.GetChild(0).gameObject;
            nickText = tempG.transform.GetChild(0).GetComponent<Text>();
            chatBox = tempG.transform.GetChild(1).gameObject;
            nickText.text = PhotonNetwork.NickName;
            nickText.color = Color.blue;
        }
        else
        {
            tempG = this.transform.GetChild(0).gameObject;
            nickText = tempG.transform.GetChild(0).GetComponent<Text>();
            chatBox = tempG.transform.GetChild(1).gameObject;
            nickText.text = this.photonView.Owner.NickName;
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

    [PunRPC]
    public void showChat(string message)
    {
        int j = message.Length;

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

        chatBox.SetActive(true);
        chatBox.GetComponentInChildren<Text>().text = message;
        Debug.Log(chatBox.GetComponentInChildren<Text>().text);

        remainTime = 1800 * Time.deltaTime;
    }
    [PunRPC]
    public void sendRPC()
    {
        GameObject.Find("GameManager").GetComponent<GManager>().SendURL();
       
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
        if (photonView.IsMine) // �� �÷��̾ ���ؼ��� �ڵ��Ұ��̶� �������� �����Ѵ� 
        {
            // JoystickManager ��ũ��Ʈ���� ���� movePosition���� ����ϴµ� �װ� �� �����Ӹ��� �������ִ� �ڵ� 
            transform.position += JoystickManager.movePosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, JoystickManager.angle, 0), rotationSpeed * Time.deltaTime);

        }
        tempG.transform.GetChild(0).transform.position = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0, 2f, 0));
        chatBox.transform.position = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(-1f, 2.2f, 0));
            
    }

    

}
