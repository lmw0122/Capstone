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
        if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname) //내 방이 아니라면 뒤로가기 버튼을 활성화 한다.
        {
            backButton.SetActive(true);
        }
        SpawnPlayer(); //미리 만들어 놓은 player 프리팹을 소환하는 함수
    }
    private void SpawnPlayer ()
    {
        var localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        PhotonNetwork.Instantiate("Player", new Vector3(0,5f,0), Quaternion.identity, 0); //플레이어 프리팹을 0,5,0 위치에 생성한다.
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
