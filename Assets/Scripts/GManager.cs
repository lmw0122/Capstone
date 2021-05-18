using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using UnityEngine.EventSystems;

public class GManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject backButton;
    public GameObject FCanvas;
    public GameObject contents;
    public static string fNickname = "";
    public InputField friendIF;


    
    public DatabaseReference reference { get; set; }

    public List<string> friendList = new List<string>();
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

    public void ClickGo()
    {
        GameObject temp = EventSystem.current.currentSelectedGameObject;
        Text tempText = temp.GetComponentInParent<Text>();
        fNickname = tempText.text;
        PhotonNetwork.Disconnect();
    }
    public void Clickf()
    {
        fNickname = friendIF.text; // 내가 검색한 텍스트를 fNickname변수에 넣고
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        FirebaseDatabase.DefaultInstance.GetReference("rooms").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("친구 불러오기 실패");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                {
                    string tempFname = (string)data.Key;
                    Debug.Log("fname is : " + tempFname);
                    if (tempFname.Contains(fNickname))
                    {
                        if(tempFname != LoginManager.nickname)
                        {
                            friendList.Add(tempFname);
                            Debug.Log("배열에 들어갔음");
                        }
                        
                    }

                }
                FCanvas.SetActive(true);
                for(int i = 1; i<friendList.Count+1; i++)
                {
                    contents = GameObject.Find($"Friend" +i.ToString());
                    Text conText = contents.GetComponent<Text>();
                    if(conText.text != LoginManager.nickname)
                    {
                        conText.text = friendList[i - 1];
                    }
                    
                }
                for (int i = 1; i < 8; i++)
                {
                    contents = GameObject.Find($"Friend" + i.ToString());
                    Text conText = contents.GetComponent<Text>();
                    
                    if(conText.text == "")
                    {
                        contents.SetActive(false);
                    }
                }
                //string tempFnames = snapshot.Value.ToString();
            }
        });

        //PhotonNetwork.Disconnect();
        // 내 방을 나옴-> 나오는 순간 마스터에 접속되고 LoginManager의 OnConnectedToMaster 안에서 fNickname 가지고 친구 방 접속 

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
}
