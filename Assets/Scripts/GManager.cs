using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Extensions;
using UnityEngine.EventSystems;
using System;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.Networking;

public class GManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject backButton;
    public GameObject FCanvas;
    public GameObject contents;
    public static string fNickname = "";
    public InputField friendIF;
    public GameObject thisisme;
    public GameObject[] players;
    public GameObject[] prefabs;
    public GameObject[] remainPrefabs;
    public PrefabInfo[] remainPrefabInfos;
    public Camera preCam;
    public Camera mainCam;
    public GameObject userifo;
    public InputField serverIF;
    public static GameObject toCreate;
    public GameObject createCanvas;
    public GameObject mainCanvas;
    public ToggleGroup toggleGoup;

    public GameObject backTemp;
    bool socketReady;
    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    public string URLforsend;
    public string URLforme;

    public string imageUrl = "";
    
    public Text infoText;
    public TMP_InputField textIF;
    public PhotonView PV;
    public RawImage testImage;
    public ScrollRect chatRect;
    public GameObject scrollRect;
    public GameObject scrollRect2;

    public static string tempIP;
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
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");

        if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname) //내 방이 아니라면 뒤로가기 버튼을 활성화 한다.
        {
            backButton.SetActive(true);
            
        }
        FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string UID = firebaseUser.UserId;

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("users").Child(UID).Child("character").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                SpawnPlayer(snapshot.Value.ToString());
            }
        });
        
        //미리 만들어 놓은 player 프리팹을 소환하는 함수
        
    }
    private void Awake()
    {
        setUserinfo();
        if (PhotonNetwork.CurrentRoom.Name == LoginManager.nickname)
        {
            loadPrefabs();
        }
        

    }
    public void setUserinfo()
    {
        
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userifo.GetComponentInChildren<Text>().text = LoginManager.nickname;
        reference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("사진 불러오기 실패");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; // users의 쿼리 결과를 snapshot으로 받아옴
                foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                {
                    string tempName = (string)data.Child("name").Value;
                    if (tempName.Equals(LoginManager.nickname))
                    {
                        imageUrl = (string)data.Child("image").Value;
                        
                        StartCoroutine(CoLoadImageTexture(imageUrl));
                        //Debug.Log("texture is : " + testImage.texture.ToString());
                        Texture tempTextture = testImage.texture;
                        userifo.GetComponentInChildren<RawImage>().texture = tempTextture;
                        return;
                    }
                }
            }
        });
        
    }
    IEnumerator CoLoadImageTexture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            testImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
    public void ShowMessage()
    {
        string mess = textIF.text.ToString();
        textIF.text = "";

        players = GameObject.FindGameObjectsWithTag("localplayers");
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView temppv = players[i].GetComponent<PhotonView>();
            if (temppv.IsMine)
            {
                thisisme = players[i];
                PV = temppv;
            }
        }
        Toggle theActiveToggle = toggleGoup.ActiveToggles().FirstOrDefault();
        if (thisisme && theActiveToggle.gameObject.name == "Private")
        {
            PV.RPC("showChat", RpcTarget.All, mess);
        }
        //채팅방 위치 초기화 + 스크롤 보이게
        
    }
    public void ConnectToServer()
    {
        
    }

    public void findMe()
    {
        players = GameObject.FindGameObjectsWithTag("localplayers");
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView temppv = players[i].GetComponent<PhotonView>();
            if (temppv.IsMine)
            {
                PV = temppv;
            }
        }
    }
    public void SendURL()
    {
        if (socketReady) return;

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("auto").Child(LoginManager.nickname).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("nickname is " + LoginManager.nickname);
                tempIP = (string)snapshot.Child("ipAddress").Value;// users의 쿼리 결과를 snapshot으로 받아옴
                Debug.Log("ip is " + tempIP);
                
                int port = 7777;

                try
                {
                    socket = new TcpClient(tempIP, port);
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

                URLforme = "https://project-6629124072636312930.web.app/info/" + LoginManager.nickname + "/" + PhotonNetwork.CurrentRoom.Name;
                URLforsend = "https://project-6629124072636312930.web.app/main/" + LoginManager.nickname + "/" + PhotonNetwork.CurrentRoom.Name;
                Debug.Log("URL : " + URLforsend);
                writer.WriteLine(URLforsend);
                writer.Flush();

                Application.OpenURL(URLforme);
            }
        });
        
    }
    
    public void ClickVOD()
    {
        findMe();
        PV.RPC("sendRPC", RpcTarget.Others);
        SendURL();
       
    }
    private void SpawnPlayer (string prefabname)
    {
        PhotonNetwork.Instantiate(prefabname, new Vector3(0, 9.483022f, 0), Quaternion.identity, 0); //플레이어 프리팹을 0,5,0 위치에 생성한다
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.DestroyPlayerObjects(otherPlayer);
    }
    public override void OnJoinedRoom()
    {
        //loadPrefabs();
        Debug.Log("join room");
    }
    public override void OnLeftRoom()
    {
        savePrefabs();
        
        SceneManager.LoadScene("Login");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //savePrefabs();
    }
    public void savePrefabs()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.GetReference("prefabs/"); // prefabs 위치 참조
        reference.Child(LoginManager.nickname).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                Debug.Log("삭제 실패");
            }
            else if(task.IsCompleted)
            {
                Debug.Log("삭제 완료");
            }
        });

        remainPrefabs = null;
        remainPrefabs = GameObject.FindGameObjectsWithTag("prefabs");
        Debug.Log(remainPrefabs.Length);
        for (int i = 0; i < remainPrefabs.Length; i++)
        {
            PrefabInfo remainPrefabInfos = new PrefabInfo(remainPrefabs[i].name, remainPrefabs[i].transform.position, remainPrefabs[i].transform.rotation);
            string json = JsonUtility.ToJson(remainPrefabInfos);
            reference.Child(LoginManager.nickname).Child(i.ToString()).SetRawJsonValueAsync(json);

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
                Debug.Log(snapshot);
                foreach (DataSnapshot data in snapshot.Children) // snapshot의 각 하위 개체들에 적용
                {
                    string tempName = (string)data.Child("tempName").Value;
                    Debug.Log("got name");
                    IDictionary tempQDic = (IDictionary)data.Child("tempQ").Value;
                    Debug.Log("got Q");
                    IDictionary tempVecDic = (IDictionary)data.Child("tempVector").Value;
                    Debug.Log("got V");

                    float tempx = Convert.ToSingle(tempVecDic["x"]);
                    float tempy = Convert.ToSingle(tempVecDic["y"]);
                    float tempz = Convert.ToSingle(tempVecDic["z"]);
             
                    Vector3 tempPosition = new Vector3(tempx,tempy,tempz);
                    Quaternion tempQ = new Quaternion(Convert.ToSingle(tempQDic["x"]), Convert.ToSingle(tempQDic["y"]), Convert.ToSingle(tempQDic["z"]), Convert.ToSingle(tempQDic["w"]));
                    

                    string name = tempName.Split('(')[0];
                    Debug.Log("origin name is : " + name);

                    for(int i =0;i < prefabs.Length; i++)
                    {
                        if(name == prefabs[i].name)
                            PhotonNetwork.Instantiate(name, tempPosition, tempQ);
                    }
                    Debug.Log("Loading Complete");
                }
                
            }
        });
        
    }

    public void CreatePrefab()
    {
        Debug.Log("Create position is : " + CreateObject.toCreatePosition);
        Instantiate(toCreate, new Vector3(CreateObject.toCreatePosition.x, 9.483022f, CreateObject.toCreatePosition.z), Quaternion.identity);
        preCam.enabled = false;
        mainCam.enabled = true;
        mainCanvas.SetActive(true);
        
        toCreate = null;
        savePrefabs();
    }
    public void CancelCreate()
    {
        preCam.enabled = false;
        mainCam.enabled = true;
        mainCanvas.SetActive(true);
        
    }
    public void DestroyPrefab()
    {
        GameObject tempToD = DestroyMyObject.toDestroy;
        if (tempToD.tag == "prefabs")
        {
            Destroy(tempToD);
            
            preCam.enabled = false;
            mainCam.enabled = true;
            mainCanvas.SetActive(true);
            savePrefabs();

        }
        else
        {
            preCam.enabled = false;
            mainCam.enabled = true;
            mainCanvas.SetActive(true);

        }
    }
    public void CancelDestroy()
    {
        preCam.enabled = false;
        mainCam.enabled = true;
        mainCanvas.SetActive(true);

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
        friendList = new List<string>();
        contents = new GameObject();
        fNickname = friendIF.text; // 내가 검색한 텍스트를 fNickname변수에 넣고
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        FirebaseDatabase.DefaultInstance.GetReference("auto").GetValueAsync().ContinueWithOnMainThread(task =>
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
                Debug.Log("flist length is : "+friendList.Count);
                for(int i = 1; i<friendList.Count+1; i++)
                {
                    contents = GameObject.Find($"Friend" +i.ToString());
                    
                    Text conText = contents.GetComponent<Text>();
                    if(conText.text != LoginManager.nickname)
                    {
                        conText.text = friendList[i - 1];
                    }
                    
                }
                for (int i = 1; i < 11; i++)
                {
                    contents = GameObject.Find($"Friend" + i.ToString());
                    Text conText = contents.GetComponent<Text>();
                    
                    if(conText.text == "")
                    {
                        contents.SetActive(false);
                    }
                }
                backTemp.SetActive(false);
                //string tempFnames = snapshot.Value.ToString();
            }
        });

        

    }

    public void ClickPrefabs()
    {
        mainCam.enabled = false;
        preCam.enabled = true;
        
        GameObject temp = EventSystem.current.currentSelectedGameObject;
        string tempText = temp.transform.parent.name;
        
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (tempText == prefabs[i].name)
                toCreate = prefabs[i];
                
        }
    }

    public void SetAnimation(string animation)
    {
        players = GameObject.FindGameObjectsWithTag("localplayers");
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView temppv = players[i].GetComponent<PhotonView>();
            if (temppv.IsMine)
            {
                thisisme = players[i];
                PV = temppv;
                break;
            }
        }
        if (thisisme)
        {
            PV.RPC("PlayAnimation", RpcTarget.All, animation);
        }
        else
        {
            Debug.Log("내꺼 찾기 실패");
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
        if (scrollRect.GetComponent<RectTransform>().position.x >= 2000f || scrollRect.GetComponent<RectTransform>().position.x <= -2000f)
            scrollRect.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);

        if (scrollRect2.GetComponent<RectTransform>().position.x >= 4500f || scrollRect2.GetComponent<RectTransform>().position.x <= -4500f)
            scrollRect2.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);

    }
}
