using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;

public class Clients : MonoBehaviour
{
    string clientName;

    bool socketReady;
    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;

    public void ConnetToServer()
    {
        if (socketReady) return;

        string host = "127.0.0.1";
        int port = 7777;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            Debug.Log("연결 성공");
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log($"소켓 에러 : {e.Message}");
        }
    }

    public void SendURL(string URL)
    {
        Debug.Log("URL : " + URL);
        writer.WriteLine(URL);
        writer.Flush();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(socketReady && stream.DataAvailable)
        {
            string data = reader.ReadLine();
            if (data != null)
                OnIncomingData(data);
        }
    }

    void OnIncomingData(string data)
    {
        if(data.Contains("project"))
        {
            Debug.Log("URL : " + data);
            Application.OpenURL(data);
            return;
        }
        else
        {
            Debug.Log("data : " + data);
            return;
        }
    }
}
