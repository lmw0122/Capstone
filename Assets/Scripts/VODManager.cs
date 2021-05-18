using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
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

    // Start is called before the first frame update
    void Start()
    {

    }
    public void Search()
    {
        string keyword = keywordIF.text;

        storageRef = storage.GetReferenceFromUrl(GetUrl(keyword));
        for (int i = 1; i < 3; i++)
        {
            StorageReference ImagesRef = storageRef.Child($"episoad{i}({i}È­)").Child("image");
            GameObject VODImage = null;
            VODImage = GameObject.Find($"VODImage{i}");
            Image image = VODImage.GetComponent<Image>();

            GameObject VODButton = null;
            VODButton = GameObject.Find($"SelecBtn{i}");
            Text text = VODButton.GetComponentInChildren<Text>();
            text.text = $"ºó¼¾Á¶ {i}È­ º¸±â";
            ImagesRef.Child($"ºó¼¾Á¶{i}È­.png").GetBytesAsync(1024 * 1024).ContinueWithOnMainThread((Task<byte[]> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
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

    public string GetUrl(string title)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        dictionary.Add("vincenzo(ºó¼¾Á¶)", "gs://project-6629124072636312930.appspot.com/vincenzo(ºó¼¾Á¶)");
        foreach(KeyValuePair<string, string> items in dictionary)
        {
            if (items.Key.Contains(title) == true)
            {
                Debug.Log("URL is " + items.Value);
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
