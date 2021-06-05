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
    public static string roommaster;

    public ScrollRect scrollPublic;
    public ScrollRect scrollPrivate;

    RectTransform messageSize;
    float yValue;
    private void Start()
    {
        scrollPublic.content.sizeDelta = new Vector2(scrollPublic.content.sizeDelta.x, 10);
        scrollPrivate.content.sizeDelta = new Vector2(scrollPrivate.content.sizeDelta.x, 10);
        GetMessages(); // 처음 시작할 때 각 채팅방의 메시지들을 받아옴
    }

    private void GetMessages() // 방 생성자의 닉네임을 통해 해당 방을 찾아 채팅 내용을 불러옴
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
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
                    if (createdBy["name"].Equals(roommaster)) // Private 채팅방 생성
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        database.ListenForMessages(InstantiateMessage, Debug.Log, key);
                    }
                    else if (createdBy["name"].Equals("admin")) // Public 채팅방 생성
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        database.ListenForMessages(InstantiateMessagePublic, Debug.Log, key);
                    }

                }
            }
        });
    }

    public void SendMessage() => database.PostMessage(new Message(textIF.text, 0, LoginManager.auth.CurrentUser.UserId,
        "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", LoginManager.nickname), () => Debug.Log("Message was sent!"), Debug.Log);

    private void InstantiateMessage(Message message)
    {
        yValue = 0;
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainer, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.getName()}: {message.content}";
        //메세지 창 크기 늘리기
        yValue = scrollPrivate.content.sizeDelta.y + newMessage.GetComponent<RectTransform>().sizeDelta.y;
        scrollPrivate.content.sizeDelta = new Vector2(scrollPrivate.content.sizeDelta.x, yValue);
    }

    private void InstantiateMessagePublic(Message message)
    {
        yValue = 0;
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainerPublic, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.getName()}: {message.content}";
        //메세지 창 크기 늘리기
        yValue = scrollPublic.content.sizeDelta.y + newMessage.GetComponent<RectTransform>().sizeDelta.y;
        scrollPublic.content.sizeDelta = new Vector2(scrollPublic.content.sizeDelta.x, yValue);
    }

}
