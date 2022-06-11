using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagesManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] public Canvas myCanvas;
    [SerializeField] public GameObject messagePrefab;
    [SerializeField] public GameObject messagesPanel;
    [SerializeField] public Text messageTo;

    #endregion

    #region Private Fields

    private List<GameObject> messagesBoxes;

    #endregion

    #region Public Fields

    public string targetName;

    #endregion

    #region Singleton

    private static MessagesManager instance;
    public static MessagesManager Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        if (PlayerManager.LocalPlayerManager == null || PlayerManager.LocalPlayerManager.cluesManager == null)
            return;

        var clues = PlayerManager.LocalPlayerManager.cluesManager.GetAllClues();
        foreach(Clue clue in clues)
        {
            GameObject obj = Instantiate(messagePrefab, messagesPanel.transform);
            obj.GetComponent<MessageBox>().UpdateMessageBox(clue);
            messagesBoxes.Add(obj);
        }
    }

    private void OnDisable()
    {
        foreach(var obj in messagesBoxes)
        {
            Destroy(obj);
        }
    }

    void Awake()
    {
        instance = this;
        messagesBoxes = new List<GameObject>();
    }

    #endregion

    #region Public Methods

    public void CloseWindow()
    {
        myCanvas.enabled = false;
        this.gameObject.SetActive(false);
    }

    public void UpdateName(string name)
    {
        targetName = name;
        messageTo.text = "Message to:\n" + name;
    }

    #endregion
    
    #region 
    #endregion


}
