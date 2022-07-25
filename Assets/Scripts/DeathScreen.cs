using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] Text deathTxt;
    [SerializeField] Text gameStatusTxt;
    [SerializeField] Text actAndRoundReference;

    void Update()
    {
        string actAndRound = actAndRoundReference.text;
        int remainingPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
        remainingPlayers += GameObject.FindGameObjectsWithTag("Agent").Length;

        gameStatusTxt.text = "Please wait for the end of the round\nRemaining Players: " + remainingPlayers + "\nGame status: " + actAndRound;
    }

    public void UpdateDeathText(bool killedByVenom)
    {
        if (killedByVenom)
            deathTxt.text = "The Venoms has murdered you during night";
        else
            deathTxt.text = "The colony has voted to eliminate you";
    }
}
