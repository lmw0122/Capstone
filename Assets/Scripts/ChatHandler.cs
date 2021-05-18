using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class ChatHandler : MonoBehaviour
{
    public DatabaseAPI database;
    private DatabaseReference reference;

    public TMP_InputField textIF;

    public GameObject messagePrefab;
    public Transform messagesContainer;
    private string key;
    private void Start()
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
                    if (createdBy["name"].Equals(LoginManager.nickname))
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        Debug.Log(key);
                        database.ListenForMessages(InstantiateMessage, Debug.Log, key);
                    }
                }
            }
        });

    }

    public void SendMessage() => database.PostMessage(new Message(LoginManager.nickname, textIF.text), () => Debug.Log("Message was sent!"), Debug.Log);

    private void InstantiateMessage(Message message)
    {
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainer, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.sender}: {message.text}";
    }
}
