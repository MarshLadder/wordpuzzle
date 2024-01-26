using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    private UIController instance
    {
        get
        {
            return this;
        }
    }
    private void Awake()
    {
        Instance = instance;
    }

    private void OnEnable()
    {
        GameManager.OnLevelEnd += LevelEnd;
    }


    private void OnDisable()
    {
        GameManager.OnLevelEnd -= LevelEnd;
    }

    public GameObject lvlCompletionScreen;
    public TMP_Text completeLevelButtonText;
    

    private void LevelEnd()
    {
        lvlCompletionScreen.SetActive(true);
        completeLevelButtonText.text = "Continue";
    }

    // Start is called before the first frame update
    void Start()
    {
        lvlCompletionScreen.SetActive(false);
    }


    public void ContinueButton()
    {
        lvlCompletionScreen.SetActive(false);
        GameManager.Instance.BacktoMain();
    }

    public void BacktoMainButton()
    {
        GameManager.Instance.BacktoMain();

    }
}
