using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;


public class GridRange
{
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public int EndRow { get; set; }
    public int EndCol { get; set; }

    public GridRange(int startRow, int startCol, int endRow, int endCol)
    {
        StartRow = startRow;
        StartCol = startCol;
        EndRow = endRow;
        EndCol = endCol;
    }
}
public class WordsGenerator
{
    
    static string wordListFilePath = "english_words.txt";
    //private static HashSet<string> sourceWordList = LoadWordList(wordListFilePath);
    private static HashSet<string> sourceWordList = new HashSet<string>(InstallDictionary.GetWords(), StringComparer.OrdinalIgnoreCase);
    public static List<string> wordlist = new List<string>();
    public static List<string> placedWords = new List<string>();
    public static List<string> revealedWords = new List<string>();
    public static List<int[]> placedWordsGridPos = new List<int[]>();
    public static List<string> usedPatterns = new List<string>();
    public static HashSet<string> usedWords = new HashSet<string>();
    public static List<GridRange> bonusRange = new List<GridRange>(); 
    static int trial;

    public static int gridSize = 10; // Adjust the size of the grid.

    public static char[,] grid;
    public static bool allplaced=false,wordsReady=false;

    static int numberofchars;
    static int rareConsonantCount;
    static int chanceaddrareconsonant;
    static int vowelCount;

    public void Start()
    {
        //GameManager.OnLevelEnd += Initialize;
        //Initialize();
    }
    public static void Initialize()
    {
        trial = 0;
        Clear();

        
    }

    internal static void Clear()
    {
        Debug.Log("Flushing WordGenerator");
        wordlist.Clear();
        placedWords.Clear();
        placedWordsGridPos.Clear();
        revealedWords.Clear();
        bonusRange.Clear();
    }
    private static HashSet<string> LoadWordList(string filePath)
    {
        try
        {
            string[] words = File.ReadAllLines(filePath);
            return new HashSet<string>(words);
        }
        catch (IOException e)
        {
            Debug.Log("Error loading word list: " + e.Message);
            return new HashSet<string>();
        }
    }

    public static bool IsWordValid(string word)
    {
        return sourceWordList.Contains(word); 
    }

    public static bool IsWordUsed(string word)
    {
        return usedWords.Contains(word);
    }

    private static List<char> vowels = new List<char> { 'a', 'e', 'i', 'o', 'u' };
    private static List<char> commonconsonants = new List<char> { 't', 'n', 's', 'r', 'l', 'd', 'c', 'm', 'h', 'p' };
    private static List<char> rareconsonants = new List<char> { 'x', 'z', 'q', 'j', 'v', 'k', 'b', 'g', 'f', 'w', 'y' };

    public static List<char> GenerateRandomLetterSet()
    {
        wordsReady = false;
        trial++;

        List<char> letterset = new List<char>();

        for (int i = 0; i < rareConsonantCount; i++)
        {
            letterset.Add(rareconsonants[new System.Random().Next(0, rareconsonants.Count)]);
        }
        for (int i = 0; i < vowelCount; i++)
        {
            letterset.Add(vowels[new System.Random().Next(0, vowels.Count)]);
        }
        for(int i=0;i<numberofchars - rareConsonantCount - vowelCount; i++)
        {
            letterset.Add(commonconsonants[new System.Random().Next(0, commonconsonants.Count)]);
        }

        Debug.Log("number of chars " + numberofchars + " vowel count: "+ vowelCount + " rareconsonant: " + rareConsonantCount + " at level " + GameManager.currentlevel);

        List<char> sortedletters = new List<char>(letterset);
        sortedletters.Sort();
        string pattern = new string(sortedletters.ToArray());

        if (usedPatterns.Contains(pattern))
        {
            letterset.Clear();
            Debug.Log("pattern "+pattern+" is already used");
            return null;
        }
        //all possible combis of letterset = check = against IswordValid
        //if bool iswordValid is true then store string word to new stringlist
        //if number of such words < 3, start from creating new letterset

        HashSet<string> allPermutations = GenerateAllPermutationsAndSubsets(letterset);
        
       
        foreach(string permutations in allPermutations)
        {

            if (IsWordValid(permutations) && permutations.Length >= 3)
            {
                //Debug.Log("valid word found: " + permutations);
                wordlist.Add(permutations);
            }
            
        }

        foreach (char letter in letterset)
        {
            Debug.Log("trial" + trial + " letter and word  " + letter);

        }


        if (wordlist.Count < new System.Random().Next(2,3) && GameManager.currentlevel<=5)
        {

            foreach (string permutations in allPermutations)
            {
                if (!wordlist.Contains(permutations))
                {
                    if (IsWordUsed(permutations) && permutations.Length >= 3)
                    {
                        //Debug.Log("valid word found: " + permutations);
                        wordlist.Add(permutations);
                    }
                }
            }
            if (wordlist.Count < new System.Random().Next(2, 3))
            {
                letterset.Clear();
                wordlist.Clear();
                return null;
            }

        } else if((wordlist.Count < 4 && GameManager.currentlevel > 5))
        {
            foreach (string permutations in allPermutations)
            {
                if (!wordlist.Contains(permutations))
                {
                    if (IsWordUsed(permutations) && permutations.Length >= 3)
                    {
                        //Debug.Log("valid word found: " + permutations);
                        wordlist.Add(permutations);
                    }
                }
            }
            if (wordlist.Count < 4)
            {
                letterset.Clear();
                wordlist.Clear();
                return null;
            }
        }


        usedPatterns.Add(pattern);


        wordlist = wordlist.OrderByDescending(word => word.Length).ToList();
        sourceWordList.ExceptWith(wordlist);
        usedWords.UnionWith(wordlist);
        wordsReady = true;
        return letterset;
    }



    static HashSet<string> GenerateAllPermutationsAndSubsets(List<char> letters)
    {
        HashSet<string> permutations = new HashSet<string>();
        GeneratePermutationsAndSubsetsRecursive(letters, 0, permutations);
        return permutations;
    }

    static void GeneratePermutationsAndSubsetsRecursive(List<char> letters, int startIndex, HashSet<string> permutations)
    {
        int n = letters.Count;

        if (startIndex == n)
        {
            return;
        }

        for (int i = startIndex; i < n; i++)
        {
            Swap(letters, startIndex, i);
            permutations.Add(new string(letters.GetRange(0, startIndex + 1).ToArray()));
            GeneratePermutationsAndSubsetsRecursive(letters, startIndex + 1, permutations);
            Swap(letters, startIndex, i); // Restore the original order
        }

        GeneratePermutationsAndSubsetsRecursive(letters, startIndex + 1, permutations);
    }

    static void Swap(List<char> letters, int i, int j)
    {
        char temp = letters[i];
        letters[i] = letters[j];
        letters[j] = temp;
    }

    public static void GenerateWordGrid(List<string> words)
    {
        CalculateGridSize(words);
        InitializeGrid();
        PlaceWords(words);
    }


    static void CalculateGridSize(List<string> words)
    {
        int totalChars = 0;
        foreach (string word in words)
        {
            totalChars += word.Length;
        }

        int gridSizeSqrt = Mathf.CeilToInt(Mathf.Sqrt(totalChars)); // Calculate a square grid size.
        gridSizeSqrt = Mathf.Max(gridSizeSqrt, 5); // Set a minimum grid size (e.g., 5x5).

        // Ensure that the grid size is at least as large as the longest word.
        foreach (string word in words)
        {
            gridSizeSqrt = Mathf.Max(gridSizeSqrt, word.Length);
        }

        gridSizeSqrt = Mathf.NextPowerOfTwo(gridSizeSqrt); // Round up to the nearest power of two for simplicity.

        gridSize = gridSizeSqrt;

        Debug.Log("CalculateGridSize()::gridsize= " + gridSize);
    }
    static void InitializeGrid()
    {
        grid = new char[gridSize, gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                //if (i == 0 && j == 0)
                //{
                //    grid[i, j] = 'S';
                //}
                //else if (i == gridSize - 1 && j == gridSize - 1)
                //{
                //    grid[i, j] = 'E';
                //}
                //else if (i == 0 && j == gridSize - 1)
                //{
                //    grid[i, j] = 'C';
                //}
                //else if (i == gridSize - 1 && j == 0)
                //{
                //    grid[i, j] = 'R';
                //}
                //else
                //{
                grid[i, j] = ' ';
                //}
            }
        }
    }

    static void PlaceWords(List<string> words)
    {
        List<string> postponedWords = new List<string>();
        List<GridRange> placedRangesHorizontal = new List<GridRange>();
        List<GridRange> placedRangesVertical = new List<GridRange>();
        bool horizontal = UnityEngine.Random.value > 0.5f;

        string lastWord = null;
        string nextWord = null;
        int[] finalpos = null;
        int row = -1;
        int col = -1;
        for(int i = 0; i < words.Count; i++)
        {

            nextWord = words[i];

            while (!(CanPlaceWord(nextWord, row, col, horizontal)))
            {
                if (lastWord == null) //place firstword
                {
                    if (horizontal)
                    {
                        row = UnityEngine.Random.Range(gridSize/3, 2*gridSize/3);
                        col = UnityEngine.Random.Range(gridSize/3, gridSize-nextWord.Length);
                    }
                    else
                    {
                        col = UnityEngine.Random.Range(gridSize / 3, 2*gridSize/3);
                        row = UnityEngine.Random.Range(gridSize/3, gridSize - nextWord.Length);
                    }
                }
                else
                {
                    int[] newPosition = DetermineNewPos(nextWord, horizontal, placedRangesHorizontal, placedRangesVertical);
                    //return lastWord gridposition and assign it to row , col
                    if (newPosition != null)
                    {
                        row = newPosition[0];
                        col = newPosition[1];
                        //Debug.Log("Row: " + row + " Col: " + col);
                        if (row == -1)
                        {
                            if (UnityEngine.Random.Range(0f, 100f) < 2f)
                            {
                                row = UnityEngine.Random.Range(0, gridSize);
                                col = UnityEngine.Random.Range(0, gridSize);
                                //Debug.Log("no valid range found: randomizing ::row: " + row + " col: " + col);
                            }
                            else
                            {
                                //Debug.Log("postponing to end as no valid range found");
                                words.Add(nextWord);
                                if (UnityEngine.Random.Range(0f, 1.0f) > 0.5f)
                                {
                                    horizontal = !horizontal;
                                }
                                break;
                            }

                        }
                    }
                    else
                    {
                        row = UnityEngine.Random.Range(0, gridSize);
                        col = UnityEngine.Random.Range(0, gridSize);
                       // Debug.Log("nullposition: randomizing ::row: " + row + " col: " + col);
                    }

                }
            }

            if (row != -1)
            {
                finalpos = PlaceWord(nextWord, row, col, horizontal);
               //Debug.Log("For current word: " + nextWord + "placed new pos between : " + row + "," + col + "Final Position is : " + finalpos[0]+","+finalpos[1]);
                if (horizontal) placedRangesHorizontal.Add(new GridRange(row, col, finalpos[0], finalpos[1]));
                else placedRangesVertical.Add(new GridRange(row, col, finalpos[0], finalpos[1]));
                lastWord = nextWord;
                horizontal = !horizontal;
            }
        }
        allplaced = true;

        ListBonusCells(placedRangesHorizontal, placedRangesVertical);
    }

    private static void ListBonusCells(List<GridRange> placedRangesHorizontal, List<GridRange> placedRangesVertical)
    {
        //handle horizontal ranges
        //if(placedranged donot have an intersection at any cell
        //add a cell range to list<GridRange> bonuscells

        foreach(GridRange range in placedRangesHorizontal)
        {
            bool bonusRangeFound = true;
            for(int i = range.StartRow; i <= range.EndRow; i++)
            {
                for(int j = range.StartCol; j <= range.EndCol; j++)
                {
                    if(grid[i+1,j]!=' '|| grid[i - 1, j] != ' ')
                    {
                        bonusRangeFound = false;
                        break;
                    }
                }
            }
            if(bonusRangeFound) bonusRange.Add(range);
        }

        foreach (GridRange range in placedRangesVertical)
        {
            bool bonusRangeFound = true;
            for (int i = range.StartCol; i <= range.EndCol; i++)
            {
                for (int j = range.StartRow; j <= range.EndRow; j++)
                {
                    if (grid[i + 1, j] != ' ' || grid[i - 1, j] != ' ')
                    {
                        bonusRangeFound = false;
                        break;
                    }
                }
            }
            if (bonusRangeFound) bonusRange.Add(range);
        }

    }
    public static bool CheckBonusRanges(int row,int col)
    {
        int[,] inputcell = new int[row, col];

        foreach (GridRange range in bonusRange)
        {
            for (int i = range.StartRow; i <= range.EndRow; i++)
            {
                for (int j = range.StartCol; j <= range.EndCol; j++)
                {
                    if (new int[i, j] == inputcell) return true;
                }
            }
        }
        return false;
    }
    static int[] DetermineNewPos(string nextWord, bool horizontal,List<GridRange> horizontalGrids,List<GridRange> verticalGrids)
    {
        try
        {
            if (horizontal) // If the last word was vertical, iterate only the row to find an index
            {
                for (int k = verticalGrids.Count - 1; k>= 0; k--)
                {
                    int startRow = verticalGrids[k].StartRow;
                    int startCol = verticalGrids[k].StartCol;
                    int endRow = verticalGrids[k].EndRow;
                    int endCol = verticalGrids[k].EndCol;
                    Debug.Log("For current word: " + nextWord + "to be placed horizontally determine new pos between : " + startRow + "," + startCol + " && Ending at : " + endRow + "," + endCol);

                    for (int i = endRow; i >= startRow; i--)
                    {
                        foreach (char ch in nextWord)
                        {
                            List<int> matchingIndices = new List<int>();

                            for (int j = 0; j < nextWord.Length; j++)
                            {
                                if (nextWord[j] == ch && grid[i, startCol] == ch)
                                {
                                    //Debug.Log("j: " + j + " nextword[j]: " + nextWord[j] + " matches char ch " + ch+". at"+i+","+startCol);
                                    if (CanPlaceWord(nextWord, i, startCol-j, horizontal))
                                    {
                                        //Debug.Log("Adding index(j) " + j + " to matching indices. As word " + nextWord + " can be placed at" + startRow+","+(startCol-j));
                                        matchingIndices.Add(j);
                                    }
                                }

                            }

                            if (matchingIndices.Count >= 1)
                            {
                                // Randomly select an index from matchingIndices
                                int randomIndex = matchingIndices[new System.Random().Next(matchingIndices.Count-1)];
                                return new int[] { i, startCol - randomIndex };
                            }
                        }

                    }
                }

            }
            else // If the last word was horizontal, iterate only the column to find an index
            {
                for (int k = horizontalGrids.Count - 1; k >= 0; k--)
                {
                    int startRow = horizontalGrids[k].StartRow;
                    int startCol = horizontalGrids[k].StartCol;
                    int endRow = horizontalGrids[k].EndRow;
                    int endCol = horizontalGrids[k].EndCol;

                    Debug.Log("For current word: " + nextWord + "to be placed vertically determine new pos between : " + startRow + "," + startCol + " && Ending at : " + endRow + "," + endCol);

                    for (int j = endCol; j >= startCol; j--)
                    {
                        foreach (char ch in nextWord)
                        {
                            List<int> matchingIndices = new List<int>();

                            for (int i = 0; i < nextWord.Length; i++)
                            {
                                
                                if (nextWord[i] == ch && grid[startRow, j] == ch)
                                {
                                    //Debug.Log("i: " + i + " nextword[i]: " + nextWord[i] + " matches char ch " + ch + ". at" + startRow + "," + j);
                                    if (CanPlaceWord(nextWord, startRow-i, j, horizontal))
                                    {
                                        //Debug.Log("Adding index(i) " + i + " to matching indices. As word " + nextWord + " can be placed at" + (startRow-i) + "," + j);
                                        matchingIndices.Add(i);
                                    }
                                    
                                }
                            }

                            if (matchingIndices.Count >= 1)
                            {
                                // Randomly select an index from matchingIndices
                                int randomIndex = matchingIndices[new System.Random().Next(0, matchingIndices.Count)];
                                return new int[] { startRow - randomIndex, j };
                            }
                        }
                    }
                }
            }

            // If no position is found, return a default value or handle it accordingly
            return new int[] { -1, -1 };
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }

    }

    static bool CanPlaceWord(string nextWord, int startRow, int startCol, bool horizontal)
    {
        if (startRow != -1)
        {
            if (horizontal)
            {
                if (startCol + nextWord.Length < gridSize)
                {
                    //to check if the cell has adjacent cells filled, if so return false; as adjacent cells will falsely display unintended letters
                    for(int i = startCol; i < startCol + nextWord.Length; i++)
                    {
                        if(grid[startRow,i] == ' ' && startRow > 0 && startRow < gridSize-1)
                        {
                            if(grid[startRow+1,i]!=' '|| grid[startRow - 1, i] != ' ')
                            {
                                return false;
                            }
                        }
                        else if(grid[startRow, i] == ' ' && startRow == 0)
                        {
                            if (grid[startRow + 1, i] != ' ')
                            {
                                return false;
                            }
                        }
                        else if(grid[startRow, i] == ' ' && startRow == gridSize - 1)
                        {
                            if (grid[startRow - 1, i] != ' ')
                            {
                                return false;
                            }
                        }
                    }
                    // Check for leading/trailing cells with letters
                    if ((startCol > 0 && grid[startRow, startCol - 1] != ' ') || (startCol + nextWord.Length < gridSize && grid[startRow, startCol + nextWord.Length] != ' '))
                    {
                        return false;
                    }
                    for (int i = 0; i < nextWord.Length; i++)
                    {
                        if (grid[startRow, startCol + i] != ' ' && grid[startRow, startCol + i] != nextWord[i])
                        {
                            return false;
                        }
                    }

                    return true;

                }
            }
            else // Vertical placement
            {
                if (startRow + nextWord.Length < gridSize)
                {
                    //to check if the cell has adjacent cells filled, if so return false; as adjacent cells will falsely display unintended letters
                    for (int i = startRow; i < startRow + nextWord.Length; i++)
                    {
                        if (grid[i, startCol] == ' ' && startCol > 0 && startCol < gridSize - 1)
                        {
                            if (grid[i, startCol + 1] != ' ' || grid[i, startCol - 1] != ' ')
                            {
                                return false;
                            }
                        }
                        else if (grid[i, startCol] == ' ' && startCol == 0)
                        {
                            if (grid[i, startCol+1] != ' ')
                            {
                                return false;
                            }
                        }
                        else if (grid[i,startCol] == ' ' && startCol == gridSize - 1)
                        {
                            if (grid[i, startCol - 1] != ' ')
                            {
                                return false;
                            }
                        }
                    }
                    // Check for leading/trailing cells with letters
                    if ((startRow > 0 && grid[startRow - 1, startCol] != ' ') || (startRow + nextWord.Length < gridSize && grid[startRow + nextWord.Length, startCol] != ' '))
                    {
                        return false;
                    }
                    for (int i = 0; i < nextWord.Length; i++)
                    {
                        if (grid[startRow + i, startCol] != ' ' && grid[startRow + i, startCol] != nextWord[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
        return false;
    }

    static int[] PlaceWord(string word, int startRow, int startCol, bool horizontal)
    {
        int finalRow = startRow;
        int finalCol = startCol;
        int[] finalpos = { finalRow, finalCol };
        if (horizontal)
        {
            for (int i = 0; i < word.Length; i++)
            {
                grid[startRow, startCol + i] = word[i];
            }
            finalCol = startCol + word.Length - 1;
            finalpos[1] = finalCol;
        }
        else // Vertical placement
        {
            for (int i = 0; i < word.Length; i++)
            {
                grid[startRow + i, startCol] = word[i];
            }
            finalRow = startRow + word.Length - 1;
            finalpos[0] = finalRow;
        }
        placedWords.Add(word);
        placedWordsGridPos.Add(new int[] { startRow, startCol, finalRow, finalCol });

        return finalpos;
    }
    
}
