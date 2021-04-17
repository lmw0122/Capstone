using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class FriendButtonManager : MonoBehaviour
{
    public static string fNickname = "";
    [SerializeField]
    private InputField friendIF;

    Button fBtn;
    
    // Start is called before the first frame update
    void Start()
    {
        fBtn = this.transform.GetComponent<Button>();
        fBtn.onClick.AddListener(Clickf);
    }
    void Clickf()
    {
        fNickname = friendIF.text;
        PhotonNetwork.LeaveRoom(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
