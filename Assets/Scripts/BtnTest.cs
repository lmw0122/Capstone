using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnTest : MonoBehaviour
{
    public InputField message;
    public Button thisBtn;
    public GameObject client;
    // Start is called before the first frame update
    void Start()
    {
        thisBtn.onClick.AddListener(sendTest);
    }

    void sendTest()
    {
        Debug.Log("message is : " + message.text);
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
