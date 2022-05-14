using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoteBar : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] public Text barText;
    [SerializeField] public Text votesCountText;

    #endregion

    #region Private Fields

    private string playerName;
    private int voteCount;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        voteCount = 0;
    }

    private void Start()
    {
        votesCountText.text = "Votes: " + voteCount;
    }

    #endregion

    #region Public Methods

    public int GetVoteCount()
    {
        return voteCount;
    }
    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetName(string playerName)
    {
        this.playerName = playerName;
        barText.text = "Player " + playerName;
    }

    public void AddVote()
    {
        voteCount++;
        votesCountText.text = "Votes: " + voteCount;
    }

    public void OnClick()
    {
        voteCount++;
        votesCountText.text = "Votes: " + voteCount;

        this.GetComponentInParent<VotesPanelManager>().DisableAll();
        PlayerManager.LocalPlayerManager.onVoteClicked(playerName);
    }

    public void OnVenomClick()
    {
        voteCount++;
        votesCountText.text = "Votes: " + voteCount;
        this.GetComponentInParent<VotesPanelManager>().DisableAll();
        //Inform Other Venoms
        PlayerManager.LocalPlayerManager.onVenomVoteClicked(playerName);
    }

    #endregion
}
