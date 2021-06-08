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

public class DatabaseAPI : MonoBehaviour
{
    private DatabaseReference reference;
    public string key;
    public static string roommaster;
    public ToggleGroup toggleGoup;
    public TMP_InputField textIF;

    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        GetKey("admin");
        Debug.Log(roommaster);
    }

    private void GetKey(string mastername)
    {
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
                    if (createdBy["name"].Equals(mastername))
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                    }
                }
            }
        });

    }

    public void CheckToggle()
    {
        Toggle theActiveToggle = toggleGoup.ActiveToggles().FirstOrDefault();
        if (theActiveToggle.gameObject.name == "Public")
        {
            GetKey("admin");
        }
        else
        {
            GetKey(roommaster);
        }
    }

    public void PostMessage(Message message, Action callback, Action<AggregateException> fallback)
    {
        textIF.Select();
        //textIF.text = "";
        var messageJSON = StringSerializationAPI.Serialize(typeof(Message), message); //Message 타입을 JSON으로 파싱
        string messageKey = reference.Child("messages").Child(key).Push().Key;
        reference.Child("messages").Child(key).Child(messageKey).SetRawJsonValueAsync(messageJSON).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else
            {
                callback();
                reference.Child("messages").Child(key).Child(messageKey).UpdateChildrenAsync(new Dictionary<string, object> { { "timestamp", ServerValue.Timestamp } }); //Timestamp 설정
            }
        }); //비동기 task로 DB에 추가
    }

    public void ListenForMessages(Action<Message> callback, Action<AggregateException> fallback, string key)
    {
        void CurrentListener(object o, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null) fallback(new AggregateException(new Exception(args.DatabaseError.Message)));
            else callback(StringSerializationAPI.Deserialize(typeof(Message), args.Snapshot.GetRawJsonValue()) as Message);
        }
        reference.Child("messages").Child(key).ChildAdded += CurrentListener;
    }

}
