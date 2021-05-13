using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.CompilerServices;

public class ChatHandler : MonoBehaviour
{
    public DatabaseAPI database;
    private DatabaseReference reference;

    public TMP_InputField textIF;

    public GameObject messagePrefab;
    public Transform messagesContainer;
    public Transform messagesContainerPublic;
    public ToggleGroup toggleGoup;
    private string key;
    private void Start()
    {
        GetMessages(); // 처음 시작할 때 각 채팅방의 메시지들을 받아옴
    }

    private void GetMessages() // 방 생성자의 닉네임을 통해 해당 방을 찾아 채팅 내용을 불러옴
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://react-firebase-chat-app-3b8de-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("chatRooms").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; // users의 쿼리 결과를 snapshot으로 받아옴
                foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                {
                    IDictionary createdBy = (IDictionary)data.Child("createdBy").Value;
                    if (createdBy["name"].Equals(LoginManager.nickname)) // Private 채팅방 생성
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        Debug.Log(key);
                        database.ListenForMessages(InstantiateMessage, Debug.Log, key);
                    }
                    else if (createdBy["name"].Equals("admin")) // Public 채팅방 생성
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        Debug.Log(key);
                        database.ListenForMessages(InstantiateMessagePublic, Debug.Log, key);
                    }

                }
            }
        });
    }
    //public void ConnectToPublicRoom() // 공개 채팅방 접속
    //{
    //    messagesContainer.DetachChildren(); // 메시지 창에 추가된 메시지들 삭제
    //    GetMessages("admin");
    //}

    //public void ConnectToPrivateRoom() // 비공개 채팅방 접속 (마이룸 별로 있는 채팅방)
    //{
    //    messagesContainer.DetachChildren();
    //    GetMessages(LoginManager.nickname);
    //}

    //public void CheckToggle() //토글 그룹 중에서 어떤 것이 체크되어 있는지 확인하여 그에 맞는 함수 실행
    //{
    //    Toggle theActiveToggle = toggleGoup.ActiveToggles().FirstOrDefault();
    //    Debug.Log("It worked! " + theActiveToggle.gameObject.name);
    //    if (theActiveToggle.gameObject.name == "Public")
    //    {
    //        ConnectToPublicRoom();
    //    }
    //    else
    //    {
    //        ConnectToPrivateRoom();
    //    }
    //}


    public void SendMessage() => database.PostMessage(new Message(textIF.text, ServerValue.Timestamp.ToString(), LoginManager.auth.CurrentUser.UserId,
        "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", LoginManager.nickname), () => Debug.Log("Message was sent!"), Debug.Log);

    private void InstantiateMessage(Message message)
    {
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainer, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.getName()}: {message.content}";
    }

    private void InstantiateMessagePublic(Message message)
    {
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainerPublic, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.getName()}: {message.content}";
    }

}
