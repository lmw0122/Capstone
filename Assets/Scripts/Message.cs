using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Message : MonoBehaviour
{
    public string content;
    public Int64 timestamp;
    public class User
    {
        public string id;
        public string image;
        public string name;
        public User(string id, string image, string name)
        {
            this.id = id;
            this.image = image;
            this.name = name;
        }
    }
    public User user;
    public Message(string content, Int64 timestamp, string id, string image, string sendername)
    {
        user = new User(id, image, sendername);
        this.content = content;
        this.timestamp = timestamp;
    }

    public string getName()
    {
        return this.user.name;
    }
}