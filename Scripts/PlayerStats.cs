using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    int currency;
    int xp;



    int bonusmultiplier_cleanreveal, bonusmultiplier_bonusword = 1;
    private int bonusmultiplier_cleanreveal_XP;

    public PlayerStats(int initialCurrency,int initialXP)
    {
        currency = initialCurrency;
        xp = initialXP;
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }
    public PlayerStats()
    {
        currency = 0;
        xp = 0;
    }

    public int Currency
    {
        get { return currency; }
        set { currency = value; }
    }

    public int XP
    {
        get { return xp; }
        set { xp = value; }
    }
    public void UpdateStat(int addedcurrency,  int addedXP,bool bonus_CR,bool bonus_bonusWord)
    {
        UpdateCurrency(addedcurrency, bonus_CR, bonus_bonusWord);
        UpdateXP(addedXP, bonus_CR, bonus_bonusWord);
        ResetMultipliers();
    }
    public void UpdateCurrency(int addedcurrency, bool bonus_CR, bool bonus_bonusWord)
    {
        if(bonus_CR == true)
        {
            bonusmultiplier_cleanreveal = 2;
        }
        if(bonus_bonusWord == true)
        {
            bonusmultiplier_bonusword = 2;
        }
        currency += addedcurrency * bonusmultiplier_cleanreveal * bonusmultiplier_bonusword;

        
    }
    public void UpdateXP(int addedXP, bool bonus_CR, bool bonus_bonusWord)
    {
        if (bonus_CR == true)
        {
            bonusmultiplier_cleanreveal_XP = 3;
        }
        xp += addedXP * bonusmultiplier_cleanreveal_XP;
    }

    private void ResetMultipliers()
    {
        bonusmultiplier_cleanreveal = 1;
        bonusmultiplier_bonusword = 1;
    }
}
