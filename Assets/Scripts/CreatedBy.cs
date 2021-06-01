using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatedBy : MonoBehaviour
{
    public string image;
    public string name;
    public string video;

    public CreatedBy()
    {

    }
    public CreatedBy(string image, string name, string video)
    {
        this.image = image;
        this.name = name;
        this.video = video;
    }
}
