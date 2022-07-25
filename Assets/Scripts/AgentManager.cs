using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public CluesManager cluesManager;

    public string playerName;
    public int appearanceId;

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

        GetName();
    }

    private void Start()
    {
        messageCoolDown = 5f;

        localXScale = transform.localScale.x;

        messagesBox.SetActive(false);

        StartCoroutine(InitTargets());
        currTargetIndex = 0;

        appearanceId = Globals.GameConfig.botAppearId1;
        animator.runtimeAnimatorController = PlayerAppearanceManager.GetAnimatorController(appearanceId);
    }

    private void FixedUpdate()
    {
        if (Com.Mercury.Game.GameManager.gameAct == Globals.GameAct.Day)
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

            if (PhotonNetwork.IsMasterClient)
            {
                Move();
            }
        }
    }

    #endregion

    #region Pun Methods

    public void UpdateCommonClue(int clueId)
    {
        this.photonView.RPC("UpdateCommon", RpcTarget.All, clueId, playerName);
    }

    public void UpdatePrivateClue(int clueId)
    {
        this.photonView.RPC("UpdatePrivate", RpcTarget.All, clueId, playerName);
    }


    private void SendMessage(Clue clue)
    {
        string from = PlayerManager.LocalPlayerManager.playerName;
        this.photonView.RPC("ChatMessage", RpcTarget.All, from, clue.GetHistory(), this.playerName, clue.GetMessage());
        Chat.Instance.SendMessageToChat(string.Format("<color=blue>To " + this.playerName + ":</color> " + clue.GetMessage()));

        var dict = new Dictionary<string, object>();
        dict.Add("research", Globals.GameConfig.researchId);
        dict.Add("source", from);
        dict.Add("target", this.playerName);
        dict.Add("score", clue.GetScore().ToString());
        dict.Add("message", clue.GetMessage());
        dict.Add("round", Globals.gameRound);
        HttpService.Instance.Post("game/interaction/", dict);
    }

    private void SendAgentMessage(Clue clue)
    {
        string from = this.playerName;
        string to = targets[currTargetIndex].playerName;
        this.photonView.RPC("ChatMessage", RpcTarget.All, from, clue.GetHistory(), to, clue.GetMessage());
        try
        {
            var dict = new Dictionary<string, object>();
            dict.Add("research", Globals.GameConfig.researchId);
            dict.Add("source", from);
            dict.Add("target", to);
            dict.Add("score", clue.GetScore().ToString());
            dict.Add("message", clue.GetMessage());
            dict.Add("round", Globals.gameRound);
            HttpService.Instance.Post("game/interaction/", dict);
        }
        catch
        {
            Debug.Log("Could not send bot message to platform");
            Debug.Log(HttpService.Instance.result);
        }
    }

    #endregion

    #region Pun Callbacks

    [PunRPC]
    void UpdateCommon(int clueId, string name)
    {
        if (playerName == name)
        {
            cluesManager.ClearAllClues();
            cluesManager.InitClues(playerName);
            CluesFactory cluesFactory = new CluesFactory();
            PlayerManager.LocalPlayerManager.cluesManager.AddMessage(cluesFactory.GetClueById(clueId), "", playerName, 1);
        }
    }

    [PunRPC]
    void UpdatePrivate(int clueId, string name)
    {
        if (playerName == name)
        {
            CluesFactory cluesFactory = new CluesFactory();
            cluesManager.AddMessage(cluesFactory.GetClueById(clueId), "", playerName, 2);
        }
    }



    [PunRPC]
    void ChatMessage(string from, string histrory, string to, string message)
    {
        if (to == this.playerName)
        {
            cluesManager.AddMessage(message, histrory, to, 2);
        }
        if (to == PlayerManager.LocalPlayerManager.playerName)
        {
            Chat.Instance.SendMessageToChat(string.Format("<color=red>From " + from + ":</color> " + message));
        }
    }

    [PunRPC]
    void NewTarget(int newTargetIndex, string name)
    {
        if (name == playerName)
        {
            currTargetIndex = newTargetIndex;
            getNewTarget = false;
        }
    }

    #endregion

    #region Public Methods

    public void onMessageClicked(int clueIndex)
    {
        SendMessage(PlayerManager.LocalPlayerManager.cluesManager.GetClueAt(clueIndex));
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
        else if ((lastMessageTime + messageCoolDown < Time.time) && (currTargetIndex >= 0) && (currTargetIndex < targets.Count) && (Vector3.Distance(transform.position, targets[currTargetIndex].transform.position) > 1.5f))
        {
            Vector2 target = new Vector2(targets[currTargetIndex].transform.position.x, targets[currTargetIndex].transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            animator.SetFloat("Speed", 1);
        }
        else if ((lastMessageTime + messageCoolDown < Time.time) && (currTargetIndex >= 0) && (currTargetIndex < targets.Count))
        {
            animator.SetFloat("Speed", 0);
            List<Clue> clues = cluesManager.GetAllClues();
            int random;

            if (Globals.GameConfig.agentsBehaviour == Globals.AgentBehaviour.SubOptimal || clues.Count < 4)
                random = Random.Range(0, clues.Count);
            else
                random = Random.Range(3, clues.Count);

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
        var bots = GameObject.FindGameObjectsWithTag("Agent");
        List<int> viewIds = new List<int>();
        foreach (var bot in bots)
        {
            viewIds.Add(bot.GetComponent<PhotonView>().ViewID);
        }
        viewIds.Sort();
        int viewIdIndex = viewIds.IndexOf(photonView.ViewID);
        if (viewIdIndex == 0)
            playerName = Globals.GameConfig.botNameId1;
        else if (viewIdIndex == 1)
            playerName = Globals.GameConfig.botNameId2;
        else if (viewIdIndex == 2)
            playerName = Globals.GameConfig.botNameId3;

        txtName.text = playerName;
    }


    [System.Obsolete]
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (currTargetIndex >= 0 && currTargetIndex < targets.Count && Vector3.Distance(transform.position, targets[currTargetIndex].transform.position) < 2f)
        {
            Com.Mercury.Game.GameManager.Instance.messageBoxCanvas.enabled = true;
            MessagesManager.Instance.gameObject.SetActive(false);
            MessagesManager.Instance.gameObject.SetActive(true);
            MessagesManager.Instance.UpdateName(playerName);
        }
    }

    #endregion
}
