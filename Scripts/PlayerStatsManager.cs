using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStatsManager : MonoBehaviourSingletonPersistent<PlayerStatsManager>
{
    [SerializeField]
    TMP_Text scoreText;
    public override void Awake()
    {
        base.Awake();
        scoreText.text = ""+inGameCurrency;

    }
    private void OnEnable()
    {

        GameManager.Instance.OnSwipeComplete += UpdateStats;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwipeComplete -= UpdateStats;
    }
    private void UpdateStats(SwipeDetection.ValidationOutcomes outcome)
    {

    }

    private int inGameCurrency = 0;
    private int experiencePoints = 0;

    // Define keys for PlayerPrefs
    private string currencyKey = "InGameCurrency";
    private string xpKey = "ExperiencePoints";

    // Function to save player stats
    public void SavePlayerStats()
    {
        PlayerPrefs.SetInt(currencyKey, inGameCurrency);
        PlayerPrefs.SetInt(xpKey, experiencePoints);
        

        PlayerPrefs.Save(); // Save changes to disk
    }

    // Function to load player stats
    public void LoadPlayerStats()
    {
        if (PlayerPrefs.HasKey(currencyKey))
            inGameCurrency = PlayerPrefs.GetInt(currencyKey);

        if (PlayerPrefs.HasKey(xpKey))
            experiencePoints = PlayerPrefs.GetInt(xpKey);
    }

    // Example function to increase in-game currency
    public void IncreaseInGameCurrency(PlayerStats stats)
    {
        inGameCurrency += stats.Currency;
    }

    // Example function to increase experience points
    public void IncreaseExperiencePoints(PlayerStats stats)
    {
        experiencePoints += stats.XP;
    }


    // Example function to reset player stats
    public void ResetPlayerStats()
    {
        inGameCurrency = 0;
        experiencePoints = 0;

        SavePlayerStats(); // Save the reset stats
    }

    // Example usage
    private void Start()
    {
        // Load player stats when the game starts
        LoadPlayerStats();
    }

    // Save player stats when the game is closed or paused
    private void OnApplicationQuit()
    {
        SavePlayerStats();
    }
}
