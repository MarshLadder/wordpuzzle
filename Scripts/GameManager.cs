using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviourSingletonPersistent<GameManager>
{

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnLevelEnd;
    public static event Action ToMainMenu;

    public delegate void LevelStart(InstallDictionary.LevelObject levelObject);
    public event LevelStart OnLevelStart;

    public delegate void SwipeEvent(SwipeDetection.ValidationOutcomes outcome);
    public event SwipeEvent OnSwipeComplete;

    public static int currentlevel = 0;
    public string currentLvlKey = "CurrentLevel";
    public string sceneName;
    public override void Awake()
    {
        base.Awake();
        LoadGameStats();
    }

    public void LoadGameStats()
    {
        if (PlayerPrefs.HasKey(currentLvlKey))
        {
            currentlevel = PlayerPrefs.GetInt(currentLvlKey);
        }
    }

    public void SaveGameStats()
    {
        if (PlayerPrefs.HasKey(currentLvlKey))
        {
            PlayerPrefs.SetInt(currentLvlKey, currentlevel);
        }
        PlayerPrefs.Save();
    }
    private void OnEnable()
    {
        OnLevelEnd += EndLevel;
        ToMainMenu += Flush;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void InitializeLevel()
    {
        Debug.Log("At GameManager.InitializeLevel()");
        InstallDictionary.LevelObject levelObject = InstallDictionary.QueryTableByLevel(currentlevel);
        OnLevelStart?.Invoke(levelObject);
    }

    public void SwipeCompleteEvent(SwipeDetection.ValidationOutcomes result)
    {
        Debug.Log("At GameManager.InvokeSwipeEvent()");
        OnSwipeComplete?.Invoke(result);
    }

    private void OnDisable()
    {
        OnLevelEnd -= EndLevel;
        ToMainMenu -= Flush;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public enum GameState
    {
        Menu,
        Playing,
        Ads,
        Loading,
        Paused
    }
    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (State)
        {
            case GameState.Menu:
                sceneName = "MainMenu";
                break;
            case GameState.Playing:
                sceneName = "Game";
                break;
            case GameState.Ads:
                break;
            case GameState.Loading:
                break;
            case GameState.Paused:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        OnGameStateChanged?.Invoke(newState);

    }

    public void BacktoMain()
    {
        ToMainMenu?.Invoke();


    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneName = scene.name;

        if(sceneName == "Game")
        {
            InitializeLevel();
        }
    }

    public void CheckforLevelEnd()
    {
        if(WordsGenerator.placedWords.Count == WordsGenerator.revealedWords.Count)
        {
            Debug.Log("Level Completed");
            OnLevelEnd?.Invoke();
        }
        else
        {
            Debug.Log(WordsGenerator.placedWords.Count - WordsGenerator.revealedWords.Count +" words to go");
        }
    }

    private void EndLevel()
    {
        currentlevel++;
        SaveGameStats();
        WordsGenerator.Initialize();
        //Loadlevel(sceneName);
    }

    private void Flush()
    {
        SaveGameStats();
        UpdateGameState(GameManager.GameState.Menu);
        Loadlevel(GameManager.Instance.sceneName);
        WordsGenerator.Clear();
    }
    public void Loadlevel(string scene)
    {
        SceneManager.LoadScene(scene);
    }


}
