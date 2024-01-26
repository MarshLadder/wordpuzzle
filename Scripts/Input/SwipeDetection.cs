using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using System.Text;
using System;

public class SwipeDetection : MonoBehaviourSingleton<SwipeDetection>
{

    private InputManager inputManager;
    [SerializeField]
    private GameObject LetterPanel;

    private Coroutine coroutine;

    [SerializeField]
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    [SerializeField]
    EventSystem m_EventSystem;
    List<RectTransform> linepositions;
    [SerializeField]
    LineRenderer lineRenderer;
    [SerializeField]
    RectTransform myCanvasRectTransform;

    [SerializeField]
    private Button letterButton;
    bool buttonclickregistered = false;
    public delegate void LetterButtonClick(List<Button> selectedButtons);
    public event LetterButtonClick OnLetterButtonSelect;

    string letterPattern;
    List<Button> clickedbuttons;

    InstallDictionary.LevelObject levelObject;
    PlayerStats playerStats;


    public enum ValidationOutcomes
    {
        NullReference,
        success,
        fail,
        none,
        otherword,
        defaultOutcome
    }
    // Start is called before the first frame update
    void Awake()
    {
        #region fortestonly
        if (GameObject.Find("InputManager") == null)
        {
            GameObject inputManagerTemp = new GameObject();
            inputManagerTemp.AddComponent<InputManager>();
        }

        if (GameObject.Find("GameManager") == null)
        {
            GameObject gameManagerTemp = new GameObject();
            gameManagerTemp.AddComponent<GameManager>();
        }
        if (GameObject.Find("PlayerStatsManager") == null)
        {
            GameObject gameManagerTemp = new GameObject();
            gameManagerTemp.AddComponent<PlayerStatsManager>();
        }
        #endregion fortestonly
        inputManager = InputManager.Instance;
        
    }

    private void Start()
    {
        linepositions = new List<RectTransform>();
        

    }
    private void OnEnable()
    {
        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;

        //OnLetterButtonSelect += () =>
        //{
        //    letterPattern += LetterButtonHandler();
        //};
        OnLetterButtonSelect += LetterButtonHandler;

        GameManager.Instance.OnLevelStart += OnLevelStart;
        GameManager.OnLevelEnd += OnLevelEnd;


    }

    private void OnLevelEnd()
    {
        PlayerStatsManager.Instance.IncreaseInGameCurrency(playerStats);
    }

    private void OnLevelStart(InstallDictionary.LevelObject lo)
    {
        levelObject = lo;
        playerStats = new PlayerStats();
    }

    private void OnDisable()
    {
        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;

        //OnLetterButtonSelect -= () =>
        //{
        //    letterPattern += LetterButtonHandler();
        //};
        OnLetterButtonSelect -= LetterButtonHandler;
    }

    void SwipeStart(Vector2 position)
    {
        clickedbuttons = new List<Button>();
        coroutine = StartCoroutine(SwipePattern());
    }

    IEnumerator SwipePattern()
    {
        while (true)
        {
            if (clickedbuttons != null)
            {

                //Debug.Log("SwipePattern(): currentPos" + InputManager.Instance.PrimaryPosition());
                m_PointerEventData = new PointerEventData(m_EventSystem);
                
                //Set the Pointer Event Position to that of the mouse position
                m_PointerEventData.position = inputManager.PrimaryPosition();

                //Create a list of Raycast Results
                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                m_Raycaster.Raycast(m_PointerEventData, results);
                //Debug.Log("Result Count:::" + results.Count);
                //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                foreach (RaycastResult result in results)
                {

                    if (result.gameObject.CompareTag("letterbutton"))
                    {

                        letterButton = result.gameObject.GetComponent<Button>();
                        //this check determines a change in selected button by the pointer and also checks if current button has not been previously selected


                        buttonclickregistered = true;

                        //select sequence
                        if (!clickedbuttons.Contains(letterButton))
                        {
                            Debug.Log("added to clicked buttons");
                            clickedbuttons.Add(letterButton);

                            linepositions.Add(result.gameObject.GetComponent<RectTransform>());
                            lineRenderer.positionCount = linepositions.Count + 1;

                            for (int i = 0; i < linepositions.Count; i++)
                            {
                                lineRenderer.SetPosition(i, linepositions[i].anchoredPosition);
                            }
                           OnLetterButtonSelect?.Invoke(clickedbuttons);
                        }

                        //unselect sequence
                        else if (clickedbuttons.Count>1 && clickedbuttons[clickedbuttons.Count - 2] == letterButton)
                        {

                            clickedbuttons.RemoveAt(clickedbuttons.Count - 1);

                            linepositions.RemoveAt(linepositions.Count - 1);
                            lineRenderer.positionCount = linepositions.Count + 1;

                            for (int i = 0; i < linepositions.Count; i++)
                            {
                                lineRenderer.SetPosition(i, linepositions[i].anchoredPosition);
                            }
                            OnLetterButtonSelect?.Invoke(clickedbuttons);
                        }
                    }
                }
            }
            yield return null;
        }
    }
    void SwipeEnd(Vector2 position)
    {
        StopCoroutine(coroutine);
        lineRenderer.positionCount = 0;
        linepositions.Clear();

        if (buttonclickregistered)
        {
            Validation();
        }
        buttonclickregistered = false;
        
    }

    private void Update()
    {
        if (buttonclickregistered)
        {

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(inputManager.PrimaryPosition());
            Vector2 anchoredPosition = myCanvasRectTransform.InverseTransformPoint(mouseWorldPosition);
            lineRenderer.SetPosition(linepositions.Count, anchoredPosition);
        }
    }



    void LetterButtonHandler(List<Button> selectedButtons)
    {

        //Add Logic to Add Visual and Audio Effects for Select, Unselect scenarios
        //
        letterPattern = ConcatenateButtonText(selectedButtons);
        Debug.Log("LetterButtonHandler(): " + letterPattern);
    }

    string ConcatenateButtonText(List<Button> buttons)
    {

        // Use StringBuilder to concatenate text from child components of buttons without spaces
        StringBuilder stringBuilder = new StringBuilder();

        foreach (Button button in buttons)
        {
            if (button != null)
            {
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    stringBuilder.Append(buttonText.text);
                }
            }
        }

        return stringBuilder.ToString();
    }
    void Validation()
    {

        ValidationOutcomes result = ValidatePattern();
        Debug.Log("letterPattern in Validation: " + letterPattern);
        GameManager.Instance.SwipeCompleteEvent(result);
        switch (result)
        {
            case ValidationOutcomes.success:
                var results = CrosswordPanel.RevealGrid(WordsGenerator.placedWords.IndexOf(letterPattern));
                bool cleanreveal = results.cleanreveal;
                bool bonusreveal = results.bonusreveal;
                if (levelObject.MainWordLength != letterPattern.Length)
                {
                    cleanreveal = false;
                }
                int currency = letterPattern.Length;

                int xp = 1;

                playerStats.UpdateStat(currency,xp,cleanreveal,bonusreveal);
                
                GameManager.Instance.CheckforLevelEnd();
               Debug.Log("Success");
                break;
            case ValidationOutcomes.fail:
                Debug.Log("fail");
                break;
            case ValidationOutcomes.none:
                Debug.Log("Already Existing Word");
                break;
            case ValidationOutcomes.defaultOutcome:
                Debug.Log("Default");
                break;
            case ValidationOutcomes.otherword:
                Debug.Log("other word detected");
                break;
            case ValidationOutcomes.NullReference:
                Debug.Log("Null Exception Encountered while accessing one of the static lists");
                break;
        }
        letterPattern = string.Empty;
    }



    ValidationOutcomes ValidatePattern()
    {
       // Debug.Log("ValidatePattern(): "+letterPattern);

        if (WordsGenerator.revealedWords == null || WordsGenerator.placedWords == null || WordsGenerator.placedWordsGridPos == null)
        {
            return ValidationOutcomes.NullReference;
        }
        else if (WordsGenerator.revealedWords.Contains(letterPattern))
        {
            return ValidationOutcomes.none;
        }
        else if (WordsGenerator.placedWords.Contains(letterPattern))
        {
            return ValidationOutcomes.success;
        }
        else if (WordsGenerator.IsWordValid(letterPattern)) 
        {
            return ValidationOutcomes.otherword;
        }
        else
        {
            return ValidationOutcomes.fail;
        }
    }

}
