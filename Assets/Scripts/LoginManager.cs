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

public class LoginManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";
    [SerializeField]
    public Text connInfoText;
    [SerializeField]
    public Button loginBtn;
    [SerializeField]
    public InputField nicknameIF;
    [SerializeField]
    public InputField idIF;
    [SerializeField]
    public InputField passwordIF;

    public static FirebaseAuth auth;
    // ��ü���� ����
    // ���� �����ϸ� �����ͼ��� ���� -> �����Ϳ� ���ӵǸ� �ڵ����� �κ�� ����(�κ񿡼� �ƹ��͵� ����, �⺻ �����̶� �鷶�� ���� ����) ->  �г������� �� ����� �ڵ� ����(���Ⱑ ���̷�)
    // ģ�� �г��� �˻��ϸ� �� �� �����ϸ鼭 ������ ������ ���� ���ٰ� ģ�� �� �̵� -> �ڷ� ��ư ������ ģ�� �� ���ͼ� ������ ���� ���ٰ� �� �г��� ������ ���ƿ� 
    private bool isFirst = true;

    public static string nickname = "";

    class User
    {
        public string email;
        public string name;
        public string image;

        public User(string email, string name)
        {
            this.email = email;
            this.name = name;
        }
    }
    public DatabaseReference reference { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); // ������ ������ ����
        auth = FirebaseAuth.DefaultInstance;
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
        if(nickname != "" && isFirst == false) // 1,3�� ���, ������ 3�� ��� ������ ���� �ٽ� �������� �ȵ��ư���(webhook �����ؼ� �ٽ� �ڵ� ����)
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
            // �Է¹��� ID + ��й�ȣ�� �α��� ���� ����(�񵿱�)
            auth.SignInWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
            task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled) // ���� ���� Task ����Ǿ��� ���
                {
                    if (PhotonNetwork.IsConnected) //�����Ϳ� ���ӵǾ� �ִٸ�
                    {
                        RoomOptions roomOptions = new RoomOptions();
                        roomOptions.IsOpen = true;
                        roomOptions.MaxPlayers = 4;
                        roomOptions.PlayerTtl = 1000000;
                        roomOptions.EmptyRoomTtl = 5000;

                        connInfoText.text = "�뿡 ������";
                        PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null); // �� �г������� ���� ����� ���� �� �ִ� �ִ� �ο����� 4���̴�. -> �� ���Ŀ� ����
                        Debug.Log("�� ����");
                        //PhotonNetwork.JoinRoom(nickname);
                        Debug.Log("�濡 ����");
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

    public void Join()
    {
        if (nicknameIF.text.Length == 0) // �г����� �Է��ؾ� ȸ������ ����
        {
            connInfoText.text = "�г����� �������ּ���";
            return;
        }
        if (idIF.text.Length != 0 && passwordIF.text.Length != 0) //ID�� ��й�ȣ ��� �Է��ؾ� ����
        {
            
            auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
             task =>
             {
                 if (!task.IsCanceled && !task.IsFaulted)
                 {
                     //DB�� ����� ���� �߰��ϴ� �κ�
                     FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://react-firebase-chat-app-3b8de-default-rtdb.firebaseio.com/");
                     reference = FirebaseDatabase.DefaultInstance.RootReference;

                     User user = new User(idIF.text, nicknameIF.text);
                     string json = JsonUtility.ToJson(user);
                     string key = reference.Child("users").Push().Key;
                     reference.Child("users").Child(key).SetRawJsonValueAsync(json);

                     connInfoText.text = "ȸ������ ����";
                     Debug.Log(idIF.text + " �� ȸ������ �ϼ̽��ϴ�.");
                 }
                 else
                 {
                     connInfoText.text = "ȸ������ ����";
                     Debug.Log("ȸ�����Կ� �����ϼ̽��ϴ�.");
                 }
             });
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
            Debug.Log(message);
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connInfoText.text = "�� ���� ��";

        PhotonNetwork.CreateRoom(nicknameIF.text, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ����");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
