using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class BackBtnManager : MonoBehaviour
{
    public Button bBtn;
    // Start is called before the first frame update
    void Start()
    {
        bBtn = this.transform.GetComponent<Button>();
        bBtn.onClick.AddListener(Clickb);
    }
    void Clickb()
    {
        FriendButtonManager.fNickname = "";
        //PhotonNetwork.OpLeave(true);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
