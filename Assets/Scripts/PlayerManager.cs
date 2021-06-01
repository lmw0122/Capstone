using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.UI;
// player의 속성들을 설정하는 스크립트 
public class PlayerManager : MonoBehaviourPun 
{
    //public GameObject other;

    
    private float rotationSpeed = 5.0f;

    
    private Vector3 movePosition;
    // Start is called before the first frame update
    void Start()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

        if (photonView.IsMine) // 이 플레이어가 내 플레이어라면 
        {
            //myName.text = LoginManager.nickname;
            //myName.color = Color.blue;
        }
        else // 나를 제외한 내 방에 있는 플레이어들에 대해
        {
            //myName.text = LoginManager.nickname;
            //myName.color = Color.red;
        }
        
        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
    }
    [PunRPC]
    void sendRPC()
    {
        GameObject.Find("GameManager").GetComponent<GManager>().SendURL();
    }
    [PunRPC]
    void loadByRemote()
    {
        GameObject.Find("GameManager").GetComponent<GManager>().loadPrefabs();
    }
    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) // 내 플레이어에 대해서만 코딩할것이라서 나머지는 리턴한다 
        {
            return;
        }
        else
        {
            // JoystickManager 스크립트에서 계산된 movePosition값을 계산하는데 그걸 매 프레임마다 적용해주는 코드 
            transform.position += JoystickManager.movePosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, JoystickManager.angle, 0), rotationSpeed * Time.deltaTime);
            
        }
    }

    

}
