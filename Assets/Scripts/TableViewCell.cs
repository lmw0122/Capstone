using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableViewCell<T> : ViewController
{
    public virtual void UpdateContent(T itemData) { }

    public int DataIndex { get; set; }

    public float Height 
    {
        get { return CachedRectTransform.sizeDelta.y; }
        set
        {
            Vector2 sizeDelta = CachedRectTransform.sizeDelta;
            sizeDelta.y = value;
            CachedRectTransform.sizeDelta = sizeDelta;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
