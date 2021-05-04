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
        connInfoText.text = "마스터에 접속중";
    }

    public override void OnConnectedToMaster()
    {
        
        connInfoText.text = "마스터에 연결됨";
        Debug.Log("마스터에 연결됨");
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
        
        connInfoText.text = "마스터에 접속 실패";

        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        if (nicknameIF.text == "")
        {
            connInfoText.text = "닉네임을 설정해주세요";
            return;
        }
        else
        {
            nickname = nicknameIF.text;
            // 입력받은 ID + 비밀번호로 로그인 인증 실행(비동기)
            auth.SignInWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
            task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled) // 문제 없이 Task 실행되었을 경우
                {
                    if (PhotonNetwork.IsConnected)
                    {
                        connInfoText.text = "룸에 접속중";
                        PhotonNetwork.CreateRoom(nickname, new RoomOptions { MaxPlayers = 4 });
                        Debug.Log("방 만듬");
                        //PhotonNetwork.JoinRoom(nickname);
                        Debug.Log("방에 들어옴");
                    }
                    else
                    {
                        connInfoText.text = "마스터에 접속 실패";
                        PhotonNetwork.ConnectUsingSettings();
                    }
                    Debug.Log(idIF.text + " 로 로그인 하셨습니다.");
                }
                else // 로그인 실패한 경우
                {
                    connInfoText.text = "ID와 비밀번호를 확인해 주세요.";
                    Debug.Log("로그인에 실패하셨습니다.");
                }
            }
        );
        }
    }

    public void Join()
    {
        if (nicknameIF.text.Length == 0) // 닉네임을 입력해야 회원가입 가능
        {
            connInfoText.text = "닉네임을 설정해주세요";
            return;
        }
        if (idIF.text.Length != 0 && passwordIF.text.Length != 0) //ID와 비밀번호 모두 입력해야 실행
        {
            
            auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
             task =>
             {
                 if (!task.IsCanceled && !task.IsFaulted)
                 {
                     //DB에 사용자 정보 추가하는 부분
                     FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://react-firebase-chat-app-3b8de-default-rtdb.firebaseio.com/");
                     reference = FirebaseDatabase.DefaultInstance.RootReference;

                     User user = new User(idIF.text, nicknameIF.text);
                     string json = JsonUtility.ToJson(user);
                     string key = reference.Child("users").Push().Key;
                     reference.Child("users").Child(key).SetRawJsonValueAsync(json);

                     connInfoText.text = "회원가입 성공";
                     Debug.Log(idIF.text + " 로 회원가입 하셨습니다.");
                 }
                 else
                 {
                     connInfoText.text = "회원가입 실패";
                     Debug.Log("회원가입에 실패하셨습니다.");
                 }
             });
        }
    }

    public override void OnJoinedRoom()
    {
        connInfoText.text = "방 참가 성공";
        PhotonNetwork.LoadLevel("Main");
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connInfoText.text = "방 생성 중";
        PhotonNetwork.CreateRoom(nicknameIF.text, new RoomOptions { MaxPlayers = 4 });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
