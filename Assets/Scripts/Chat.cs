using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    [SerializeField] List<Message> messageList = new List<Message>();

    [SerializeField] public GameObject chatPanel;
    [SerializeField] public GameObject textObject;

    #region Singleton

    private static Chat instance;
    public static Chat Instance
    {
        get
        {
            if (instance == null)
                instance = new Chat();

            return instance;
        }
    }

    #endregion

    #region MonoBehaviour

    void Awake()
    {
        instance = this;
    }

    #endregion

    #region Public Methods

    public void SendMessageToChat(string text)
    {
        GameObject newText = Instantiate(textObject, chatPanel.transform);

        Message newMessage = new Message(text, newText.GetComponent<Text>()); 

        messageList.Add(newMessage);
    }

    #endregion
}

[System.Serializable]
public class Message {
    private string messageText;
    private Text textObject;

    public Message(string text, Text newText) {
        this.messageText = text;
        this.textObject = newText;

        this.textObject.text = this.messageText;
    }
}
