﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PauseMenu : MonoBehaviour {

    public AudioClip clickSound;
    public AudioClip pauseSound;
    public GameObject pauseMenuUI;
	public Button ResumeButton;
	public Button QuitButton;
	private AudioSource menuSounds;
	private TutorialPopups tutPopups;
	private PlayerControls playerScript;

    [HideInInspector] public bool GameIsPaused = false;

    // Use this for initialization
    private void Start()
    {
        menuSounds = gameObject.GetComponent<AudioSource>();
		tutPopups = GameObject.Find("Controller").GetComponent<TutorialPopups>();
		playerScript = GameObject.Find("Player").GetComponent<PlayerControls>();
    }


    // Update is called once per frame
    void Update () {

		if (Input.GetButtonDown("Pause") && !playerScript.Dead)
		{
			if (GameIsPaused)
			{
				Resume();
			}
			else
			{
				Pause();
			}
		}

	}
	
	public void Pause()
	{
		if (!playerScript.Dead)
		{
			pauseMenuUI.SetActive(true);
			menuSounds.clip = pauseSound;
			menuSounds.loop = false;
			menuSounds.Play();
			Time.timeScale = 0f;
			GameIsPaused = true;
			if (Application.platform == RuntimePlatform.IPhonePlayer
			|| Application.platform == RuntimePlatform.Android)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			else
			{
				ResumeButton.Select();
			}
		}
	}

	public void Resume()
	{
		if (!playerScript.Dead)
		{
			pauseMenuUI.SetActive(false);
			menuSounds.clip = clickSound;
			menuSounds.loop = false;
			menuSounds.Play();
			if (!tutPopups.TutorialPopupPause)
			{
				Time.timeScale = 1f;
			}
			if (tutPopups.TutorialPopupPause && Application.platform != RuntimePlatform.IPhonePlayer
			&& Application.platform != RuntimePlatform.Android)
			{
				tutPopups.currentButton.Select();
			}
			GameIsPaused = false;
		}
	}

	public void LoadMenu()
	{
		Time.timeScale = 1f;
		pauseMenuUI.SetActive(false);
		SceneManager.LoadScene("StartMenu");
	}
}

