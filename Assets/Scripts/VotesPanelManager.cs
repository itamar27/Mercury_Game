using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VotesPanelManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] public GameObject barPrefab;

    #endregion

    #region Private Fields

    private Dictionary<string, GameObject> bars;
    private Dictionary<string, VoteBar> barsScript;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        bars = new Dictionary<string, GameObject>();
        barsScript = new Dictionary<string, VoteBar>();
    }

    #endregion

    #region Public Methods

    public void ResetPanel()
    {
        foreach(GameObject obj in bars.Values)
        {
            Destroy(obj);
        }

        bars.Clear();
        barsScript.Clear();
    }

    public void AddBar(string playerName)
    {
        GameObject obj = Instantiate(barPrefab, this.transform);
        VoteBar voteBarSc = obj.GetComponent<VoteBar>();
        voteBarSc.SetName(playerName);

        bars.Add(playerName, obj);
        barsScript.Add(playerName, voteBarSc);

        if (playerName == PlayerManager.LocalPlayerManager.playerName)
            bars[playerName].GetComponent<Button>().interactable = false;
    }

    public void DisableAll()
    {
        foreach(var bar in bars.Values)
        {
            bar.GetComponent<Button>().interactable = false;
        }
    }

    public void AddVote(string playerName)
    {
        barsScript[playerName].AddVote();
    }

    public string MostVoted()
    {
        int maxVotes = -1;
        List<string> names = new List<string>();
        foreach (VoteBar player in barsScript.Values)
        {
            int voteCount = player.GetVoteCount();
            if (voteCount > maxVotes)
            {
                maxVotes = voteCount;
                names.Clear();
                names.Add(player.GetPlayerName());
            }
            else if (voteCount == maxVotes)
            {
                names.Add(player.GetPlayerName());
            }
        }

        names.Sort();
        return names[0];
    }

    #endregion
}
