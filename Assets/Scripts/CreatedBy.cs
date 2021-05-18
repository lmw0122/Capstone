using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatedBy : MonoBehaviour
{
    public string image;
    public string name;

    public CreatedBy()
    {

    }
    public CreatedBy(string image, string name)
    {
        this.image = image;
        this.name = name;
    }
}
