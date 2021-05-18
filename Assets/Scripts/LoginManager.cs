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
    public static bool isFirst = true;

    public static string nickname = "";
    public static LoadBalancingClient client;
    EnterRoomParams roomParams = new EnterRoomParams();
    EnterRoomParams froomParams = new EnterRoomParams();
    RoomOptions roomOptions = new RoomOptions();
    AppSettings appset = new AppSettings();
    class User
    {
        public string email;
        public string name;
        public string image;

        public User(string email, string name, string image)
        {
            this.email = email;
            this.name = name;
            this.image = image;
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

        //client = new LoadBalancingClient();
        //appset.AppIdRealtime = "3cdd2e0f-e063-42b9-96e1-6245fcc77a1a";
        //appset.AppVersion = gameVersion;
        //bool connectInProcess = client.ConnectUsingSettings(appset);
    }


    // ������ ������ ���ӵǸ� �ڵ� ����Ǵ� �Լ� 3���� ��� �� ����Ǽ� ������ �ڵ��� 1. �� ó�� ���� �� 2.�� �濡�� ������ ģ���� ã�ư��� 3.ģ���� ���ͼ� �������� ���ư� ��
    public override void OnConnectedToMaster()
    {
        Debug.Log(isFirst);
        connInfoText.text = "�����Ϳ� �����";
        Debug.Log("�����Ϳ� �����");
        
        if(GManager.fNickname != "") // 2�� ���, fNickname�� ���� ���̷뿡�� ���� ���� ģ�� �˻����� �� �� �г���
        {
            Debug.Log("ģ�������� �̵���");
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = 4;
            roomOptions.PlayerTtl = -1;
            roomOptions.EmptyRoomTtl = 10;

            //froomParams.RoomOptions = roomOptions;
            //froomParams.RoomName = FriendButtonManager.fNickname;
            //client.OpJoinRoom(froomParams);
            //froomParams.ExpectedUsers = null;
            //froomParams.Lobby = TypedLobby.Default;
            //froomParams.PlayerProperties = null;
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
        nickname = nicknameIF.text;
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 4;
        roomOptions.PlayerTtl = -1;
        roomOptions.EmptyRoomTtl = 10;

        //roomParams.RoomName = nickname;
        //roomParams.RoomOptions = roomOptions;
        //roomParams.ExpectedUsers = null;
        //roomParams.Lobby = TypedLobby.Default;
        //roomParams.PlayerProperties = null;
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
                        
                        connInfoText.text = "�������� ó�� �̵���";
                        PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);
                        
                        //client.OpJoinRoom(roomParams
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

    public void Join()
    {
        if (nicknameIF.text.Length == 0) // �г����� �Է��ؾ� ȸ������ ����
        {
            connInfoText.text = "�г����� �������ּ���";
            return;
        }
        if (idIF.text.Length != 0 && passwordIF.text.Length != 0) //ID�� ��й�ȣ ��� �Է��ؾ� ����
        {
            //DB�� ����� ���� �߰��ϴ� �κ�
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://react-firebase-chat-app-3b8de-default-rtdb.firebaseio.com/");
            reference = FirebaseDatabase.DefaultInstance.GetReference("users"); // users ��ġ ����
            
            reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    connInfoText.text = "ȸ������ ����";
                    Debug.Log("ȸ�����Կ� �����ϼ̽��ϴ�.");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result; // users�� ���� ����� snapshot���� �޾ƿ�
                    foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
                    {
                        IDictionary users = (IDictionary)data.Value;
                        if (users["name"].Equals(nicknameIF.text.ToString()))
                        {
                            connInfoText.text = "�ߺ��� �г���";
                            Debug.Log(nicknameIF.text + "�� �̹� �ִ� �г����Դϴ�.");
                            return;
                        }
                    }
                    Debug.Log("�г��� ��� ����");
                    auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread( task =>
                    {
                        if (!task.IsCanceled && !task.IsFaulted)
                        {
                            User user = new User(idIF.text, nicknameIF.text, "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon");
                            string userJson = JsonUtility.ToJson(user);
                            string userKey = reference.Push().Key;
                            reference.Child(userKey).SetRawJsonValueAsync(userJson);

                            connInfoText.text = "ȸ������ ����";
                            Debug.Log(idIF.text + " �� ȸ������ �ϼ̽��ϴ�.");

                            reference = FirebaseDatabase.DefaultInstance.GetReference("chatRooms"); // chatRooms ��ġ ����
                            /*string chatRoomJson = JsonUtility.ToJson(chatRoom);
                            string chatRoomKey = reference.Push().Key;
                            reference.Child(chatRoomKey).SetRawJsonValueAsync(chatRoomJson);*/
                            string chatRoomKey = reference.Push().Key;
                            ChatRoom chatRoom = new ChatRoom("http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", nicknameIF.text, "description", chatRoomKey, "name"); //TODO: ���� �� ������ nickname�� �ԷµǴ� ����, ���� ���� �ʿ�
                            var chatRoomJson = StringSerializationAPI.Serialize(typeof(ChatRoom), chatRoom);
                            reference.Child(chatRoomKey).SetRawJsonValueAsync(chatRoomJson);
                            Debug.Log("ä�ù� ���� �Ϸ�");
                        }
                        else
                        {
                            connInfoText.text = "ȸ������ ����";
                            Debug.Log("ȸ�����Կ� �����ϼ̽��ϴ�.");
                        }
                    });
                }
            });

            
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("�� ����� ����");
    }
    public override void OnJoinedRoom() // �� �г������� ���� ����� ������ ���
    {
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
