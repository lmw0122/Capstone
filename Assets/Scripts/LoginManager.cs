using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEditor;
using ExitGames.Client.Photon;
using CreateValidateJWT;

public class LoginManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";
    [SerializeField]
    public Text connInfoText;
    [SerializeField]
    public InputField idIF;
    [SerializeField]
    public InputField passwordIF;

    public static FirebaseAuth auth;
    // ��ü���� ����
    // ���� �����ϸ� �����ͼ��� ���� -> �����Ϳ� ���ӵǸ� �ڵ����� �κ�� ����(�κ񿡼� �ƹ��͵� ����, �⺻ �����̶� �鷶�� ���� ����) ->  �г������� �� ����� �ڵ� ����(���Ⱑ ���̷�)
    // ģ�� �г��� �˻��ϸ� �� �� �����ϸ鼭 ������ ������ ���� ���ٰ� ģ�� �� �̵� -> �ڷ� ��ư ������ ģ�� �� ���ͼ� ������ ���� ���ٰ� �� �г��� ������ ���ƿ� 
    public static bool isFirst = true;
    int roomIndex = 0;
    public static string nickname = "";
    public static LoadBalancingClient client;
    EnterRoomParams roomParams = new EnterRoomParams();
    EnterRoomParams froomParams = new EnterRoomParams();
    RoomOptions roomOptions = new RoomOptions();
    AppSettings appset = new AppSettings();
    public DatabaseReference reference { get; set; }

    public class UserInfo
    {
        public string email;
        public string password;

        public UserInfo(string tempEmail, string tempPassword)
        {
            this.email = tempEmail;
            this.password = tempPassword;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); // ������ ������ ����
        auth = FirebaseAuth.DefaultInstance;
    }


    // ������ ������ ���ӵǸ� �ڵ� ����Ǵ� �Լ� 3���� ��� �� ����Ǽ� ������ �ڵ��� 1. �� ó�� ���� �� 2.�� �濡�� ������ ģ���� ã�ư��� 3.ģ���� ���ͼ� �������� ���ư� ��
    public override void OnConnectedToMaster()
    {
        Debug.Log(isFirst);
        //connInfoText.text = "�����Ϳ� �����";
        connInfoText.text = "�����Ϳ� �����";
        
        if(GManager.fNickname != "") // 2�� ���, fNickname�� ���� ���̷뿡�� ���� ���� ģ�� �˻����� �� �� �г���
        {
            Debug.Log("ģ�������� �̵���");
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = 4;
            roomOptions.PlayerTtl = -1;
            roomOptions.EmptyRoomTtl = 10;

            PhotonNetwork.JoinOrCreateRoom(GManager.fNickname, roomOptions, null);//fNickname�̸��� ���� ������ �̵�
        }
        if(isFirst == false) // 1,3�� ���, ������ 3�� ��� ������ ���� �ٽ� �������� �ȵ��ư���(webhook �����ؼ� �ٽ� �ڵ� ����)
        {
            Debug.Log("�������� ��������");
            PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);
            //PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null); // �� �г��� �̸��� ������ �̵�
        }
        
    }

    public override void OnDisconnected(DisconnectCause cause) // 1,2,3�� ��쿡 ������ ������ ���� ���� ��� �ڵ� ����Ǵ� �Լ�(���� ó����?)
    {
        
        connInfoText.text = "�����Ϳ� ���� ����";

        PhotonNetwork.ConnectUsingSettings();// �ٽ� �����Ϳ� ���� �õ�
    }

    public void Connect() // ����Ƽ���� Login ��ư�� ������ �� ȣ��Ǵ� �Լ�
    {
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 4;
        roomOptions.PlayerTtl = -1;
        roomOptions.EmptyRoomTtl = 10;

        if (idIF.text.Length == 0 || passwordIF.text.Length == 0) // �г��� �Է��ߴ��� üũ��(�г����� �� ����� �ؼ� �ʼ��̱� ������)
        {
            connInfoText.text = "ID�� ��й�ȣ�� �Է��� �ּ���.";
            return;
        }
        else
        {
            // �Է¹��� ID + ��й�ȣ�� �α��� ���� ����(�񵿱�)
            auth.SignInWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
            task =>
            {
                connInfoText.text = "�α�����";
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled) // ���� ���� Task ����Ǿ��� ���
                {
                    FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                    nickname = firebaseUser.DisplayName;
                    Debug.Log("nickname is : " + nickname);
                    FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
                    reference = FirebaseDatabase.DefaultInstance.GetReference("auto/" + nickname); // prefabs ��ġ ����
                    UserInfo tempUser = new UserInfo(idIF.text,passwordIF.text);
                    string json = JsonUtility.ToJson(tempUser);
                    reference.SetRawJsonValueAsync(json);

                    if (PhotonNetwork.IsConnected) //�����Ϳ� ���ӵǾ� �ִٸ�
                    {
                        PhotonNetwork.NickName = nickname;
                        connInfoText.text = "�������� ó�� �̵���";
                        PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);
                        //PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null); // �� �г������� ���� ����� ���� �� �ִ� �ִ� �ο����� 4���̴�. -> �� ���Ŀ� ����
                    }
                    else //�����Ϳ� ���� �ȵǾ� �ִٸ�
                    {
                        connInfoText.text = "�����Ϳ� ���� ����";
                        PhotonNetwork.ConnectUsingSettings();
                    }
                    Debug.Log(idIF.text + " �� �α��� �ϼ̽��ϴ�.");
                }
                else // �α��� ������ ���
                {
                    connInfoText.text = "ID�� ��й�ȣ�� Ȯ���� �ּ���.";
                    Debug.Log("�α��ο� �����ϼ̽��ϴ�.");
                }
            }
        );
        }
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("�� ����� ����");
    }
    public override void OnJoinedRoom() // �� �г������� ���� ����� ������ ���
    {
        DatabaseAPI.roommaster = PhotonNetwork.CurrentRoom.Name;
        ChatHandler.roommaster = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("�� ���� ����");
       
        Debug.Log(PhotonNetwork.CurrentRoom.Name);

        PhotonNetwork.LoadLevel("Main");// Main �� �ҷ���(���̷� ��)
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("�÷��̾� ����");
    }
    public override void OnJoinRoomFailed(short returnCode, string message) // �濡 ���°� �������� ���
    {
        Debug.Log("�� ���� ���� : " + message);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ����");
    }
    // Update is called once per frame
    void Update()
    {
        //client.Service();
    }
    private void OnApplicationQuit()
    {
        //client.Disconnect();
    }

}
