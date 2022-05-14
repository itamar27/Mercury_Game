using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager : MonoBehaviourPun, IComparable
{
    #region Serialze Fields
    [SerializeField] VotesPanelManager votePanelManager;
    [SerializeField] VotesPanelManager venomVotePanelManager;

    [SerializeField] private GameObject messagesBox;
    [SerializeField] private Animator animator;
    [SerializeField] private TextMesh txtName;

    #endregion

    #region Public Fields

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public static PlayerManager LocalPlayerManager;
   // public static Photon.Realtime.Player LocalPhotonPlayer;
    public CluesManager cluesManager;

    public string playerName;
    public string playerRole;
    public int appearanceId;

    #endregion

    #region Private Fields

    private CameraBehaviour mainCamera;

    private Rigidbody2D rigidbody2d;

    private float localXScale;
    private float speed = 2f;
    private float x = 0f;
    private float y = 0f;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
            PlayerManager.LocalPlayerManager = this.GetComponent<PlayerManager>();
            PlayerManager.LocalPlayerManager.cluesManager = new CluesManager();
            //PlayerManager.LocalPhotonPlayer = PhotonNetwork.LocalPlayer;
        }
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        rigidbody2d = GetComponent<Rigidbody2D>();

        GetName();
    }

    private void Start()
    {
        localXScale = transform.localScale.x;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehaviour>();

        messagesBox.SetActive(false);

        if (photonView.IsMine)
        {
            int appearId = Globals.LocalPlayerInfo.appearanceId;
            animator.runtimeAnimatorController = PlayerAppearanceManager.GetAnimatorController(appearId);
            this.photonView.RPC("OnAnimatorChanged", RpcTarget.All, playerName, appearId);

            this.photonView.RPC("UpdateRole", RpcTarget.All, Globals.LocalPlayerInfo.role, playerName);
            this.photonView.RPC("UpdateAppearId", RpcTarget.All, Globals.LocalPlayerInfo.appearanceId, playerName);
        }
    }

    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;

        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        nameDirection();
        messageBoxDirection();

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;

        if (Com.Mercury.Game.GameManager.gameAct == Globals.GameAct.Day)
        {
            Move(x * Time.deltaTime, y * Time.deltaTime);
        }
        else if (Com.Mercury.Game.GameManager.gameAct == Globals.GameAct.Vote)
        {
            // Get Vote
        }
        else
        {
            if (Globals.LocalPlayerInfo.role == "Venom")
            {
                // Venoms night
            }
            else
            {
                // Citizens night
            }
        }
    }

    #endregion

    #region Pun Methods

    private void SendMessage(Clue clue)
    {
        string from = PlayerManager.LocalPlayerManager.playerName;
        this.photonView.RPC("ChatMessage", RpcTarget.Others, from, clue.GetHistory(), this.playerName, clue.GetMessage());
        Chat.Instance.SendMessageToChat(string.Format("<color=blue>To " + this.playerName + ":</color> " + clue.GetMessage()));

        var dict = new Dictionary<string, object>();
        dict.Add("research", Globals.GameConfig.researchId);
        dict.Add("source", from);
        dict.Add("target", this.playerName);
        dict.Add("score", clue.GetScore().ToString());
        dict.Add("message", clue.GetMessage());
        dict.Add("round", Com.Mercury.Game.GameManager.Instance.round.ToString());
        HttpService.Instance.Post("game/interaction/", dict);
    }

    #endregion

    #region Pun Callbacks

    [PunRPC]
    void ChatMessage(string from, string histrory, string to, string message)
    {
        if (to == LocalPlayerManager.playerName)
        {
            Chat.Instance.SendMessageToChat(string.Format("<color=red>" + from + ":</color> " + message));

            PlayerManager.LocalPlayerManager.cluesManager.AddMessage(message, histrory, to);
        }
    }

    [PunRPC]
    void OnPlayerVoted(string votedTo)
    {
        Com.Mercury.Game.GameManager.Instance.AddToVotePanel(votedTo);
    }

    [PunRPC]
    void UpdateRole(string role, string name)
    {
        if (name == playerName)
        {
            playerRole = role;
        }
    }

    [PunRPC]
    void UpdateAppearId(int appearId, string name)
    {
        if (name == playerName)
        {
            appearanceId = appearId;
        }
    }

    [PunRPC]
    void UpdateCommon(int clueId, string name)
    {
        if (playerName == name)
        {
            PlayerManager.LocalPlayerManager.cluesManager.ClearAllClues();
            CluesFactory cluesFactory = new CluesFactory();
            PlayerManager.LocalPlayerManager.cluesManager.AddMessage(cluesFactory.GetClueById(clueId), "", "");
        }
    }

    [PunRPC]
    void UpdatePrivate(int clueId, string name)
    {
        if (playerName == name && photonView.IsMine)
        {
            CluesFactory cluesFactory = new CluesFactory();
            PlayerManager.LocalPlayerManager.cluesManager.AddMessage(cluesFactory.GetClueById(clueId), "", playerName);
        }
    }

    [PunRPC]
    void OnVenomVoted(string votedTo)
    {
        if (LocalPlayerManager.playerRole == "Venom")
            Com.Mercury.Game.GameManager.Instance.AddToVenomVotePanel(votedTo);
    }

    [PunRPC]
    void OnAnimatorChanged(string name, int id)
    {
        if (name == playerName)
        {
            animator.runtimeAnimatorController = PlayerAppearanceManager.GetAnimatorController(id);
        }
    }

    [PunRPC]
    void OnPlayerEliminated(string name, int id)
    {
        Com.Mercury.Game.GameManager.Instance.EliminatePlayer(name, id);
    }
    #endregion

    #region Public Methods

    public void UpdateCommonClue(int clueId)
    {
        this.photonView.RPC("UpdateCommon", RpcTarget.All, clueId, playerName);
    }

    public void UpdatePrivateClue(int clueId)
    {
        this.photonView.RPC("UpdatePrivate", RpcTarget.All, clueId, playerName);
    }

    public void onMessageClicked(int clueIndex)
    {
        SendMessage(PlayerManager.LocalPlayerManager.cluesManager.GetClueAt(clueIndex));
        messagesBox.SetActive(false);
    }

    public void onVoteClicked(string playerName)
    {
        this.photonView.RPC("OnPlayerVoted", RpcTarget.Others, playerName);
    }

    public void onVenomVoteClicked(string playerName)
    {
        this.photonView.RPC("OnVenomVoted", RpcTarget.Others, playerName);
    }

    public void onPlayerEliminated(string playerName, int apearId)
    {
        this.photonView.RPC("OnPlayerEliminated", RpcTarget.All, playerName, apearId);
    }

    public int CompareTo(object obj)
    {
        var a = this;
        var b = obj as PlayerManager;

        if (a.photonView.ViewID < b.photonView.ViewID)
            return -1;
        else
            return 1;
    }

    #endregion

    #region Private Methods

    private void Move(float x, float y)
    {
        transform.position += new Vector3(speed * x, speed * y, 0);

        animator.SetFloat("Speed", Mathf.Abs(x) + Mathf.Abs(y));

        var textTransform = txtName.transform.localScale;
        if (x < 0)
        {
            transform.localScale = new Vector3(-localXScale, transform.localScale.y, transform.localScale.z);
        }
        else if (x > 0)
        {
            transform.localScale = new Vector3(localXScale, transform.localScale.y, transform.localScale.z);
        }

        mainCamera.Reposition(new Vector3(transform.position.x, transform.position.y, transform.position.z - 5));
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
        if(Globals.gameRound == 1)
        {
            playerName = Com.Mercury.Game.GameManager.Instance.GeneratePlayerName();
            txtName.text = playerName;
            Globals.LocalPlayerInfo.name = playerName;
        } else
        {
            playerName = Globals.LocalPlayerInfo.name;
            txtName.text = playerName;
        }

        /*
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
            playerName = "" + System.Convert.ToChar(toConvert);
        }
        else
        {
            playerName = "" + System.Convert.ToChar(64 + counter) + System.Convert.ToChar(toConvert - 26);
        }
        txtName.text = playerName;
        Globals.LocalPlayerInfo.name = playerName;
        */
    }

    [Obsolete]
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            if (messagesBox.active)
                messagesBox.SetActive(false);
            else if (Vector3.Distance(transform.position, LocalPlayerInstance.transform.position) < 2f)
            {
                messagesBox.GetComponent<MessageBoxManager>().UpdateClues(PlayerManager.LocalPlayerManager.cluesManager.GetAllClues());
                messagesBox.SetActive(true);
            }
        }
    }

    #endregion
}
