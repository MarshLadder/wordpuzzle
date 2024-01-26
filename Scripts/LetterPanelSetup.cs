using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class LetterPanelSetup : MonoBehaviour
{
    int numberOfElements;
    public GameObject letterButton;
    private List<GameObject> letterObjList;
    float circleRadius;
    List<char> letters = null;
    private void Awake()
    {
        WordsGenerator.Initialize();
    }
    private void OnEnable()
    {
        circleRadius = gameObject.GetComponent<CircleCollider2D>().radius;
        if (letterButton == null)
        {
            Debug.LogError("Please assign the Image UI element prefab.");
            return;
        }
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the hierarchy.");
            return;
        }
        GameManager.Instance.OnLevelStart += Initialize;
        GameManager.OnLevelEnd += LevelEnd;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnLevelStart -= Initialize;
        GameManager.OnLevelEnd -= LevelEnd;
    }

    private void LevelEnd()
    {
        if (letterObjList != null)
        {
            foreach (GameObject letter in letterObjList)
            {
                Destroy(letter);
            }
            letterObjList.Clear();
        }

        letters = null;
    }

    void Initialize(InstallDictionary.LevelObject levelObject)
    {
        Debug.Log("LetterPanelInitialize():"+ levelObject.AssociatedWords);
        if (letterObjList != null)
        {
            foreach(GameObject letter in letterObjList)
            {
                Destroy(letter);
            }
            letterObjList.Clear();
        }

        SetupLetterPanel(GetUniqueLetters(levelObject.AssociatedWords).ToList());

    }
    private void SetupLetterPanel(List<char> letters)
    {
        numberOfElements = letters.Count;
        float angleStep = 360.0f / numberOfElements;

        for (int i = 0; i < numberOfElements; i++)
        {
            float angle = i * angleStep;
            float radians = Mathf.Deg2Rad * angle;

            float x = circleRadius * Mathf.Cos(radians);
            float y = circleRadius * Mathf.Sin(radians);

            Vector2 position = new Vector2(x, y);

            // Instantiate the Image UI element at the calculated position
            GameObject imageElement = Instantiate(letterButton, gameObject.transform);
            RectTransform imageTransform = imageElement.GetComponent<RectTransform>();
            imageTransform.anchoredPosition = position;
            imageElement.GetComponent<Button>().interactable = false;
            imageElement.GetComponentInChildren<RectTransform>().sizeDelta = imageTransform.sizeDelta;
            imageElement.GetComponentInChildren<TMP_Text>().text = letters[i].ToString().ToLower();
            if (letterObjList == null)
            {
                letterObjList = new List<GameObject>();
            }
            letterObjList.Add(imageElement);
        }
    }



    static char[] GetUniqueLetters(string associatedWords)
    {
        // Extract unique letters from all words
        return associatedWords.Replace(",", "")
                              .Distinct()
                              .OrderBy(letter => letter)
                              .ToArray();
    }

    //void InitializeDailyChallenge()
    //{
    //    if (letterObjList != null)
    //    {
    //        foreach (GameObject letter in letterObjList)
    //        {
    //            Destroy(letter);
    //        }
    //        letterObjList.Clear();
    //    }

    //    letters = null;
    //    StartCoroutine(SetupLettersCoroutineforDailyChallenge());
    //}
    //IEnumerator SetupLettersCoroutineforDailyChallenge()
    //{
    //    int maxAttempts = 2000; // Define a maximum number of attempts
    //    int attempts = 0;
    //    int threshold = 0;

    //    while (letters == null && attempts < maxAttempts)
    //    {
    //        letters = WordsGenerator.GenerateRandomLetterSet();
    //        attempts++;
    //        threshold++;
    //        if (threshold > 200)
    //        {
    //            Debug.Log("threshold exceeded");
    //            //WordsGenerator.SetLevelProperties();
    //            threshold = 0;
    //        }
    //        yield return null; // Wait for one frame before the next attempt
    //    }

    //    if (letters == null)
    //    {
    //        Debug.LogError("Failed to generate letters after " + maxAttempts + " attempts.");
    //        yield break; // Exit the coroutine
    //    }
    //    Debug.Log("took " + attempts + " attempts");
    //    foreach (char letter in letters)
    //    {
    //        Debug.Log("final letter and word : " + letter);
    //    }
    //    numberOfElements = letters.Count;
    //    float angleStep = 360.0f / numberOfElements;

    //    for (int i = 0; i < numberOfElements; i++)
    //    {
    //        float angle = i * angleStep;
    //        float radians = Mathf.Deg2Rad * angle;

    //        float x = circleRadius * Mathf.Cos(radians);
    //        float y = circleRadius * Mathf.Sin(radians);

    //        Vector2 position = new Vector2(x, y);

    //        // Instantiate the Image UI element at the calculated position
    //        GameObject imageElement = Instantiate(letterButton, gameObject.transform);
    //        RectTransform imageTransform = imageElement.GetComponent<RectTransform>();
    //        imageTransform.anchoredPosition = position;
    //        imageElement.GetComponent<Button>().interactable = false;
    //        imageElement.GetComponentInChildren<RectTransform>().sizeDelta = imageTransform.sizeDelta;
    //        imageElement.GetComponentInChildren<TMP_Text>().text = letters[i].ToString();
    //        if (letterObjList == null)
    //        {
    //            letterObjList = new List<GameObject>();
    //        }
    //        letterObjList.Add(imageElement);
    //    }
    //}

    //auto generation for daily challenge


}


