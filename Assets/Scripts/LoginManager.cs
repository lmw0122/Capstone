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
        PhotonNetwork.ConnectUsingSettings();
        auth = FirebaseAuth.DefaultInstance;
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
            // �Է¹��� ID + ��й�ȣ�� �α��� ���� ����(�񵿱�)
            auth.SignInWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
            task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled) // ���� ���� Task ����Ǿ��� ���
                {
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
