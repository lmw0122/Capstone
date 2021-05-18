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
        if (PhotonNetwork.CurrentRoom.Name != LoginManager.nickname) //�� ���� �ƴ϶�� �ڷΰ��� ��ư�� Ȱ��ȭ �Ѵ�.
        {
            backButton.SetActive(true);
        }
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
        // �� ���� ����-> ������ ���� �����Ϳ� ���ӵǰ� LoginManager�� OnConnectedToMaster �ȿ��� fNickname ������ ģ�� �� ���� 

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
}
