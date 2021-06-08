using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.UI;
// player의 속성들을 설정하는 스크립트 
public class PlayerManager : MonoBehaviourPun 
{
    public GameObject tempG;
    public Text nickText;
    public GameObject chatBox;
    public float remainTime;
    private float rotationSpeed = 5.0f;

    
    private Vector3 movePosition;
    // Start is called before the first frame update
    void Start()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
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

    [PunRPC]
    void setName()
    {
        if(!photonView.IsMine)
        {
            
            nickText.color = Color.cyan;
        }
        
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
        if (photonView.IsMine)
        {
            transform.position += JoystickManager.movePosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, JoystickManager.angle, 0), rotationSpeed * Time.deltaTime);

            
        }
        tempG.transform.GetChild(0).transform.position = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0, 5f, 0));
        chatBox.transform.position = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(-3f, 6f, 0));
        // JoystickManager 스크립트에서 계산된 movePosition값을 계산하는데 그걸 매 프레임마다 적용해주는 코드 


    }

    

}
