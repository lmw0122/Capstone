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
using TMPro;
using System.IdentityModel.Tokens.Jwt;
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

    public static GameObject toCreate;

    bool socketReady;
    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    public string URLforsend;
    public string URLforme;

    public string imageUrl = "";
    private string token;
    public Text infoText;
    public TMP_InputField textIF;
    public PhotonView PV;
    public RawImage testImage;
    public ScrollRect chatRect;
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
        
        if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname) //�� ���� �ƴ϶�� �ڷΰ��� ��ư�� Ȱ��ȭ �Ѵ�.
        {
            backButton.SetActive(true);
            
        }
        SpawnPlayer();
        if(PhotonNetwork.CurrentRoom.Name == LoginManager.nickname)
        {
            loadPrefabs();
        }
        setUserinfo();
        URLforme = "https://project-6629124072636312930.web.app/info/" + LoginManager.nickname;
        URLforsend = "https://project-6629124072636312930.web.app/main/" + LoginManager.nickname;
        //�̸� ����� ���� player �������� ��ȯ�ϴ� �Լ�
    }
    public void setUserinfo()
    {
        Debug.Log("name is : "+LoginManager.nickname);
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userifo.GetComponentInChildren<Text>().text = LoginManager.nickname;
        reference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("aht qnffjdhkTdma");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; // users�� ���� ����� snapshot���� �޾ƿ�
                foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
                {
                    string tempName = (string)data.Child("name").Value;
                    Debug.Log("temp name is :" + tempName);
                    if (tempName.Equals(LoginManager.nickname))
                    {
                        imageUrl = (string)data.Child("image").Value;
                        Debug.Log("image url : " + imageUrl);
                        StartCoroutine(CoLoadImageTexture(imageUrl));
                        //Debug.Log("texture is : " + testImage.texture.ToString());
                        Texture tempTextture = testImage.texture;
                        userifo.GetComponentInChildren<RawImage>().texture = tempTextture;
                    }
                    Debug.Log("iURL : " + imageUrl);
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
    private void GetImage(string mastername)
    {
        

    }
    public void ShowMessage()
    {
        string mess = textIF.text.ToString();
        textIF.text = "";
        Debug.Log("mess is : " + mess);
        players = GameObject.FindGameObjectsWithTag("localplayers");
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView temppv = players[i].GetComponent<PhotonView>();
            if (temppv.IsMine)
            {
                thisisme = players[i];
                PV = temppv;
                Debug.Log("���� ã�� ����");
            }
        }
        if (thisisme)
        {
            PlayerManager tempPM = thisisme.GetComponent<PlayerManager>();
            tempPM.showChat(mess);
        }
        else
        {
            Debug.Log("���� ã�� ����");
        }

        //ä�ù� ��ġ �ʱ�ȭ + ��ũ�� ���̰�
        
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
                Debug.Log("���� ã�� ����");
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
            Debug.Log("���� ����");
            infoText.text = "���� ����";
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log($"���� ���� : {e.Message}");
        }

        URLforme = URLforme + "/" + PhotonNetwork.CurrentRoom.Name;
        URLforsend = URLforsend + "/" + PhotonNetwork.CurrentRoom.Name;
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
        PhotonNetwork.Instantiate("Mouse", new Vector3(0,10f,0), Quaternion.identity, 0); //�÷��̾� �������� 0,5,0 ��ġ�� �����Ѵ�.
        

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
        reference = FirebaseDatabase.DefaultInstance.GetReference("prefabs/"+LoginManager.nickname); // prefabs ��ġ ����
        remainPrefabs = null;
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
                Debug.Log("������ �ҷ����� ����");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
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
        fNickname = friendIF.text; // ���� �˻��� �ؽ�Ʈ�� fNickname������ �ְ�
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-6629124072636312930-default-rtdb.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        FirebaseDatabase.DefaultInstance.GetReference("rooms").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("ģ�� �ҷ����� ����");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot data in snapshot.Children) // snapshot�� �� ���� ��ü�鿡 ����
                {
                    string tempFname = (string)data.Key;
                    Debug.Log("fname is : " + tempFname);
                    if (tempFname.Contains(fNickname))
                    {
                        if(tempFname != LoginManager.nickname)
                        {
                            friendList.Add(tempFname);
                            Debug.Log("�迭�� ����");
                        }
                        
                    }

                }
                //FCanvas.SetActive(true);
                for(int i = 1; i<friendList.Count+1; i++)
                {
                    contents = GameObject.Find($"Friend" +i.ToString());
                    Text conText = contents.GetComponent<Text>();
                    if(conText.text != LoginManager.nickname)
                    {
                        conText.text = friendList[i - 1];
                    }
                    
                }
                for (int i = 1; i < 10; i++)
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
