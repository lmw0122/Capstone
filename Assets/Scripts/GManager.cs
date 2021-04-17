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
        SpawnPlayer();
    }
    private void SpawnPlayer ()
    {
        var localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        PhotonNetwork.Instantiate("Player", new Vector3(0,5f,0), Quaternion.identity, 0);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Login");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname)
        {
            backButton.SetActive(true);
        }
    }

}
