using UnityEngine;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
public class InstallDictionary : MonoBehaviour
{
    private const string FirstRunKey = "FirstRun";
    private const string dictSQLPath = "englishdictionary.sql"; // Replace with the actual path to your .sql file
    private const string dictDBPath = "englishdictionary.db";
    private const string levelDBPath = "levels.db";
    private const string levelsTabletxt = "LevelInfoTable.txt"; // Replace with the actual path to your Excel file
    private const string outputFile = "ValidateTable.txt";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (IsFirstRun())
        {
            //Execute one-time setup here
            Debug.Log("First run - executing one-time setup");

            //Set the flag to indicate that the setup has been executed
            PlayerPrefs.SetInt(FirstRunKey, 1);

            //Read the SQL script from the file
            string script = File.ReadAllText(dictSQLPath);

            //Create and initialize the SQLite database
            IDbConnection dbConnection = new SqliteConnection($"URI=file:{dictDBPath}");
            dbConnection.Open();

            //Execute the SQL script
            ExecuteScript(dbConnection, script);

            // Close the database connection
            dbConnection.Close();

            // Create Word Game SQLite database and populate it with data from Excel
            //=========================================================================
            //=========================================================================
            CreateWordGameDB(levelDBPath, levelsTabletxt);
            ////GetTableSchema(levelDBPath, "WordGameTable");
            InsertDataIntoWordGameDB(levelsTabletxt, levelDBPath);

            // Populate sourceWordList
            HashSet<string> sourceWordList = new HashSet<string>(GetWords(), StringComparer.OrdinalIgnoreCase);

            //Placeholder method to populate Word Game SQLite database based on criteria
            PopulateWordGameDB(sourceWordList);

            ExportTableToTextFile("WordGameTable", levelDBPath, outputFile);

            string testpath = "output.txt";
            // Use StreamWriter to write HashSet elements to a text file
                using (StreamWriter writer = new StreamWriter(testpath))
                {
                    foreach (string item in sourceWordList)
                    {
                        writer.WriteLine(item);
                    }
                }
        }
        else
        {
            // Application has been run before
            Debug.Log("Application has been run before");
        }
    }


    //static void GetTableSchema(string sqliteFilePath, string tableName)
    //{
    //    using (IDbConnection connection = new SqliteConnection($"Data Source={sqliteFilePath};Version=3;"))
    //    {
    //        connection.Open();

    //        // Get schema information for the specified table
    //        DataTable schemaTable = ((System.Data.Common.DbConnection)connection).GetSchema("Columns", new[] { null, null, tableName });

    //        // Display column names and data types
    //        Debug.Log("Column Names and Data Types for Table "+tableName);
    //        Debug.Log("------------------------------------------");

    //        foreach (DataRow row in schemaTable.Rows)
    //        {
    //            string columnName = row["COLUMN_NAME"].ToString();
    //            string dataType = row["DATA_TYPE"].ToString();

    //            Debug.Log("Column Name: "+columnName+" , Data Type: "+dataType);
    //        }

    //        connection.Close();
    //    }
    //}

    //static string GetDataTypeString(string dataType)
    //{
    //    // Convert SQLite data type constants to human-readable strings
    //    switch (dataType)
    //    {
    //        case "3": return "INTEGER";
    //        case "4": return "REAL";
    //        case "5": return "TEXT";
    //        case "6": return "BLOB";
    //        default: return "UNKNOWN";
    //    }
    //}

    private static bool IsFirstRun()
    {
        return PlayerPrefs.GetInt(FirstRunKey, 0) == 0;
    }

    private static void ExecuteScript(IDbConnection connection, string script)
    {
        // Split the script into individual commands based on semicolons
        string[] commands = Regex.Split(script, @"(?<!\\);");

        // Adjust the chunk size based on your needs
        int chunkSize = 100;

        for (int i = 0; i < commands.Length; i += chunkSize)
        {
            string[] chunk = commands.Skip(i).Take(chunkSize).ToArray();
            string chunkScript = string.Join(";", chunk);

            // Create a command
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = chunkScript;

                // Execute the command
                command.ExecuteNonQuery();
            }
        }
    }

    private static void PopulateWordGameDB(HashSet<string> sourceWordList)
    {
        System.Random random = new System.Random();
        HashSet<string> shuffledWords = sourceWordList.OrderBy(word => random.Next()).ToHashSet<string>();

        HashSet<string> usedWordList = new HashSet<string>();
        //read WordGameTable
        //get main word length for current entry,
        //go through each word in sourcewordlist if word.length = main word length.
        //upon finding a word , get all permutations of the letters in the word.
        //if permutation exist in source word list, concatenate associatedwordlist with "permutation"+",", increment associatedwordcount by 1(new variables created for this).
        //if(wordcount!=0) check if associatedwordcount >= wordcount, if true then write mainword to mainword column and associatedwordlist to list of associated word column in WordGameTable current entry. move to next entry in the table and go back to step 2.
        //if(wordcount!=0) check if associatedwordcount >= wordcount, if false then go to next word in line 3 and reset associatedwordlist,associatedwordcount and anything else created for current words operation.
        //if wordcount==0, check if associatedwordcount >= 2,if true then write mainword to mainword column and associatedwordlist to list of associated word column, if false then go to next word in line 3 and reset associatedwordlist,associatedwordcount and anything else created for current words operation.

        // Add your logic to populate Word Game SQLite database based on criteria
        // You can use the sourceWordList HashSet to filter words based on criteria

        IDbConnection dbConnection = new SqliteConnection($"URI=file:{levelDBPath}");
        dbConnection.Open();

        IDbCommand selectCommand = dbConnection.CreateCommand();
        selectCommand.CommandText = "SELECT * FROM WordGameTable";

        using (IDataReader reader = selectCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                int mainWordLength = 0;
                int mainWordLengthOrdinal = reader.GetOrdinal("main_word_length");

                if (mainWordLengthOrdinal != -1)
                {
                    if (!reader.IsDBNull(mainWordLengthOrdinal))
                    {
                        mainWordLength = reader.GetInt32(mainWordLengthOrdinal);
                    }
                    else
                    {
                        // Handle the case where main_word_length is NULL
                        // You can assign a default value or take an alternative action.
                        mainWordLength = 0; // For example, assuming 0 as a default value
                    }
                }
                else
                {
                    // Column does not exist, handle accordingly
                    // You might want to log a message or take alternative action
                    Debug.Log("Column 'main_word_length' not found in the result set.");
                }
                int wordCount = reader.GetInt32(reader.GetOrdinal("word_count"));
                string mainWord = null;
                if (wordCount != 0)
                {

                    foreach (string word in shuffledWords.Where(w => w.Length == mainWordLength))
                    {
                        string associatedWordList = "";
                        int associatedWordCount = 0;
                        if (!usedWordList.Contains(word))
                        {
                            foreach (string permutation in GenerateAllPermutationsAndSubsets(word))
                            {
                                if (shuffledWords.Contains(permutation))
                                {
                                    // Concatenate associatedWordList with "permutation" + ","
                                    mainWord = word;
                                    associatedWordList += permutation + ",";
                                    associatedWordCount++;


                                    //THIS CODE REQUIRES FIXING LATER ON
                                    //Get all possible permutations, and not just within wordcount limit
                                    //otherwords can be added to extra Words


                                }
                            }
                        }
                        if (associatedWordCount >= wordCount)
                        {
                            usedWordList.Add(mainWord);
                            // Update the current entry in WordGameTable
                            IDbCommand updateCommand = dbConnection.CreateCommand();
                            updateCommand.CommandText = "UPDATE WordGameTable SET main_word = @mainWord, list_of_associated_words = @associatedWordList WHERE level = @level";

                            updateCommand.Parameters.Add(new SqliteParameter("@mainWord", mainWord));
                            updateCommand.Parameters.Add(new SqliteParameter("@associatedWordList", associatedWordList.TrimEnd(',')));
                            updateCommand.Parameters.Add(new SqliteParameter("@level", reader.GetInt32(reader.GetOrdinal("level"))));
                            updateCommand.ExecuteNonQuery();
                            break;

                        }

                    }


                }
                else
                {
                    foreach (string word in shuffledWords.Where(w => w.Length == mainWordLength))
                    {
                        string associatedWordList = "";
                        int associatedWordCount = 0;
                        if (!usedWordList.Contains(word))
                        {
                            foreach (string permutation in GenerateAllPermutationsAndSubsets(word))
                            {

                                if (shuffledWords.Contains(permutation))
                                {
                                    mainWord = word;
                                    // Concatenate associatedWordList with "permutation" + ","
                                    associatedWordList += permutation + ",";
                                    associatedWordCount++;



                                }
                            }
                        }
                        if (associatedWordCount >= 2)
                        {

                            usedWordList.Add(mainWord);
                            // Update the current entry in WordGameTable
                            IDbCommand updateCommand = dbConnection.CreateCommand();
                            updateCommand.CommandText = "UPDATE WordGameTable SET main_word = @mainWord, list_of_associated_words = @associatedWordList WHERE level = @level";

                            updateCommand.Parameters.Add(new SqliteParameter("@mainWord", mainWord));
                            updateCommand.Parameters.Add(new SqliteParameter("@associatedWordList", associatedWordList.TrimEnd(',')));
                            updateCommand.Parameters.Add(new SqliteParameter("@level", reader.GetInt32(reader.GetOrdinal("level"))));

                            updateCommand.ExecuteNonQuery();
                            break;
                        }
                    }

                    
                }
            }
        }

        dbConnection.Close();
    }

    static HashSet<string> GenerateAllPermutationsAndSubsets(string word)
    {
        List<char> letters = word.ToCharArray().ToList();
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

    private static void CreateWordGameDB(string sqliteFilePath, string levelsTabletxt)
    {
        using (IDbConnection connection = new SqliteConnection($"Data Source={sqliteFilePath};Version=3;"))
        {
            connection.Open();

            // Read the first line of the text file to get headers and data types
            string[] headersWithTypes = GetHeadersAndTypesFromFile(levelsTabletxt);

            foreach(string header in headersWithTypes)
            {
                Debug.Log("CreateWordGameDB: Header and DataType" + header);
            }

            // Execute CreateTable method with headers and types
            CreateTable(connection, headersWithTypes);

            connection.Close();
        }
    }

    private static string[] GetHeadersAndTypesFromFile(string filePath)
    {
        string firstLine;

        // Read the first line from the file
        using (StreamReader reader = new StreamReader(filePath))
        {
            firstLine = reader.ReadLine();
        }

        // Split the first line into headers
        string[] headers = firstLine?.Split('|') ?? new string[0];

        // Infer data types based on the values in the subsequent columns
        string[] headersWithTypes = new string[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i].Trim();
            string dataType = InferDataTypeFromFile(filePath, i); // Start from the second column

            headersWithTypes[i] = header+" "+dataType;
        }

        return headersWithTypes;
    }

    private static string InferDataTypeFromFile(string filePath, int columnIndex)
    {
        // Read a few lines from the file to infer data types
        using (StreamReader reader = new StreamReader(filePath))
        {
            bool foundNonTextType = false;
            for (int i = 0; i < 5; i++) // Read 5 lines to infer data types
            {
                string line = reader.ReadLine();
                string[] values = line?.Split('|') ?? new string[0];

                if (columnIndex < values.Length)
                {
                    if (int.TryParse(values[columnIndex], out _))
                    {
                        return "INT";
                    }
                    else if (double.TryParse(values[columnIndex], out _))
                    {
                        return "REAL";
                    }
                    else
                    {
                        foundNonTextType = true;
                    }
                }
            }

            // If non-numeric types were found, default to TEXT
            if (foundNonTextType)
            {
                return "TEXT";
            }

            // Default to INTEGER if the type cannot be inferred
            return "INTEGER";
        }
    }

    private static void CreateTable(IDbConnection connection, string[] headersWithTypes)
    {
        IDbCommand command = connection.CreateCommand();

        // Build the CREATE TABLE SQL command dynamically based on headers and inferred data types
        string createTableSql = $"DROP TABLE IF EXISTS WordGameTable;CREATE TABLE WordGameTable ({string.Join(", ", headersWithTypes)});";

        command.CommandText = createTableSql;
        command.ExecuteNonQuery();
    }

    private static void InsertDataIntoWordGameDB(string levelsTabletxt, string sqliteFilePath)
    {
        string[] lines;

        try
        {
            // Read all lines from the text file
            lines = File.ReadAllLines(levelsTabletxt);
        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., file not found, permission issues, etc.
            Debug.Log("Error reading file: "+ex.Message);
            return;
        }

        // Check if there are at least two lines (header and at least one data line)
        if (lines.Length < 2)
        {
            Console.WriteLine("Not enough lines in the file to process.");
            return;
        }

        // Retrieve column names from the header (first line)
        var columnNames = lines[0]?.Split('|') ?? new string[0];

        // Open SQLite connection
        using (IDbConnection sqliteConnection = new SqliteConnection($"Data Source={sqliteFilePath};Version=3;"))
        {
            sqliteConnection.Open();

            // Start from the second line to skip the header
            for (int row = 1; row < lines.Length; row++)
            {
                // Retrieve values from each line
                var values = lines[row]?.Split('|') ?? new string[0];

                // Build the INSERT INTO SQL command dynamically based on columns
                string insertCommand = $"INSERT INTO WordGameTable ({string.Join(", ", columnNames)}) VALUES (" +
                                       $"{string.Join(", ", values.Select(value => $"'{value}'"))});";

                // Insert the data into your Word Game SQLite table
                using (IDbCommand sqliteCommand = sqliteConnection.CreateCommand())
                {
                    sqliteCommand.CommandText = insertCommand;
                    sqliteCommand.ExecuteNonQuery();
                }
            }

            sqliteConnection.Close();
        }
    }


    private static void ExecuteQuery(IDbConnection connection, string query)
    {
        IDbCommand command = connection.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();
    }

    public static HashSet<string> GetWords()
    {
        IDbConnection dbConnection = new SqliteConnection($"URI=file:{dictDBPath}");
        dbConnection.Open();
        HashSet<string> uniqueWords = GetUniqueWords(dbConnection);
        dbConnection.Close();
        return uniqueWords;
    }

    private static HashSet<string> GetUniqueWords(IDbConnection connection)
    {
        HashSet<string> uniqueWords = new HashSet<string>();

        // Query unique 'word' values from the 'entries' table
        //string query = "SELECT DISTINCT word FROM entries WHERE word GLOB '[A-Za-z]*';";
        string query = "SELECT DISTINCT word " +
               "FROM entries " +
               "WHERE word GLOB '[A-Za-z]*' AND word NOT GLOB '*[0-9]*' AND word NOT GLOB '*[^A-Za-z]*' AND " +
               "meaning NOT LIKE ' abbr%' AND " +
               "meaning NOT LIKE ' prefix%' AND " +
               "meaning NOT LIKE ' suffix%' AND " +
               "meaning NOT LIKE '%[imitative]%' AND " +
               "meaning NOT LIKE '%[abbreviation%' AND " +
               "meaning NOT LIKE '%[hebrew%' AND " +
               "meaning NOT LIKE '%[hindi%' AND " +
               "meaning NOT LIKE '%coarse slang%' AND " +
               "meaning NOT LIKE '%ffens.%' AND " +
               "meaning NOT LIKE ' symb%' AND " +
               "LENGTH(word) >= 3 AND LENGTH(word) <= 7;";
        IDbCommand command = connection.CreateCommand();
        command.CommandText = query;

        using (IDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string word = reader.GetString(0);
                uniqueWords.Add(word);
            }
        }

        return uniqueWords;
    }

    private static void ExportTableToTextFile(string tableName, string sqliteFilePath, string outputFile)
    {
        using (IDbConnection connection = new SqliteConnection($"Data Source={sqliteFilePath};Version=3;"))
        {
            connection.Open();

            using (IDbCommand selectCommand = connection.CreateCommand())
            {
                // Construct a SELECT query for the specified table
                selectCommand.CommandText = $"SELECT * FROM {tableName}";

                // Create a data reader to fetch the results
                using (IDataReader reader = selectCommand.ExecuteReader())
                {
                    // Open a StreamWriter to write to the output file
                    using (StreamWriter writer = new StreamWriter(outputFile))
                    {
                        // Write header (column names) to the file
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            writer.Write(reader.GetName(i));
                            if (i < reader.FieldCount - 1)
                            {
                                writer.Write("|");
                            }
                        }
                        writer.WriteLine(); // Move to the next line after writing the header

                        // Write data rows to the file
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                writer.Write(reader[i]);
                                if (i < reader.FieldCount - 1)
                                {
                                    writer.Write("|");
                                }
                            }
                            writer.WriteLine(); // Move to the next line after writing a data row
                        }
                    }
                }
            }

            connection.Close();
        }
    }
    public static LevelObject QueryTableByLevel(int level)
    {
        using (var connection = new SqliteConnection($"URI=file:{levelDBPath}"))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                // Customize the query based on your table structure
                command.CommandText = $"SELECT level, levelname, sublevel, main_word_length, word_count, main_word, list_of_associated_words FROM WordGameTable WHERE level = {level}";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Create an object to hold the result
                        var result = new LevelObject
                        {
                            Level = reader.GetInt32(reader.GetOrdinal("level")),
                            LevelName = reader.GetString(reader.GetOrdinal("levelname")),
                            Sublevel = reader.GetInt32(reader.GetOrdinal("sublevel")),
                            MainWordLength = reader.GetInt32(reader.GetOrdinal("main_word_length")),
                            WordCount = reader.GetInt32(reader.GetOrdinal("word_count")),
                            MainWord = reader.GetString(reader.GetOrdinal("main_word")),
                            AssociatedWords = reader.GetString(reader.GetOrdinal("list_of_associated_words"))
                        };

                        return result;
                    }
                }
            }
        }

        // Return null or throw an exception if no result is found
        return null;
    }

    public class LevelObject
    {
        public int Level { get; set; }
        public string LevelName { get; set; }
        public int Sublevel { get; set; }
        public int MainWordLength { get; set; }
        public int WordCount { get; set; }
        public string MainWord { get; set; }
        public string AssociatedWords { get; set; }
    }
}
