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
    // 전체적인 구조
    // 앱이 시작하면 마스터서버 접속 -> 마스터에 접속되면 자동으로 로비로 접속(로비에선 아무것도 안함, 기본 설정이라 들렀다 가는 느낌) ->  닉네임으로 방 만들고 자동 접속(여기가 마이룸)
    // 친구 닉네임 검색하면 내 방 유지하면서 나가고 마스터 서버 갔다가 친구 방 이동 -> 뒤로 버튼 누르면 친구 방 나와서 마스터 서버 갔다가 내 닉네임 방으로 돌아옴 
    public static bool isFirst = true;
    int roomIndex = 0;
    public static string nickname;
    RoomOptions roomOptions = new RoomOptions();


    public DatabaseReference reference { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); // 마스터 서버에 접속
        auth = FirebaseAuth.DefaultInstance;
    }


    // 마스터 서버에 접속되면 자동 실행되는 함수 3가지 경우 다 실행되서 나눠서 코딩함 1. 맨 처음 들어올 때 2.내 방에서 나가서 친구방 찾아갈때 3.친구방 나와서 내방으로 돌아갈 때
    public override void OnConnectedToMaster()
    {
        Debug.Log(isFirst);
        connInfoText.text = "환영합니다";

        if (GManager.fNickname != "") // 2번 경우, fNickname은 내가 마이룸에서 가고 싶은 친구 검색했을 때 그 닉네임
        {
            Debug.Log("친구방으로 이동중");
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = 4;
            roomOptions.PlayerTtl = -1;
            roomOptions.EmptyRoomTtl = 10;

            PhotonNetwork.JoinOrCreateRoom(GManager.fNickname, roomOptions, null);//fNickname이름과 같은 방으로 이동
        }
        if (isFirst == false) // 1,3번 경우, 지금은 3번 경우 오류가 나서 다시 내방으로 안돌아가짐(webhook 관련해서 다시 코딩 예정)
        {
            Debug.Log("내방으로 재접속중");
            PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);
        }

    }

    public override void OnDisconnected(DisconnectCause cause) // 1,2,3의 경우에 마스터 서버로 접속 못한 경우 자동 실행되는 함수(예외 처리용?)
    {

        connInfoText.text = "접속 실패";

        PhotonNetwork.ConnectUsingSettings();// 다시 마스터에 접속 시도
    }

    public void Connect() // 유니티에서 Login 버튼을 눌렀을 때 호출되는 함수
    {
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 4;
        roomOptions.PlayerTtl = -1;
        roomOptions.EmptyRoomTtl = 10;

        if (idIF.text.Length == 0 || passwordIF.text.Length == 0) // 닉네임 입력했는지 체크용(닉네임이 방 만들고 해서 필수이기 때문에)
        {
            connInfoText.text = "ID와 비밀번호를 입력해 주세요.";
            return;
        }
        else
        {
            // 입력받은 ID + 비밀번호로 로그인 인증 실행(비동기)
            auth.SignInWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(
            task =>
            {   
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled) // 문제 없이 Task 실행되었을 경우
                {
                    FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                    nickname = firebaseUser.DisplayName;
                    Debug.Log(firebaseUser.UserId);

                    connInfoText.text = "로그인 중";
                   
                    if (PhotonNetwork.IsConnected) //마스터에 접속되어 있다면
                    {
                        PhotonNetwork.NickName = nickname;
                        connInfoText.text = "방으로 이동중";
                        PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);
                    }
                    else //마스터에 접속 안되어 있다면
                    {
                        connInfoText.text = "접속 실패";
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
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패");
    }
    public override void OnJoinedRoom() // 내 닉네임으로 방을 만들고 들어왔을 경우
    {
        DatabaseAPI.roommaster = PhotonNetwork.CurrentRoom.Name;
        ChatHandler.roommaster = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("방 참가 성공");
       
        Debug.Log(PhotonNetwork.CurrentRoom.Name);

        PhotonNetwork.LoadLevel("Main");
        // Main 씬 불러옴(마이룸 씬)
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("플레이어 입장");
    }
    public override void OnJoinRoomFailed(short returnCode, string message) // 방에 들어가는걸 실패했을 경우
    {
        Debug.Log("방 참가 실패 : " + message);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 도착");
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
