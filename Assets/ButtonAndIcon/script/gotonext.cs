using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gotonext : MonoBehaviour
{
	private int LevelNext;
	private bool stop = false;

	void Start() {
		LevelNext = PlayerPrefs.GetInt("LevelNum");
		PlayerPrefs.SetInt("LevelNum", LevelNext);
		PlayerPrefs.Save();
		Debug.Log(LevelNext);
	}
	void FixedUpdate()
	{
		if (Input.GetKeyUp(KeyCode.Space))
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
	}

	  void OnApplicationQuit() { 
		PlayerPrefs.SetInt("LevelNum", 0);
		PlayerPrefs.Save();
	}

}
