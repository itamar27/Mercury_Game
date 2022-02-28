using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class AgentManager : MonoBehaviourPun
{
    #region Serialze Fields

    [SerializeField] private GameObject messagesBox;
    [SerializeField] private Animator animator;
    [SerializeField] private TextMesh txtName;

    #endregion

    #region Public Fields

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public static PlayerManager LocalPlayerManager;
    public static CluesManager cluesManager;

    public string playerName;

    #endregion

    #region Private Fields

    private float localXScale;
    private float speed = 1.5f;
    private float x = 0f;

    // Agent Behaiour
    private int currTargetIndex;
    private List<PlayerManager> targets;
    private float messageCoolDown;
    private float lastMessageTime;
    private bool getNewTarget;

    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        Com.Mercury.Game.GameManager.playerJoinedRoomDelegate += UpdateTargets;
        Com.Mercury.Game.GameManager.playerLeftRoomDelegate += UpdateTargets;
    }

    private void OnDisable()
    {
        Com.Mercury.Game.GameManager.playerJoinedRoomDelegate -= UpdateTargets;
        Com.Mercury.Game.GameManager.playerLeftRoomDelegate -= UpdateTargets;
    }

    private void Awake()
    {
        targets = new List<PlayerManager>();
        cluesManager = new CluesManager();

        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        int randomWaitSecAtStart = Random.Range(0, 5);
        lastMessageTime = 5f + randomWaitSecAtStart;
        getNewTarget = true;
    }

    private void Start()
    {
        messageCoolDown = 5f;

        GetName();

        localXScale = transform.localScale.x;

        messagesBox.SetActive(false);

        StartCoroutine(InitTargets());
        currTargetIndex = 0;
    }

    private void FixedUpdate()
    {
        nameDirection();
        messageBoxDirection();
        try
        {
            if (currTargetIndex >= 0 && currTargetIndex < targets.Count)
            {
                if (transform.position.x > targets[currTargetIndex].transform.position.x)
                    x = -1;
                else
                    x = 1;
            }
        }
        catch
        {
            UpdateTargets();
            getNewTarget = true;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            Move();
        }
    }

    #endregion

    #region Pun Methods

    private void SendMessage(Clue clue)
    {
        string from = PlayerManager.LocalPlayerManager.playerName;
        this.photonView.RPC("ChatMessage", RpcTarget.All, from, clue.GetHistory(), this.playerName, clue.GetMessage());
        Chat.Instance.SendMessageToChat(string.Format("<color=blue>To " + this.playerName + ":</color> " + clue.GetMessage()));
    }

    private void SendAgentMessage(Clue clue)
    {
        string from = this.playerName;
        this.photonView.RPC("ChatMessage", RpcTarget.All, from, clue.GetHistory(), targets[currTargetIndex].playerName, clue.GetMessage());
    }

    #endregion

    #region Pun Callbacks

    [PunRPC]
    void ChatMessage(string from, string histrory, string to, string message)
    {
        if (to == this.playerName)
        {
            cluesManager.AddMessage(message, histrory, to);
        }
        if(to == PlayerManager.LocalPlayerManager.playerName)
        {
            Chat.Instance.SendMessageToChat(string.Format("<color=red>" + from + ":</color> " + message));
        }
    }

    [PunRPC]
    void NewTarget(int newTargetIndex, string name)
    {
        if(name == playerName)
        {
            currTargetIndex = newTargetIndex;
            getNewTarget = false;
        }
    }

    #endregion

    #region Public Methods

    public void onMessageClicked(int clueIndex) // Need to be caught from event
    {
        SendMessage(PlayerManager.cluesManager.GetClueAt(clueIndex));
        messagesBox.SetActive(false);
    }

    #endregion

    #region Private Methods

    private void UpdateTargets()
    {
        targets.RemoveRange(0, targets.Count);
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            targets.Add(obj.GetComponent<PlayerManager>());
        }

        targets.Sort();
    }

    private IEnumerator InitTargets()
    {
        yield return new WaitForSeconds(2);
        UpdateTargets();
    }

    private void Move()
    {
        if (getNewTarget)
        {
            getNewTarget = false;
            currTargetIndex = Random.Range(0, targets.Count);
            this.photonView.RPC("NewTarget", RpcTarget.Others, currTargetIndex, playerName);
        }
        else if((lastMessageTime + messageCoolDown < Time.time) && (currTargetIndex >= 0) && (currTargetIndex < targets.Count) && (Vector3.Distance(transform.position, targets[currTargetIndex].transform.position) > 1.5f))
        {
            Vector2 target = new Vector2(targets[currTargetIndex].transform.position.x, targets[currTargetIndex].transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            animator.SetFloat("Speed", 1);
        }
        else if((lastMessageTime + messageCoolDown < Time.time) && (currTargetIndex >= 0) && (currTargetIndex < targets.Count))
        {
            animator.SetFloat("Speed", 0);
            List<Clue> clues = cluesManager.GetAllClues();
            int random = Random.Range(0, clues.Count);
            SendAgentMessage(clues[random]);
            lastMessageTime = Time.time;
            getNewTarget = true;
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        var textTransform = txtName.transform.localScale;
        if (x < 0)
        {
            transform.localScale = new Vector3(-localXScale, transform.localScale.y, transform.localScale.z);
        }
        else if (x > 0)
        {
            transform.localScale = new Vector3(localXScale, transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void nameDirection()
    {
        if (transform.localScale.x < 0)
            txtName.transform.localScale = new Vector3(-Mathf.Abs(txtName.transform.localScale.x), txtName.transform.localScale.y, txtName.transform.localScale.z);
        else
            txtName.transform.localScale = new Vector3(Mathf.Abs(txtName.transform.localScale.x), txtName.transform.localScale.y, txtName.transform.localScale.z);
    }

    private void messageBoxDirection()
    {
        if (transform.localScale.x < 0)
            messagesBox.transform.localScale = new Vector3(-Mathf.Abs(messagesBox.transform.localScale.x), messagesBox.transform.localScale.y, messagesBox.transform.localScale.z);
        else
            messagesBox.transform.localScale = new Vector3(Mathf.Abs(messagesBox.transform.localScale.x), messagesBox.transform.localScale.y, messagesBox.transform.localScale.z);
    }

    private void GetName()
    {
        int cutPhotonId = photonView.ViewID;
        if (cutPhotonId > 99)
        {
            while (cutPhotonId > 99)
                cutPhotonId /= 10;
        }

        int idToName = cutPhotonId;
        string prepareForConvert;
        if (idToName > 9)
        {
            prepareForConvert = "" + (idToName % 10) + (idToName / 10);
        }
        else
        {
            prepareForConvert = "" + idToName;
        }

        int toConvert = int.Parse(prepareForConvert);
        toConvert += 64;

        int counter = 0;
        while (toConvert > 90)
        {
            toConvert -= 26;
            counter++;
        }

        if (counter == 0)
        {
            playerName = "Bot " + System.Convert.ToChar(toConvert);
        }
        else
        {
            playerName = "Bot " + System.Convert.ToChar(64 + counter) + System.Convert.ToChar(toConvert - 26);
        }
        txtName.text = playerName;
    }

    private void OnMouseDown()
    {
        if (messagesBox.active)
            messagesBox.SetActive(false);
        else if (currTargetIndex >= 0 && currTargetIndex < targets.Count && Vector3.Distance(transform.position, targets[currTargetIndex].transform.position) < 2f)
        {
            messagesBox.GetComponent<MessageBoxManager>().UpdateClues(cluesManager.GetAllClues());
            messagesBox.SetActive(true);
        }
    }    

    #endregion
}
