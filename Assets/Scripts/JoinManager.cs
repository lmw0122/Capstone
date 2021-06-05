using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Unity.Editor;
using UnityEngine;
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
    public GameObject JoinCanvas;
    public GameObject CharacterSelect;

    public static FirebaseAuth auth;
    public DatabaseReference reference { get; set; }
    private bool nicknameChecked = false;
    private int roomIndex = 0;

    
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
            passwordText.text = "��й�ȣ�� 6�� �̻��̾�� �մϴ�.";
            passwordCheckText.text = "";
        }
        else
        {
            passwordText.text = "��밡���� ��й�ȣ�Դϴ�.";
            if (passwordIF.text != passwordCheckIF.text)
                passwordCheckText.text = "��й�ȣ�� Ȯ���� �ּ���.";
            else
                passwordCheckText.text = "��й�ȣ�� Ȯ�εǾ����ϴ�.";
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
        if (nicknameIF.text.Length == 0) // �г����� �Է��ؾ� ȸ������ ����
        {
            connInfoText.text = "�г����� �������ּ���";
            return;
        }
        if (idIF.text.Length != 0 && passwordIF.text.Length != 0) //ID�� ��й�ȣ ��� �Է��ؾ� ����
        {
            //DB�� ����� ���� �߰��ϴ� �κ�
            if (nicknameChecked)
                CreateUser();
            else
                connInfoText.text = "�г����� Ȯ�����ּ���";
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
                string UID = firebaseUser.UserId;

                reference.Child(UID).SetRawJsonValueAsync(userJson);

                connInfoText.text = "ȸ������ ����";
                Debug.Log(idIF.text + " �� ȸ������ �ϼ̽��ϴ�.");
                LoginManager.nickname = nicknameIF.text;
                CreateChatRoom(UID);
            }
            else
            {
                connInfoText.text = "ȸ������ ����";
                Debug.Log("ȸ�����Կ� �����ϼ̽��ϴ�.");
            }
        });
    }

    public void CreateChatRoom(string UID)
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
                DataSnapshot snapshot = task.Result;
                IDictionary temp = (IDictionary)snapshot.Value;
                roomIndex = temp.Count;
                ChatRoom chatRoom = new ChatRoom("http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", nicknameIF.text, "", nicknameIF.text + "�� ��", UID, nicknameIF.text + "�� ��", roomIndex);
                var chatRoomJson = StringSerializationAPI.Serialize(typeof(ChatRoom), chatRoom);
                reference.Child(UID).SetRawJsonValueAsync(chatRoomJson);
                Debug.Log("ä�ù� ���� �Ϸ�");
                SetPlayerNameAndImage(nicknameIF.text, "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon");
                JoinCanvas.SetActive(false);
                CharacterSelect.SetActive(true);
            }
        });

    }

    public void CheckNickname()
    {
        reference = FirebaseDatabase.DefaultInstance.GetReference("users"); // users ��ġ ����
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; // users�� ���� ����� snapshot���� �޾ƿ�
                foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
                {
                    IDictionary users = (IDictionary)data.Value;

                    if (users["name"].Equals(nicknameIF.text))
                    {
                        connInfoText.text = "�ߺ��� �г���";
                        Debug.Log(nicknameIF.text + "�� �̹� �ִ� �г����Դϴ�.");
                        nicknameChecked = false;
                        return;
                    }
                }
                connInfoText.text = "�г��� ��� ����";
                Debug.Log("�г��� ��� ����");
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



}
