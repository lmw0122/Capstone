using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class touchTotext : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	private int LevelNext;
	private bool stop = false;

	void Start() { 
	LevelNext = PlayerPrefs.GetInt("LevelNum") + 0;
	}

	public void OnPointerDown(PointerEventData eventData)
	{

		stop = true;
			if (LevelNext <= 7 && stop == true)
			{
				if (PlayerPrefs.HasKey("LevelNum"))
				{
					LevelNext += 1;
					PlayerPrefs.SetInt("LevelNum", LevelNext);
					PlayerPrefs.Save();
					Debug.Log("next");
					Application.LoadLevel(LevelNext);
					stop = false;
				}
			}
			if (LevelNext > 7) {
				PlayerPrefs.SetInt("LevelNum",0);
				PlayerPrefs.Save();
				Application.LoadLevel(0);
			}

	}

	public void OnPointerUp(PointerEventData eventData)
	{
		
	}
}
