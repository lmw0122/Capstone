using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ChatRoom : MonoBehaviour
{
    public string description;
    public string id;
    public string name;
    public CreatedBy createdBy;
    public int number;
    public ChatRoom(string image, string mastername, string video, string description, string id, string name, int number)
    {
        createdBy = new CreatedBy(image, mastername, video);
        this.description = description;
        this.id = id;
        this.name = name;
        this.number = number;
    }
}
