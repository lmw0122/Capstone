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
using System;
using System.Net.Sockets;
using System.IO;

public class GManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject backButton;
    public GameObject FCanvas;
    public GameObject contents;
    public static string fNickname = "";
    public InputField friendIF;

    public GameObject[] players;
    public GameObject[] prefabs;
    public GameObject[] remainPrefabs;
    public PrefabInfo[] remainPrefabInfos;
    public Camera preCam;
    public Camera mainCam;

    public static GameObject toCreate;

    bool socketReady;
    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    public string URLforsend;
    public string URLforme;
    public Text infoText;

    public PhotonView PV;
    public class PrefabInfo
    {
        public Vector3 tempVector;
        public Quaternion tempQ;
        public string tempName;

        public PrefabInfo(string tempName, Vector3 tempVector, Quaternion tempQ)
        {
            this.tempName = tempName;
            this.tempQ = tempQ;
            this.tempVector = tempVector;
        }
    }

    public DatabaseReference reference { get; set; }

    public List<string> friendList = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname) //내 방이 아니라면 뒤로가기 버튼을 활성화 한다.
        {
            backButton.SetActive(true);
            
        }
        SpawnPlayer();
        if(PhotonNetwork.CurrentRoom.Name == LoginManager.nickname)
        {
            loadPrefabs();
        }
        URLforme = "https://project-6629124072636312930.web.app/sub?" + LoginManager.nickname;
        URLforsend = "https://project-6629124072636312930.web.app/main?" + LoginManager.nickname;
        //미리 만들어 놓은 player 프리팹을 소환하는 함수
    }
    public void ConnetToServer()
    {
        players = GameObject.FindGameObjectsWithTag("localplayers");
        for(int i = 0; i < players.Length; i++)
        {
            PhotonView temppv = players[i].GetComponent<PhotonView>();
            if(temppv.IsMine)
            {
                PV = temppv;
                Debug.Log("내꺼 찾기 성공");
            }
        }
    }
    public void SendURL()
    {
        if (socketReady) return;

        string host = "172.20.10.2";
        int port = 7777;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            Debug.Log("연결 성공");
            infoText.text = "연결 성공";
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log($"소켓 에러 : {e.Message}");
        }

        URLforme = URLforme + PhotonNetwork.CurrentRoom.Name;
        URLforsend = URLforsend + PhotonNetwork.CurrentRoom.Name;
        Debug.Log("URL : " + URLforsend);
        writer.WriteLine(URLforsend);
        writer.Flush();

        Application.OpenURL(URLforme);
    }
    
    public void ClickVOD()
    {
        PV.RPC("sendRPC", RpcTarget.All);
        Debug.Log("rpc sended");
        SendURL();
    }
    private void SpawnPlayer ()
    {
        PhotonNetwork.Instantiate("Mouse", new Vector3(0,1f,0), Quaternion.identity, 0); //플레이어 프리팹을 0,5,0 위치에 생성한다.
        

    }
    
    public override void OnJoinedRoom()
    {
        loadPrefabs();
        Debug.Log("join room");
    }
    public override void OnLeftRoom()
    {
        savePrefabs();
        
        SceneManager.LoadScene("Login");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        savePrefabs();
    }
    public void savePrefabs()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.GetReference("prefabs/"+LoginManager.nickname); // prefabs 위치 참조

        remainPrefabs = GameObject.FindGameObjectsWithTag("prefabs");
        Debug.Log(remainPrefabs.Length);
        for (int i = 0; i < remainPrefabs.Length; i++)
        {
            Debug.Log(remainPrefabs[i].name);
            Debug.Log(remainPrefabs[i].transform.position);
            Debug.Log(remainPrefabs[i].transform.rotation);
            PrefabInfo remainPrefabInfos = new PrefabInfo(remainPrefabs[i].name, remainPrefabs[i].transform.position, remainPrefabs[i].transform.rotation);
            string json = JsonUtility.ToJson(remainPrefabInfos);
            reference.Child(i.ToString()).SetRawJsonValueAsync(json);

        }
    }

    public void loadPrefabs()
    {
        Debug.Log("Loading...");
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance.GetReference("prefabs/"+PhotonNetwork.CurrentRoom.Name).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("프리팹 불러오기 실패");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                {
                    string tempName = (string)data.Child("tempName").Value;
                    IDictionary tempQDic = (IDictionary)data.Child("tempQ").Value;
                    IDictionary tempVecDic = (IDictionary)data.Child("tempVector").Value;
  
                    float tempx = Convert.ToSingle(tempVecDic["x"]);
                    float tempy = Convert.ToSingle(tempVecDic["y"]);
                    float tempz = Convert.ToSingle(tempVecDic["z"]);
             
                    Vector3 tempPosition = new Vector3(tempx,tempy,tempz);
                    Quaternion tempQ = new Quaternion(Convert.ToSingle(tempQDic["x"]), Convert.ToSingle(tempQDic["y"]), Convert.ToSingle(tempQDic["z"]), Convert.ToSingle(tempQDic["w"]));
                    Debug.Log("name is : " + tempName);
                    Debug.Log("vector is : " + tempPosition);
                    Debug.Log("Q is : " + tempQ);

                    string name = tempName.Split('(')[0];
                    Debug.Log("origin name is : " + name);

                    for(int i =0;i < prefabs.Length; i++)
                    {
                        if(name == prefabs[i].name)
                            Instantiate(prefabs[i], tempPosition, tempQ);
                    }

                }
                //string tempFnames = snapshot.Value.ToString();
            }
        });
        Debug.Log("Loading Complete");
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

    public void ClickPrefabs()
    {
        mainCam.enabled = false;
        preCam.enabled = true;
        
        GameObject temp = EventSystem.current.currentSelectedGameObject;
        Text tempText = temp.GetComponentInParent<Text>();
        
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (tempText.text == prefabs[i].name)
                toCreate = prefabs[i];
                
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (socketReady && stream.DataAvailable)
        {
            string data = reader.ReadLine();
            if (data != null)
                Debug.Log("Data from others : " + data);
        }
    }
}
