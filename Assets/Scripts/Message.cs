using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Message : MonoBehaviour
{
    public string sender;
    public string text;
    public Message(string sender, string text)
    {
        this.sender = sender;
        this.text = text;
    }
}
