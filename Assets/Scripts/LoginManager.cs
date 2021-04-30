using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Auth;
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
            auth.SignInWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
            task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
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
                else
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
        if (idIF.text.Length != 0 && passwordIF.text.Length != 0)
        {
            auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
             task =>
             {
                 if (!task.IsCanceled && !task.IsFaulted)
                 {
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
