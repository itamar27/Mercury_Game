using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    [SerializeField] public Text messageText;

    private Clue clue;

    public void UpdateMessageBox(Clue clue)
    {
        this.clue = clue;
        messageText.text = clue.GetMessage();
    }

    public void OnBoxClicked()
    {
        PlayerManager.LocalPlayerManager.SendMessageToPlayer(clue, MessagesManager.Instance.targetName);
        MessagesManager.Instance.CloseWindow();
    }
}
