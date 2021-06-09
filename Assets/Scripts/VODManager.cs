using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Image = UnityEngine.UI.Image;
using Firebase.Extensions;
using UnityEngine.EventSystems;

public class VODManager : MonoBehaviour
{
    private static FirebaseStorage storage = FirebaseStorage.DefaultInstance;
    StorageReference storageRef = storage.GetReferenceFromUrl("gs://project-6629124072636312930.appspot.com");
    public InputField keywordIF;
    public GameObject panel;

    public DatabaseReference reference;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void Search()
    {
        panel.SetActive(true);
        string keyword = keywordIF.text;
        int stopIndex = 0;
        storageRef = storage.GetReferenceFromUrl(GetUrl(keyword));
        bool isFirst = true;
        for (int i = 1; i <= 3; i++)
        {
            StorageReference ImagesRef = storageRef.Child($"{i}ȭ").Child("image");
            GameObject VODImage = null;
            VODImage = GameObject.Find($"VODImage{i}");
            Image image = VODImage.GetComponent<Image>();
            GameObject[] videoContent = GameObject.FindGameObjectsWithTag("videoContent");
            GameObject VODButton = null;
            VODButton = GameObject.Find($"SelecBtn{i}");
            Text text = VODButton.GetComponentInChildren<Text>();
            text.text = $"{i}ȭ ����";
            ImagesRef.Child($"{i}ȭ.png").GetBytesAsync(1024 * 1024).ContinueWithOnMainThread((Task<byte[]> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                    videoContent[i].SetActive(false);

                }
                else
                {
                    byte[] fileContents = task.Result;
                    Debug.Log("Start Download");
                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(fileContents);
                    Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    image.overrideSprite = newSprite;
                    Debug.Log("Finished downloading!");
                }
            });
        }
    }

    public void saveUrl()
    {
        GameObject temp = EventSystem.current.currentSelectedGameObject;
        Text tempText = temp.GetComponentInChildren<Text>();
        var a = tempText.text.Split(' ');
        string url = GetUrl(keywordIF.text) + "/" + a[0] + "/dvd/" + a[0] + ".mp4";
        FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string UID = firebaseUser.UserId;
        reference = FirebaseDatabase.DefaultInstance.GetReference("chatRooms");
        reference.Child(UID).Child("createdBy").Child("video").SetValueAsync(url);
    }

    public string GetUrl(string title)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        dictionary.Add("����", "gs://project-6629124072636312930.appspot.com/����");
        dictionary.Add("�ν���", "gs://project-6629124072636312930.appspot.com/�ν���");
        foreach(KeyValuePair<string, string> items in dictionary)
        {
            if (items.Key.Contains(title) == true)
            {
                return (items.Value);
            }
        }
        return ("Not Found");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
