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
    // ��ü���� ����
    // ���� �����ϸ� �����ͼ��� ���� -> �����Ϳ� ���ӵǸ� �ڵ����� �κ�� ����(�κ񿡼� �ƹ��͵� ����, �⺻ �����̶� �鷶�� ���� ����) ->  �г������� �� ����� �ڵ� ����(���Ⱑ ���̷�)
    // ģ�� �г��� �˻��ϸ� �� �� �����ϸ鼭 ������ ������ ���� ���ٰ� ģ�� �� �̵� -> �ڷ� ��ư ������ ģ�� �� ���ͼ� ������ ���� ���ٰ� �� �г��� ������ ���ƿ� 
    private bool isFirst = true;
    public static string nickname = ""; //�г������� ���̷� ������ �� �̸��̰� ä���Ҷ� ������ �̸�
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); // ������ ������ ����

        
        connInfoText.text = "�����Ϳ� ������";
    }

    // ������ ������ ���ӵǸ� �ڵ� ����Ǵ� �Լ� 3���� ��� �� ����Ǽ� ������ �ڵ��� 1. �� ó�� ���� �� 2.�� �濡�� ������ ģ���� ã�ư��� 3.ģ���� ���ͼ� �������� ���ư� ��
    public override void OnConnectedToMaster() 
    {
        
        connInfoText.text = "�����Ϳ� �����";
        Debug.Log("�����Ϳ� �����");
        if(FriendButtonManager.fNickname != "") // 2�� ���, fNickname�� ���� ���̷뿡�� ���� ���� ģ�� �˻����� �� �� �г���
        {
            Debug.Log("ģ�������� �̵���");
            PhotonNetwork.JoinRoom(FriendButtonManager.fNickname); //fNickname�̸��� ���� ������ �̵�
        }
        if(nickname != "") // 1,3�� ���, ������ 3�� ��� ������ ���� �ٽ� �������� �ȵ��ư���(webhook �����ؼ� �ٽ� �ڵ� ����)
        {
            Debug.Log("�������� �̵���");
            PhotonNetwork.JoinRoom(nickname); // �� �г��� �̸��� ������ �̵�
        }
    }

    public override void OnDisconnected(DisconnectCause cause) // 1,2,3�� ��쿡 ������ ������ ���� ���� ��� �ڵ� ����Ǵ� �Լ�(���� ó����?)
    {
        
        connInfoText.text = "�����Ϳ� ���� ����";

        PhotonNetwork.ConnectUsingSettings();// �ٽ� �����Ϳ� ���� �õ�
    }

    public void Connect() // ����Ƽ���� Login ��ư�� ������ �� ȣ��Ǵ� �Լ�
    {
        if (nicknameIF.text == "") // �г��� �Է��ߴ��� üũ��(�г����� �� ����� �ؼ� �ʼ��̱� ������)
        {
            connInfoText.text = "�г����� �������ּ���";
            return;
        }
        else
        {
            nickname = nicknameIF.text;
            if (PhotonNetwork.IsConnected) //�����Ϳ� ���ӵǾ� �ִٸ�
            {
                connInfoText.text = "�뿡 ������";
                PhotonNetwork.CreateRoom(nickname, new RoomOptions { MaxPlayers = 4 }); // �� �г������� ���� ����� ���� �� �ִ� �ִ� �ο����� 4���̴�. -> �� ���Ŀ� ����
                Debug.Log("�� ������");
            }
            else //�����Ϳ� ���� �ȵǾ� �ִٸ� 
            {
                connInfoText.text = "�����Ϳ� ���� ����";
                PhotonNetwork.ConnectUsingSettings(); //�����Ϳ� ���� �õ�
            }
        }
    }

    public override void OnJoinedRoom() // �� �г������� ���� ����� ������ ���
    {
        connInfoText.text = "�� ���� ����";
       
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        
        PhotonNetwork.LoadLevel("Main"); // Main �� �ҷ���(���̷� ��)
    }
    public override void OnJoinRoomFailed(short returnCode, string message) // �濡 ���°� �������� ���
    {
        
        if(nickname != "") // �ذ���
        {
            Debug.Log("���ƿ��� ����");
            
        }
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
