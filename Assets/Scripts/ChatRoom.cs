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
    public ChatRoom(string image, string mastername, string description, string id, string name)
    {
        createdBy = new CreatedBy(image, mastername);
        this.description = description;
        this.id = id;
        this.name = name;
    }
}
