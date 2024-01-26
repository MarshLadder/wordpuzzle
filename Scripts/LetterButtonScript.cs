using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyCoded.HapticFeedback;
using TMPro;
public class LetterButtonScript : MonoBehaviour
{



    public string LetterButtonPress()
    {

        Debug.Log("button Pressed");
        HapticFeedback.LightFeedback();
        return this.GetComponentInChildren<TMP_Text>().text;
    }
}
