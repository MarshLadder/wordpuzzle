using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CrosswordPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject _crossword;
    [SerializeField]
    private GameObject ImagePrefab;
    Grid _grid;
    private List<GameObject> gridObjList;



    private void OnEnable()
    {
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
        if (gridObjList != null)
        {
            foreach (GameObject gridObj in gridObjList)
            {
                Destroy(gridObj);
            }
            gridObjList.Clear();
        }
    }

    private void Initialize(InstallDictionary.LevelObject levelObject)
    {
        string words = levelObject.AssociatedWords.ToLower();
        Debug.Log("CrossWordPanelInitialize():" + words);
        if (gridObjList != null)
        {
            foreach(GameObject gridObj in gridObjList)
            {
                Destroy(gridObj);
            }
            gridObjList.Clear();
        }

        _grid = _crossword.GetComponent<Grid>();
        WordsGenerator.GenerateWordGrid(GetWordsList(words));
        DisplayGrid(WordsGenerator.grid);
    }

    static List<string> GetWordsList(string associatedWords)
    {
        // Split the associatedWords string into a list of words and order by length
        return associatedWords.Split(',')
                              .OrderByDescending(word => word.Length)
                              .ToList();
    }



    void DisplayGrid(char[,] twoDArray)
    {
        float gridsize = Mathf.Sqrt(twoDArray.Length);
        _crossword.GetComponent<RectTransform>().sizeDelta = _crossword.GetComponent<RectTransform>().sizeDelta / gridsize;
        //Debug.Log("DisplayGrid(): cellsize : " + cellsize + " and GridSize: "+gridsize);
        Vector3Int position = Vector3Int.zero;
        for (int i = 0; i < gridsize; i++)
        {
            for (int j = 0; j < gridsize; j++)
            {
                //position = new Vector3Int((int)cellsize.x + i, (int)cellsize.y + j, (int)transform.position.z);
                position = new Vector3Int(/*(int)gridsize - 1 - */j, (int)gridsize - 1 -  i, (int)transform.position.z);
                //Debug.Log("Position: " + position);
                Vector3 letterpos = _grid.GetCellCenterWorld(position);
                GameObject letter = Instantiate(ImagePrefab, letterpos, Quaternion.identity, _crossword.transform);
                letter.GetComponentInChildren<TMP_Text>().text = twoDArray[i, j].ToString();
                letter.transform.Find("Text (TMP)").gameObject.SetActive(false);
                if (WordsGenerator.CheckBonusRanges(i, j))
                {
                    letter.transform.Find("Image").gameObject.SetActive(true);
                }
                letter.gameObject.name = i +""+ j;
                if (twoDArray[i,j]==' ')
                {
                    letter.SetActive(false);
                }

                if (gridObjList == null)
                {
                    gridObjList = new List<GameObject>();
                }
                gridObjList.Add(letter);
            }
        }
    }

    public static (bool cleanreveal, bool bonusreveal) RevealGrid(int index)
    {
        bool cleanreveal = true;
        bool bonusreveal = false;
        WordsGenerator.revealedWords.Add(WordsGenerator.placedWords[index]);
        int[] gridpos = WordsGenerator.placedWordsGridPos[index];
        int startRow = gridpos[0]; 
        int startCol = gridpos[1];
        int finalRow = gridpos[2];
        int finalCol = gridpos[3];

        for(int i = startRow; i <= finalRow; i++)
        {
            for (int j = startCol; j <= finalCol; j++)
            {
                GameObject lettertext = GameObject.Find(i + "" + j).transform.Find("Text (TMP)").gameObject;
                GameObject bonusImage = GameObject.Find(i + "" + j).transform.Find("Bonus_Image").gameObject;

                if (bonusImage.activeInHierarchy)
                {
                    bonusreveal = true;
                }
                if (!lettertext.activeInHierarchy)
                {
                    //reveal happens here
                    lettertext.SetActive(true);
                }
                else
                {
                    cleanreveal = false;
                }
            }
        }
        return (cleanreveal, bonusreveal);

    }

    //private void InitializeDailyChallenge()
    //{

    //    if (gridObjList != null)
    //    {
    //        foreach (GameObject gridObj in gridObjList)
    //        {
    //            Destroy(gridObj);
    //        }
    //        gridObjList.Clear();
    //    }

    //    _grid = _crossword.GetComponent<Grid>();
    //    StartCoroutine(SetupCrossWordGridCoroutineforDailyChallenge());
    //}
    //IEnumerator SetupCrossWordGridCoroutineforDailyChallenge()
    //{
    //    Debug.Log("started CrossWordPanel coroutine");
    //    while (!WordsGenerator.wordsReady)
    //    {
    //        yield return null; // Wait for one frame before the next attempt
    //    }

    //    foreach (string word in WordsGenerator.wordlist)
    //    {
    //        Debug.Log("final letter and word : " + word);
    //    }
    //    WordsGenerator.GenerateWordGrid(WordsGenerator.wordlist);
    //    if (!WordsGenerator.allplaced)
    //    {
    //        yield return null;
    //    }
    //    DisplayGrid(WordsGenerator.grid);
    //    WordsGenerator.wordsReady = false;
    //}
}
