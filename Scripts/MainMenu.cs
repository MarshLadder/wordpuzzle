using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject levelLoadingScreen,mainMenuScreen;
    public Button startButton;
    string sceneToActivate;
    bool buttonActivated = false;
    public TMP_Text loadingText,levelnumberText;
    public TMP_Text playButtonText;

    
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.LoadGameStats();
        CloseMenus();
        mainMenuScreen.SetActive(true);
        if(GameManager.currentlevel != 0)
        {
            playButtonText.text = "continue level " + GameManager.currentlevel;
        }
        else
        {
            playButtonText.text = "";
            GameManager.currentlevel=1;
        }

    }


    private void CloseMenus()
    {
        levelLoadingScreen.SetActive(false);
        mainMenuScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSceneLoaded(sceneToActivate) && !buttonActivated)
        {
            
            Debug.Log("Scene is Loaded");
            buttonActivated = true;
            loadingText.text = "Ready";
            startButton.gameObject.SetActive(true);

        }
    }

    void OnClickPlay()
    {
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        sceneToActivate = GameManager.Instance.sceneName;
        CloseMenus();
        levelLoadingScreen.SetActive(true);
        levelnumberText.text += "" + GameManager.currentlevel;
        SceneManager.LoadScene(sceneToActivate, LoadSceneMode.Additive);

    }

    public void ActivateScene()
    {
        Scene scene = SceneManager.GetSceneByName(sceneToActivate);
        SceneManager.SetActiveScene(scene);
        SceneManager.UnloadSceneAsync(0);
        
        Debug.Log(SceneManager.GetActiveScene().name);
    }
    public bool IsSceneLoaded(string sceneName)
    {
        
        Scene scene = SceneManager.GetSceneByName(sceneName);
        // Check if the scene is valid and loaded.
        return scene.IsValid() && scene.isLoaded;
    }
}
