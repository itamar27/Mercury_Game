using Photon.Pun;
using System;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun, IComparable
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

    private CameraBehaviour mainCamera;

    private Rigidbody2D rigidbody2d;

    private float localXScale;
    private float speed = 2f;
    private float x = 0f;
    private float y = 0f;

    #endregion

    #region Delegates

    /*
    public delegate void JoinedRoom(PlayerManager playerManager);
    public static JoinedRoom joinedRoomDelegate;

    public delegate void LeftRoom(PlayerManager playerManager);
    public static LeftRoom leftRoomDelegate;
    */

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
            PlayerManager.LocalPlayerManager = this.GetComponent<PlayerManager>();
            PlayerManager.cluesManager = new CluesManager();
        }
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //JoinedRoomEvent();
        
        GetName();

        localXScale = transform.localScale.x;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehaviour>();

        messagesBox.SetActive(false);
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

        Move(x * Time.deltaTime, y * Time.deltaTime);
    }

    #endregion

    #region Pun Methods

    private void SendMessage(Clue clue)
    {
        string from = PlayerManager.LocalPlayerManager.playerName;
        this.photonView.RPC("ChatMessage", RpcTarget.Others, from, clue.GetHistory(), this.playerName, clue.GetMessage());
        Chat.Instance.SendMessageToChat(string.Format("<color=blue>To " + this.playerName + ":</color> " + clue.GetMessage()));
    }

    #endregion

    #region Pun Callbacks

    [PunRPC]
    void ChatMessage(string from, string histrory, string to, string message) 
    { 
        if(to == LocalPlayerManager.playerName)
        {
            Chat.Instance.SendMessageToChat(string.Format("<color=red>" + from + ":</color> " + message));

            PlayerManager.cluesManager.AddMessage(message, histrory, to);
        }
    }

    #endregion

    #region Public Methods

    public void onMessageClicked(int clueIndex) // Need to be caught from event
    {
        SendMessage(PlayerManager.cluesManager.GetClueAt(clueIndex));
        messagesBox.SetActive(false);
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
        while(toConvert > 90)
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
    }

    private void OnMouseDown()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            if(messagesBox.active)
                messagesBox.SetActive(false);
            else if (Vector3.Distance(transform.position, LocalPlayerInstance.transform.position) < 2f)
            {
                messagesBox.GetComponent<MessageBoxManager>().UpdateClues(PlayerManager.cluesManager.GetAllClues());
                messagesBox.SetActive(true);
            }
        }
    }

    #endregion

}
