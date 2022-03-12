using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Com.Mercury.Game
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Serialize Fields

        [SerializeField] public Text actAndRoundText;
        [SerializeField] public Text roleText;

        [SerializeField] public GameObject chatPanel;

        [SerializeField] public Canvas councilCanvas;
        [SerializeField] public Canvas venomNightCanvas;
        [SerializeField] public Canvas citizenNightCanvas;

        [SerializeField] public VotesPanelManager votePanelManager;

        #endregion

        #region Public Fields

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        public static Globals.GameAct gameAct;

        public int round;

        #endregion

        #region Delegates

        public delegate void PlayerJoinedRoom();
        public static PlayerJoinedRoom playerJoinedRoomDelegate;
        
        public delegate void PlayerLeftRoom();
        public static PlayerLeftRoom playerLeftRoomDelegate;

        #endregion

        #region Singleton

        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameManager();

                return instance;
            }
        }

        #endregion

        #region MonoBehaviour

        private new void OnEnable()
        {
            GameTimer.timerDoneDelegate += NextAct;
        }
        private new void OnDisable()
        {
            GameTimer.timerDoneDelegate -= NextAct;
        }

        private void Awake()
        {
            instance = this;
            gameAct = Globals.GameAct.Day;
            round = 1;
        }

        private void Start()
        {
            councilCanvas.enabled = false;
            venomNightCanvas.enabled = false;
            citizenNightCanvas.enabled = false;

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.InstantiateRoomObject("Agent", new Vector3(0, 0, -1.1f), Quaternion.identity, 0);
                PhotonNetwork.InstantiateRoomObject("Agent", new Vector3(0, 0, -1.1f), Quaternion.identity, 0);
                PhotonNetwork.InstantiateRoomObject("Agent", new Vector3(0, 0, -1.1f), Quaternion.identity, 0);
            }
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, -1f), Quaternion.identity, 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }

            roleText.text = "Role: " + Globals.LocalPlayerInfo.role;

            Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> Welcome to Mercury, please enjoy this time to chat with everyone."));
        }

        #endregion

        #region Photon Callbacks

        public override void OnLeftRoom()
        {
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            StartCoroutine(PlayerJoinedRoomEvent());
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            StartCoroutine(PlayerLeftRoomEvent());
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Launcher");
        }

        #endregion

        #region Private Methods

        void LoadLevel()
        {
            PhotonNetwork.LoadLevel("GameRoom_Main");
        }

        private IEnumerator PlayerJoinedRoomEvent()
        {
            yield return new WaitForSeconds(1);
            playerJoinedRoomDelegate();
        }

        private IEnumerator PlayerLeftRoomEvent()
        {
            yield return new WaitForSeconds(1);
            playerLeftRoomDelegate();
        }

        private void NextAct()
        {
            gameAct += 1;
            gameAct = (Globals.GameAct)((int)gameAct % Enum.GetValues(typeof(Globals.GameAct)).Length);

            if (gameAct == Globals.GameAct.Day)
            {
                chatPanel.SetActive(true);
                venomNightCanvas.enabled = false;
                citizenNightCanvas.enabled = false;
                councilCanvas.enabled = false;
                round++;
                actAndRoundText.text = "Day " + round;
                Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> It's a new day, enjoy it."));
            }
            else if(gameAct == Globals.GameAct.Vote)
            {
                chatPanel.SetActive(true);
                venomNightCanvas.enabled = false;
                citizenNightCanvas.enabled = false;
                councilCanvas.enabled = true;
                InitVotePanel();

                actAndRoundText.text = "Council  " + round;
                Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> It's time for the Mercury council meeting."));
            }
            else if(gameAct == Globals.GameAct.Night)
            {
                venomNightCanvas.enabled = false;
                citizenNightCanvas.enabled = false;
                councilCanvas.enabled = false;
                chatPanel.SetActive(false);

                if (Globals.LocalPlayerInfo.role == "Venom")
                    venomNightCanvas.enabled = true;
                else
                    citizenNightCanvas.enabled = true;

                actAndRoundText.text = "Night  " + round;
                Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> Good night."));
            }
        }

        private void InitVotePanel()
        {
            votePanelManager.ResetPanel();
            var players = GameObject.FindGameObjectsWithTag("Player");
            var agents = GameObject.FindGameObjectsWithTag("Agent");
            
            List<string> allPlayersNames = new List<string>();

            foreach (var player in players)
            {
                string playerName = player.GetComponent<PlayerManager>().playerName;
                allPlayersNames.Add(playerName);
            }
            foreach (var agent in agents)
                allPlayersNames.Add(agent.GetComponent<AgentManager>().playerName);

            allPlayersNames.Sort();

            foreach (var playerName in allPlayersNames)
                votePanelManager.AddBar(playerName);

            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(AgentsVotes(agents.Length, allPlayersNames));
            }
        }

        private IEnumerator AgentsVotes(int agentsCount, List<string> allPlayersNames)
        {
            for (int i = 0; i < agentsCount; i++)
            {
                yield return new WaitForSeconds(2);
                string voteTo = allPlayersNames[UnityEngine.Random.Range(0, allPlayersNames.Count)];
                votePanelManager.AddVote(voteTo);
                PlayerManager.LocalPlayerManager.onVoteClicked(voteTo);
            }
        }

        #endregion
    }
}