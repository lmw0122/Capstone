using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
//마이룸에서 친구 닉네임을 검색하면 내 방에서 나가고 친구 방으로 가는 스크립트
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
        fBtn.onClick.AddListener(Clickf); //버튼이 눌렸을 때 Clickf가 실행 됨
    }
    void Clickf()
    {
        fNickname = friendIF.text; // 내가 검색한 텍스트를 fNickname변수에 넣고
        PhotonNetwork.LeaveRoom(true);// 내 방을 나옴-> 나오는 순간 마스터에 접속되고 LoginManager의 OnConnectedToMaster 안에서 fNickname 가지고 친구 방 접속 
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
