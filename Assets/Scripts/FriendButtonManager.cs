using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
//���̷뿡�� ģ�� �г����� �˻��ϸ� �� �濡�� ������ ģ�� ������ ���� ��ũ��Ʈ
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
        fBtn.onClick.AddListener(Clickf); //��ư�� ������ �� Clickf�� ���� ��
    }
    void Clickf()
    {
        fNickname = friendIF.text; // ���� �˻��� �ؽ�Ʈ�� fNickname������ �ְ�
        PhotonNetwork.LeaveRoom(true);// �� ���� ����-> ������ ���� �����Ϳ� ���ӵǰ� LoginManager�� OnConnectedToMaster �ȿ��� fNickname ������ ģ�� �� ���� 
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
