using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject backButton;
    // Start is called before the first frame update
    void Start()
    {
        //if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname) //�� ���� �ƴ϶�� �ڷΰ��� ��ư�� Ȱ��ȭ �Ѵ�.
        //{
        //    backButton.SetActive(true);
        //}
        SpawnPlayer(); //�̸� ����� ���� player �������� ��ȯ�ϴ� �Լ�
    }
    private void SpawnPlayer ()
    {
        var localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        PhotonNetwork.Instantiate("Player", new Vector3(0,5f,0), Quaternion.identity, 0); //�÷��̾� �������� 0,5,0 ��ġ�� �����Ѵ�.
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Login");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
}
