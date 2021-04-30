using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LoginManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";
    [SerializeField]
    public Text connInfoText;
    [SerializeField]
    public Button loginBtn;
    [SerializeField]
    public InputField nicknameIF;

    public static string nickname = "";
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        
        connInfoText.text = "�����Ϳ� ������";
    }

    public override void OnConnectedToMaster()
    {
        
        connInfoText.text = "�����Ϳ� �����";
        Debug.Log("�����Ϳ� �����");
        if(FriendButtonManager.fNickname != "")
        {
            PhotonNetwork.JoinRoom(FriendButtonManager.fNickname);
        }
        else if(nickname != "")
        {
            PhotonNetwork.JoinRoom(nickname);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        
        connInfoText.text = "�����Ϳ� ���� ����";

        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        if (nicknameIF.text == "")
        {
            connInfoText.text = "�г����� �������ּ���";
            return;
        }
        else
        {
            nickname = nicknameIF.text;
            if (PhotonNetwork.IsConnected)
            {
                connInfoText.text = "�뿡 ������";
                PhotonNetwork.CreateRoom(nickname, new RoomOptions { MaxPlayers = 4 });
                Debug.Log("�� ����");
                //PhotonNetwork.JoinRoom(nickname);
                Debug.Log("�濡 ����");
            }
            else
            {
                connInfoText.text = "�����Ϳ� ���� ����";
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public override void OnJoinedRoom()
    {
        connInfoText.text = "�� ���� ����";
        PhotonNetwork.LoadLevel("Main");
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connInfoText.text = "�� ���� ��";
        PhotonNetwork.CreateRoom(nicknameIF.text, new RoomOptions { MaxPlayers = 4 });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
