using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DatabaseAPI : MonoBehaviour
{
    private DatabaseReference reference;
    public string key;
    public ToggleGroup toggleGoup;

    private void Awake()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://react-firebase-chat-app-3b8de-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        GetKey(LoginManager.nickname);
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
                DataSnapshot snapshot = task.Result; // users�� ���� ����� snapshot���� �޾ƿ�
                foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
                {
                    IDictionary createdBy = (IDictionary)data.Child("createdBy").Value;
                    if (createdBy["name"].Equals(mastername))
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        Debug.Log(key);
                    }
                }
            }
        });

    }

    public void CheckToggle()
    {
        Toggle theActiveToggle = toggleGoup.ActiveToggles().FirstOrDefault();
        Debug.Log("It worked! " + theActiveToggle.gameObject.name);
        if (theActiveToggle.gameObject.name == "Public")
        {
            GetKey("admin");
        }
        else
        {
            GetKey(LoginManager.nickname);
        }
    }

    public void PostMessage(Message message, Action callback, Action<AggregateException> fallback)
    {
        
        var messageJSON = StringSerializationAPI.Serialize(typeof(Message), message); //Message Ÿ���� JSON���� �Ľ�
        reference.Child("messages").Child(key).Push().SetRawJsonValueAsync(messageJSON).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted) fallback(task.Exception);
            else callback();
        }); //�񵿱� task�� DB�� �߰�
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
