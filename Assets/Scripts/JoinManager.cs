using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoinManager : MonoBehaviour
{
    public InputField nicknameIF;
    public Text connInfoText;
    public Text passwordCheckText;
    public Text passwordText;
    public InputField idIF;
    public InputField passwordIF;
    public InputField passwordCheckIF;
    public GameObject LoginCanvas;
    public GameObject JoinCanvas;
    public GameObject CharacterSelect;
    public GameObject CharacterConfirm;

    public static FirebaseAuth auth;
    public DatabaseReference reference { get; set; }
    private bool nicknameChecked = false;
    private int roomIndex = 0;
    private static string character;
    private static string UID = "";

    
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
    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        auth = FirebaseAuth.DefaultInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (passwordIF.text.Length == 0)
        {
            passwordText.text = "";
            passwordCheckText.text = "";
        }
        else if (passwordIF.text.Length < 6)
        {
            passwordText.text = "비밀번호는 6자 이상이어야 합니다.";
            passwordCheckText.text = "";
        }
        else
        {
            passwordText.text = "사용가능한 비밀번호입니다.";
            if (passwordIF.text != passwordCheckIF.text)
                passwordCheckText.text = "비밀번호를 확인해 주세요.";
            else
                passwordCheckText.text = "비밀번호가 확인되었습니다.";
        }
        
        nicknameIF.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
    }
    public void ValueChangeCheck()
    {
        if (nicknameChecked)
            nicknameChecked = false;
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
            if (nicknameChecked)
                CreateUser();
            else
                connInfoText.text = "닉네임을 확인해주세요";
        }
    }
    
    public void CreateUser()
    {
        auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCanceled && !task.IsFaulted)
            {
                User user = new User(idIF.text, nicknameIF.text, "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon");
                string userJson = JsonUtility.ToJson(user);
                FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                UID = firebaseUser.UserId;

                reference.Child(UID).SetRawJsonValueAsync(userJson);

                connInfoText.text = "회원가입 성공";
                Debug.Log(idIF.text + " 로 회원가입 하셨습니다.");
                Debug.Log(UID);
                LoginManager.nickname = nicknameIF.text;
                CreateChatRoom();
            }
            else
            {
                connInfoText.text = "회원가입 실패";
                Debug.Log("회원가입에 실패하셨습니다.");
            }
        });
    }

    public void CreateChatRoom()
    {
        reference = FirebaseDatabase.DefaultInstance.GetReference("chatRooms");
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                Debug.Log(roomIndex);
                DataSnapshot snapshot = task.Result;
                Debug.Log(task.Result);
                if (snapshot.Value != null)
                {
                    IDictionary temp = (IDictionary)snapshot.Value;
                    roomIndex = temp.Count;
                }
                Debug.Log(roomIndex);
                ChatRoom chatRoom = new ChatRoom("http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", nicknameIF.text, "", nicknameIF.text + "의 방", UID, nicknameIF.text + "의 방", roomIndex);
                var chatRoomJson = StringSerializationAPI.Serialize(typeof(ChatRoom), chatRoom);
                reference.Child(UID).SetRawJsonValueAsync(chatRoomJson);
                Debug.Log("채팅방 생성 완료");
                SetPlayerNameAndImage(nicknameIF.text, "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon");
                HumanAnimatorController.isJoin = true;
                JoinCanvas.SetActive(false);
                CharacterSelect.SetActive(true);
            }
        });

    }

    public void CheckNickname()
    {
        reference = FirebaseDatabase.DefaultInstance.GetReference("users"); // users 위치 참조
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; // users의 쿼리 결과를 snapshot으로 받아옴
                foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                {
                    IDictionary users = (IDictionary)data.Value;

                    if (users["name"].Equals(nicknameIF.text))
                    {
                        connInfoText.text = "중복된 닉네임";
                        nicknameChecked = false;
                        return;
                    }
                }
                connInfoText.text = "닉네임 사용 가능";
                Debug.Log("닉네임 사용 가능");
                nicknameChecked = true;
                return;
            }
        });
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

    public void Confirm()
    {
        reference = FirebaseDatabase.DefaultInstance.GetReference("users");
        reference.Child(UID).Child("character").SetValueAsync(character);
        CharacterConfirm.SetActive(false);
        LoginCanvas.SetActive(true);
        HumanAnimatorController.isJoin = false;
    }
    public void Cancel()
    {
        CharacterConfirm.SetActive(false);
        CharacterSelect.SetActive(true);
    }
    public void SelectCharacter()
    {
        GameObject temp = EventSystem.current.currentSelectedGameObject;
        character = temp.name;
        CharacterSelect.SetActive(false);
        CharacterConfirm.SetActive(true);
    }
}
