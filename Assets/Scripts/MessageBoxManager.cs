using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxManager : MonoBehaviour
{
    [SerializeField] public List<GameObject> messagesObject;
    [SerializeField] public List<TextMesh> messagesText;
    private List<Clue> clues;

    private void OnEnable()
    {
        if (clues == null)
            return;

        int i = 0;
        for(; i<4; i++)
        {
            if(i<clues.Count)
            {
                messagesObject[i].SetActive(true);
                messagesText[i].text = clues[i].GetMessage();
            }
            else
            {
                messagesObject[i].SetActive(false);
            }
        }
    }

    public void UpdateClues(List<Clue> _clues)
    {
        this.clues = _clues;
    }
}
