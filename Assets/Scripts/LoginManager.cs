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
    // 전체적인 구조
    // 앱이 시작하면 마스터서버 접속 -> 마스터에 접속되면 자동으로 로비로 접속(로비에선 아무것도 안함, 기본 설정이라 들렀다 가는 느낌) ->  닉네임으로 방 만들고 자동 접속(여기가 마이룸)
    // 친구 닉네임 검색하면 내 방 유지하면서 나가고 마스터 서버 갔다가 친구 방 이동 -> 뒤로 버튼 누르면 친구 방 나와서 마스터 서버 갔다가 내 닉네임 방으로 돌아옴 
    private bool isFirst = true;
    public static string nickname = ""; //닉네임이자 마이룸 마다의 방 이름이고 채팅할때 나오는 이름
    void Start()
    {
        
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); // 마스터 서버에 접속

        
        connInfoText.text = "마스터에 접속중";
    }

    // 마스터 서버에 접속되면 자동 실행되는 함수 3가지 경우 다 실행되서 나눠서 코딩함 1. 맨 처음 들어올 때 2.내 방에서 나가서 친구방 찾아갈때 3.친구방 나와서 내방으로 돌아갈 때
    public override void OnConnectedToMaster() 
    {
        
        connInfoText.text = "마스터에 연결됨";
        Debug.Log("마스터에 연결됨");
        
        if(FriendButtonManager.fNickname != "") // 2번 경우, fNickname은 내가 마이룸에서 가고 싶은 친구 검색했을 때 그 닉네임
        {
            Debug.Log("친구방으로 이동중");
            PhotonNetwork.JoinRoom(FriendButtonManager.fNickname); //fNickname이름과 같은 방으로 이동
        }
        if(nickname != "" && isFirst == false) // 1,3번 경우, 지금은 3번 경우 오류가 나서 다시 내방으로 안돌아가짐(webhook 관련해서 다시 코딩 예정)
        {
            Debug.Log("내방으로 이동중");
            PhotonNetwork.JoinRoom(nickname); // 내 닉네임 이름의 방으로 이동
        }
    }

    public override void OnDisconnected(DisconnectCause cause) // 1,2,3의 경우에 마스터 서버로 접속 못한 경우 자동 실행되는 함수(예외 처리용?)
    {
        
        connInfoText.text = "마스터에 접속 실패";

        PhotonNetwork.ConnectUsingSettings();// 다시 마스터에 접속 시도
    }

    public void Connect() // 유니티에서 Login 버튼을 눌렀을 때 호출되는 함수
    {
        if (nicknameIF.text == "") // 닉네임 입력했는지 체크용(닉네임이 방 만들고 해서 필수이기 때문에)
        {
            connInfoText.text = "닉네임을 설정해주세요";
            return;
        }
        else
        {
            
            nickname = nicknameIF.text;
            if (PhotonNetwork.IsConnected) //마스터에 접속되어 있다면
            {
                isFirst = false;
                connInfoText.text = "룸에 접속중";
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.IsOpen = true;
                roomOptions.MaxPlayers = 4;
                roomOptions.PlayerTtl = 1000000;
                roomOptions.EmptyRoomTtl = 5000;
   
                PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions,null); // 내 닉네임으로 방을 만들고 들어올 수 있는 최대 인원수는 4명이다. -> 그 이후에 접속
                Debug.Log("방 생성됨");
               
            }
            else //마스터에 접속 안되어 있다면 
            {
                connInfoText.text = "마스터에 접속 실패";
                PhotonNetwork.ConnectUsingSettings(); //마스터에 접속 시도
            }
        }
    }

    public override void OnJoinedRoom() // 내 닉네임으로 방을 만들고 들어왔을 경우
    {
        connInfoText.text = "방 참가 성공";
       
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        
        PhotonNetwork.LoadLevel("Main"); // Main 씬 불러옴(마이룸 씬)
    }
    public override void OnJoinRoomFailed(short returnCode, string message) // 방에 들어가는걸 실패했을 경우
    {
        
        if(nickname != "") // 해결중
        {
            Debug.Log(message);
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connInfoText.text = "방 생성 중";

        PhotonNetwork.CreateRoom(nicknameIF.text, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 도착");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
