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
    public Button loginBtn;
    [SerializeField]
    public InputField nicknameIF;
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
    public FirebaseApp app = null;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); // 마스터 서버에 접속
        auth = FirebaseAuth.DefaultInstance;
        //connInfoText.text = "마스터에 접속중";

        //client = new LoadBalancingClient();
        //appset.AppIdRealtime = "3cdd2e0f-e063-42b9-96e1-6245fcc77a1a";
        //appset.AppVersion = gameVersion;
        //bool connectInProcess = client.ConnectUsingSettings(appset);
    }


    // 마스터 서버에 접속되면 자동 실행되는 함수 3가지 경우 다 실행되서 나눠서 코딩함 1. 맨 처음 들어올 때 2.내 방에서 나가서 친구방 찾아갈때 3.친구방 나와서 내방으로 돌아갈 때
    public override void OnConnectedToMaster()
    {
        Debug.Log(isFirst);
        //connInfoText.text = "마스터에 연결됨";
        connInfoText.text = "마스터에 연결됨";
        
        if(GManager.fNickname != "") // 2번 경우, fNickname은 내가 마이룸에서 가고 싶은 친구 검색했을 때 그 닉네임
        {
            Debug.Log("친구방으로 이동중");
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
            PhotonNetwork.JoinOrCreateRoom(GManager.fNickname, roomOptions, null);//fNickname이름과 같은 방으로 이동
        }
        if(isFirst == false) // 1,3번 경우, 지금은 3번 경우 오류가 나서 다시 내방으로 안돌아가짐(webhook 관련해서 다시 코딩 예정)
        {
            Debug.Log("내방으로 재접속중");
            PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);
            //PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null); // 내 닉네임 이름의 방으로 이동
        }
        
    }

    public override void OnDisconnected(DisconnectCause cause) // 1,2,3의 경우에 마스터 서버로 접속 못한 경우 자동 실행되는 함수(예외 처리용?)
    {
        
        connInfoText.text = "마스터에 접속 실패";

        PhotonNetwork.ConnectUsingSettings();// 다시 마스터에 접속 시도
    }

    public void Connect() // 유니티에서 Login 버튼을 눌렀을 때 호출되는 함수
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
        if (nicknameIF.text == "") // 닉네임 입력했는지 체크용(닉네임이 방 만들고 해서 필수이기 때문에)
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
                connInfoText.text = "로그인중";
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled) // 문제 없이 Task 실행되었을 경우
                {
                    if (PhotonNetwork.IsConnected) //마스터에 접속되어 있다면
                    {
                        
                        connInfoText.text = "내방으로 처음 이동중";
                        PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null);

                        SendToken();
                        //client.OpJoinRoom(roomParams
                        //PhotonNetwork.JoinOrCreateRoom(nickname, roomOptions, null); // 내 닉네임으로 방을 만들고 들어올 수 있는 최대 인원수는 4명이다. -> 그 이후에 접속
                    }
                    else //마스터에 접속 안되어 있다면
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
            //DB에 사용자 정보 추가하는 부분
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
            reference = FirebaseDatabase.DefaultInstance.GetReference("users"); // users 위치 참조
            
            reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    connInfoText.text = "회원가입 실패";
                    Debug.Log("회원가입에 실패하셨습니다.");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result; // users의 쿼리 결과를 snapshot으로 받아옴
                    foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                    {
                        IDictionary users = (IDictionary)data.Value;
                       
                        if (users["name"].Equals(nicknameIF.text.ToString()))
                        {
                            connInfoText.text = "중복된 닉네임";
                            Debug.Log(nicknameIF.text + "는 이미 있는 닉네임입니다.");
                            return;
                        }
                    }
                    Debug.Log("닉네임 사용 가능");
                    auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread( task =>
                    {
                        if (!task.IsCanceled && !task.IsFaulted)
                        {
                            User user = new User(idIF.text, nicknameIF.text, "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon");
                            string userJson = JsonUtility.ToJson(user);
                            FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                            string UID = firebaseUser.UserId;

                            string userKey = reference.Push().Key;
                            reference.Child(UID).SetRawJsonValueAsync(userJson);

                            connInfoText.text = "회원가입 성공";
                            Debug.Log(idIF.text + " 로 회원가입 하셨습니다.");

                            reference = FirebaseDatabase.DefaultInstance.GetReference("chatRooms");
                            reference.GetValueAsync().ContinueWithOnMainThread(task =>
                            {
                                if (task.IsFaulted || task.IsCanceled)
                                {

                                }
                                else if (task.IsCompleted)
                                {
                                    Debug.Log("room index is : ");

                                    DataSnapshot snapshot = task.Result;
                                    IDictionary temp = (IDictionary)snapshot.Value;
                                    roomIndex = temp.Count;
                                    Debug.Log("room index is : " + roomIndex);
                                    string chatRoomKey = reference.Push().Key;
                                    ChatRoom chatRoom = new ChatRoom("http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", nicknameIF.text, "", "description", UID, nicknameIF.text + "의 방", roomIndex); //TODO: 현재 방 주인의 nickname만 입력되는 상태, 추후 수정 필요
                                    var chatRoomJson = StringSerializationAPI.Serialize(typeof(ChatRoom), chatRoom);
                                    reference.Child(UID).SetRawJsonValueAsync(chatRoomJson);
                                    Debug.Log("채팅방 생성 완료");
                                    SetPlayerNameAndImage(nicknameIF.text, "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon");
                                }
                            });
                            // chatRooms 위치 참조
                            
                        }
                        else
                        {
                            connInfoText.text = "회원가입 실패";
                            Debug.Log("회원가입에 실패하셨습니다.");
                        }
                    });
                }
            });

            
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

        PhotonNetwork.LoadLevel("Main");// Main 씬 불러옴(마이룸 씬)
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

    public void SetPlayerNameAndImage(string playerName, string imageUrl)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            UserProfile profile = new UserProfile
            {
                DisplayName = playerName,
                PhotoUrl = new System.Uri(imageUrl),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        }
    }
    public void SendToken()
    {
        FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string UID = firebaseUser.UserId;

        Program P = new Program();
        string token = P.GenerateJWTToken(UID);

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.GetReference("login/" + LoginManager.nickname);
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("token", token);
        Debug.Log(token);
        reference.SetValueAsync(dictionary);
    }

}
