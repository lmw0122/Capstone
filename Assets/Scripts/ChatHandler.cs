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
        GetMessages(); // ó�� ������ �� �� ä�ù��� �޽������� �޾ƿ�
    }

    private void GetMessages() // �� �������� �г����� ���� �ش� ���� ã�� ä�� ������ �ҷ���
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
                DataSnapshot snapshot = task.Result; // users�� ���� ����� snapshot���� �޾ƿ�
                foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
                {
                    IDictionary createdBy = (IDictionary)data.Child("createdBy").Value;
                    if (createdBy["name"].Equals(roommaster)) // Private ä�ù� ����
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        database.ListenForMessages(InstantiateMessage, Debug.Log, key);
                    }
                    else if (createdBy["name"].Equals("admin")) // Public ä�ù� ����
                    {
                        IDictionary chatRooms = (IDictionary)data.Value;
                        key = chatRooms["id"].ToString();
                        database.ListenForMessages(InstantiateMessagePublic, Debug.Log, key);
                    }

                }
            }
        });
    }
    //public void ConnectToPublicRoom() // ���� ä�ù� ����
    //{
    //    messagesContainer.DetachChildren(); // �޽��� â�� �߰��� �޽����� ����
    //    GetMessages("admin");
    //}

    //public void ConnectToPrivateRoom() // ����� ä�ù� ���� (���̷� ���� �ִ� ä�ù�)
    //{
    //    messagesContainer.DetachChildren();
    //    GetMessages(LoginManager.nickname);
    //}

    //public void CheckToggle() //��� �׷� �߿��� � ���� üũ�Ǿ� �ִ��� Ȯ���Ͽ� �׿� �´� �Լ� ����
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


    public void SendMessage() => database.PostMessage(new Message(textIF.text, 0, LoginManager.auth.CurrentUser.UserId,
        "http://gravatar.com/avatar/6c3b875d4cca14d87106af96bd2951e5?d=identicon", LoginManager.nickname), () => Debug.Log("Message was sent!"), Debug.Log);

    private void InstantiateMessage(Message message)
    {
        yValue = 0;
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainer, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.getName()}: {message.content}";
        //�޼��� â ũ�� �ø���
        yValue = scrollPrivate.content.sizeDelta.y + newMessage.GetComponent<RectTransform>().sizeDelta.y;
        scrollPrivate.content.sizeDelta = new Vector2(scrollPrivate.content.sizeDelta.x, yValue);
    }

    private void InstantiateMessagePublic(Message message)
    {
        yValue = 0;
        var newMessage = Instantiate(messagePrefab, transform.position, Quaternion.identity);
        newMessage.transform.SetParent(messagesContainerPublic, false);
        newMessage.GetComponent<TextMeshProUGUI>().text = $"{message.getName()}: {message.content}";
        //�޼��� â ũ�� �ø���
        yValue = scrollPublic.content.sizeDelta.y + newMessage.GetComponent<RectTransform>().sizeDelta.y;
        scrollPublic.content.sizeDelta = new Vector2(scrollPublic.content.sizeDelta.x, yValue);
    }

}
