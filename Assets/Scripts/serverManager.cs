using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
public class serverManager : MonoBehaviour
{
    string ip;
    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    bool socketReady;
    string URLforme;
    string URLforsend;
    GameObject[] players;
    PhotonView PV;
    // Start is called before the first frame update
    void Start()
    {
        ip = LoginManager.ipAdd;
        URLforme = "https://project-6629124072636312930.web.app/info/" + LoginManager.nickname;
        URLforsend = "https://project-6629124072636312930.web.app/main/" + LoginManager.nickname;

        players = GameObject.FindGameObjectsWithTag("localplayers");
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView temppv = players[i].GetComponent<PhotonView>();
            if (temppv.IsMine)
            {
                PV = temppv;
            }
        }
    }
    public void SendURL()
    {
        if (socketReady) return;


        string host = ip;
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

        URLforme = URLforme + "/" + PhotonNetwork.CurrentRoom.Name;
        URLforsend = URLforsend + "/" + PhotonNetwork.CurrentRoom.Name;
        Debug.Log("URL : " + URLforsend);
        writer.WriteLine(URLforsend);
        writer.Flush();

        Application.OpenURL(URLforme);
    }
    public void ClickVOD()
    {
        PV.RPC("sendRPC", RpcTarget.All);
    }

    // Update is called once per frame
    void Update()
    {
        if (socketReady && stream.DataAvailable)
        {
            string data = reader.ReadLine();
            if (data != null)
                Debug.Log("Data from others : " + data);
        }
    }
}
