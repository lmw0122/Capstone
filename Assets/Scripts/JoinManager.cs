using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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
    public InputField ipIF;

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

    public string imageURL;
    
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

    public class UserInfo
    {
        public string email;
        public string password;
        public string ipAddress;

        public UserInfo(string tempEmail, string tempPassword, string tempIP)
        {
            this.email = tempEmail;
            this.password = tempPassword;
            this.ipAddress = tempIP;
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
            passwordText.text = "?????????? 6?? ?????????? ??????.";
            passwordCheckText.text = "";
        }
        else
        {
            passwordText.text = "?????????? ??????????????.";
            if (passwordIF.text != passwordCheckIF.text)
                passwordCheckText.text = "?????????? ?????? ??????.";
            else
                passwordCheckText.text = "?????????? ??????????????.";
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
        if (nicknameIF.text.Length == 0) // ???????? ???????? ???????? ????
        {
            connInfoText.text = "???????? ????????????";
            return;
        }
        if (idIF.text.Length != 0 && passwordIF.text.Length != 0) //ID?? ???????? ???? ???????? ????
        {
            //DB?? ?????? ???? ???????? ????
            if (nicknameChecked)
                CreateUser();
            else
                connInfoText.text = "???????? ????????????";
        }
    }
    
    public void CreateUser()
    {
        auth.CreateUserWithEmailAndPasswordAsync(idIF.text, passwordIF.text).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCanceled && !task.IsFaulted)
            {
                imageURL = HashEmailForGravatar(idIF.text);
                User user = new User(idIF.text, nicknameIF.text, $"http://gravatar.com/avatar/{imageURL}?d=identicon");
                string userJson = JsonUtility.ToJson(user);
                FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                UID = firebaseUser.UserId;

                reference.Child(UID).SetRawJsonValueAsync(userJson);

                connInfoText.text = "???????? ????";
                Debug.Log(idIF.text + " ?? ???????? ??????????.");
                Debug.Log(UID);
                LoginManager.nickname = nicknameIF.text;
                
                
                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
                reference = FirebaseDatabase.DefaultInstance.GetReference("auto/" + LoginManager.nickname); // prefabs ???? ????
                UserInfo tempUser = new UserInfo(idIF.text, passwordIF.text, ipIF.text);
                string json = JsonUtility.ToJson(tempUser);
                reference.SetRawJsonValueAsync(json);
                CreateChatRoom();
            }
            else
            {
                connInfoText.text = "???????? ????";
                Debug.Log("?????????? ??????????????.");
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
                ChatRoom chatRoom = new ChatRoom($"http://gravatar.com/avatar/{imageURL}?d=identicon", nicknameIF.text, "", nicknameIF.text + "?? ??", UID, nicknameIF.text + "?? ??", roomIndex);
                var chatRoomJson = StringSerializationAPI.Serialize(typeof(ChatRoom), chatRoom);
                reference.Child(UID).SetRawJsonValueAsync(chatRoomJson);
                Debug.Log("?????? ???? ????");
                SetPlayerNameAndImage(nicknameIF.text, $"http://gravatar.com/avatar/{imageURL}?d=identicon");
                HumanAnimatorController.isJoin = true;
                JoinCanvas.SetActive(false);
                CharacterSelect.SetActive(true);
            }
        });

    }

    public void CheckNickname()
    {
        reference = FirebaseDatabase.DefaultInstance.GetReference("users"); // users ???? ????
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; // users?? ???? ?????? snapshot???? ??????
                foreach (DataSnapshot data in snapshot.Children) // snapshot?? ?? ???? ???????? ????
                {
                    IDictionary users = (IDictionary)data.Value;

                    if (users["name"].Equals(nicknameIF.text))
                    {
                        connInfoText.text = "?????? ??????";
                        nicknameChecked = false;
                        return;
                    }
                }
                connInfoText.text = "?????? ???? ????";
                Debug.Log("?????? ???? ????");
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
    
    public string HashEmailForGravatar(string email)
    {
        // Create a new instance of the MD5CryptoServiceProvider object.  
        MD5 md5Hasher = MD5.Create();
 
        // Convert the input string to a byte array and compute the hash.  
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));
 
        // Create a new Stringbuilder to collect the bytes  
        // and create a string.  
        StringBuilder sBuilder = new StringBuilder();
 
        // Loop through each byte of the hashed data  
        // and format each one as a hexadecimal string.  
        for(int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
 
        return sBuilder.ToString();  // Return the hexadecimal string. 
    }

}
