using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class BackBtnManager : MonoBehaviour
{
    public Button bBtn;
    //public const bool isfirst = false;
    // Start is called before the first frame update
    void Start()
    {
        bBtn = this.transform.GetComponent<Button>();
        bBtn.onClick.AddListener(Clickb);
    }

    void Clickb()
    {
        GManager.fNickname = "";
        LoginManager.isFirst = false;
        //LoadBalancingClient client = LoginManager.client;
        //client.OpLeaveRoom(true);
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Login");
    }
    // Update is called once per frame
    void Update()
    {

    }
}
