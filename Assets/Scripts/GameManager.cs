using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Random = UnityEngine.Random;
using System.Linq;

namespace Com.Mercury.Game
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Serialize Fields

        [SerializeField] public Text actAndRoundText;
        [SerializeField] public Text roleText;
        [SerializeField] public Text nameText;
        [SerializeField] public Text winText;

        [SerializeField] public GameObject chatPanel;

        [SerializeField] public Canvas deathCanvas;
        [SerializeField] public Canvas winCanvas;
        [SerializeField] public Canvas councilCanvas;
        [SerializeField] public Canvas eliminationCanvas;
        [SerializeField] public Canvas venomNightCanvas;
        [SerializeField] public Canvas citizenNightCanvas;

        [SerializeField] public VotesPanelManager votePanelManager;
        [SerializeField] public VotesPanelManager venomVotePanelManager;
        [SerializeField] public EliminationScript eliminationScript;

        #endregion

        #region Public Fields

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        public static Globals.GameAct gameAct;

        public int round;

        public bool isDead;

        #endregion

        #region Private Fields

        private ExitGames.Client.Photon.Hashtable CustomeValue;

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
            PhotonNetwork.AutomaticallySyncScene = true;
            instance = this;
            gameAct = Globals.GameAct.Day;
            round = 1;
            if (PhotonNetwork.CurrentRoom.CustomProperties == null)
                CustomeValue = new ExitGames.Client.Photon.Hashtable();
            else
                CustomeValue = PhotonNetwork.CurrentRoom.CustomProperties;

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }

        private void Start()
        {
            councilCanvas.enabled = false;
            venomNightCanvas.enabled = false;
            citizenNightCanvas.enabled = false;
            eliminationCanvas.enabled = false;
            deathCanvas.enabled = false;
            winCanvas.enabled = false;

            isDead = false;

            if (PhotonNetwork.IsMasterClient)
            {
                switch (Globals.gameRound)
                {
                    case 1:
                        break;
                    case 2:
                        PhotonNetwork.InstantiateRoomObject("Agent", new Vector3(0, 0, -1.1f), Quaternion.identity, 0);
                        break;
                    case 3:
                        PhotonNetwork.InstantiateRoomObject("Agent", new Vector3(0, 0, -1.1f), Quaternion.identity, 0);
                        break;
                }
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
                    Debug.Log("Local Player Instantiate");
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }

            roleText.text = "Role: " + Globals.LocalPlayerInfo.role;
            nameText.text = "Name: " + Globals.playersNames[PlayerManager.LocalPlayerManager.photonView.Owner.ActorNumber];
            Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> Welcome to Mercury, please enjoy this time to chat with everyone."));

            Globals.Results.gamePhase = Globals.GamePhase.OnGoing;

            StartCoroutine(UpdatePlayerData());
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

        public void AddToVotePanel(string name)
        {
            votePanelManager.AddVote(name);
        }

        public void AddToVenomVotePanel(string name)
        {
            venomVotePanelManager.AddVote(name);
        }
        
        #endregion

        #region Private Methods

        private IEnumerator UpdatePlayerData()
        {
            /*
            Dictionary<string, object> playerData = new Dictionary<string, object>();

            if (Globals.gameRound == 1)
            {
                AppearancesClues appearanceClues = new AppearancesClues();
                var appearance = appearanceClues.GetCluesPropertiesById(Globals.LocalPlayerInfo.appearanceId);
                string appearanceJsonString = JsonConvert.SerializeObject(appearance);
                playerData.Add("game_appearance", appearance);

                string name = PlayerManager.LocalPlayerInstance.name;
                playerData.Add("character_name", Globals.LocalPlayerInfo.name);
            }

            if (Globals.LocalPlayerInfo.role == "Venom")
            {
                playerData.Add("was_killer", true);
                playerData.Add("killer_round", Globals.gameRound);
            }

            HttpService.Instance.result = "";
            HttpService.Instance.Patch("game/player/" + Globals.PlayerProfile.id + "/update/", playerData);
            yield return new WaitUntil(() => HttpService.Instance.result != "");
            var parsed = HttpService.Instance.GetParsedResult();
            */
            yield return new WaitForSeconds(0);
        }

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

        private IEnumerator NextGameRound()
        {
            var agents = GameObject.FindGameObjectsWithTag("Agent");
            foreach (var agent in agents)
            {
                var agentView = agent.GetComponent<PhotonView>();
                PhotonNetwork.Destroy(agentView);
            }

            yield return new WaitForSeconds(3);
            PhotonNetwork.AutomaticallySyncScene = true;
            if (Globals.gameRound < 3)
                PhotonNetwork.LoadLevel("WaitingRoom");
            else
                PhotonNetwork.LoadLevel("Launcher");
        }

        private void NextAct()
        {
            if (Globals.Results.gamePhase != Globals.GamePhase.OnGoing)
                return;

            if (CheckWinCondition())
            {
                winCanvas.enabled = true;
                if (Globals.Results.gamePhase == Globals.GamePhase.CitizenWin)
                {
                    winText.text = "The colony has elimanated all the venoms!";
                }
                else if (Globals.Results.gamePhase == Globals.GamePhase.VenomWin)
                {
                    winText.text = "The Venoms has elimanated all the colony!";
                }

                if (isDead == false)
                {
                    isDead = true;
                    PhotonNetwork.Destroy(PlayerManager.LocalPlayerInstance.GetComponent<PhotonView>());
                }
            }

            if (Globals.Results.gamePhase != Globals.GamePhase.OnGoing && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(NextGameRound());
                return;
            }

            gameAct += 1;
            gameAct = (Globals.GameAct)((int)gameAct % Enum.GetValues(typeof(Globals.GameAct)).Length);

            if (gameAct == Globals.GameAct.Day)
            {
                if (round == 1 && PhotonNetwork.IsMasterClient)
                {
                    PickClues();
                }

                if (isDead == false)
                    StartCoroutine(DayRutine());
            }
            else if (gameAct == Globals.GameAct.Vote)
            {
                VoteRutine();
            }
            else if (gameAct == Globals.GameAct.Night)
            {
                if (isDead == false)
                    StartCoroutine(NightCorutine());
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

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(AgentsVotes(agents.Length, allPlayersNames));
            }
        }

        private void InitVotePanelForVenoms()
        {
            venomVotePanelManager.ResetPanel();
            var players = GameObject.FindGameObjectsWithTag("Player");
            var agents = GameObject.FindGameObjectsWithTag("Agent");

            List<string> allPlayersNames = new List<string>();

            foreach (var player in players)
            {
                PlayerManager playerManager = player.GetComponent<PlayerManager>();
                if (playerManager.playerRole != "Venom")
                {
                    string playerName = playerManager.playerName;
                    allPlayersNames.Add(playerName);
                }
            }
            foreach (var agent in agents)
                allPlayersNames.Add(agent.GetComponent<AgentManager>().playerName);

            allPlayersNames.Sort();

            foreach (var playerName in allPlayersNames)
                venomVotePanelManager.AddBar(playerName);
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

        void CheckForPlayerElimination(string mostVoted)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            int playerAppear = -1;
            foreach (var player in players)
            {
                PlayerManager playerScript = player.GetComponent<PlayerManager>();
                if (playerScript.playerName == mostVoted)
                {
                    playerAppear = playerScript.appearanceId;
                    break;
                }
            }
            if (playerAppear == -1)
            {
                var agents = GameObject.FindGameObjectsWithTag("Agent");
                foreach (var agent in agents)
                {
                    AgentManager agentScript = agent.GetComponent<AgentManager>();
                    if (agentScript.playerName == mostVoted)
                    {
                        playerAppear = agentScript.appearanceId;
                        break;
                    }
                }
            }

            PlayerManager.LocalPlayerManager.onPlayerEliminated(mostVoted, playerAppear);
        }

        public void EliminatePlayer(string mostVoted, int playerAppear)
        {
            eliminationScript.UpdateCanvas(mostVoted, playerAppear);

            PhotonView toDestroy = null;
            var players = GameObject.FindGameObjectsWithTag("Player");
            Photon.Realtime.Player potentialMaster = null;
            foreach (var player in players)
            {
                if (toDestroy != null && potentialMaster != null) break;

                PlayerManager playerScript = player.GetComponent<PlayerManager>();
                if (playerScript.playerName == mostVoted)
                {
                    toDestroy = player.GetComponent<PhotonView>();
                }
                else if (potentialMaster == null)
                {
                    potentialMaster = PhotonNetwork.CurrentRoom.GetPlayer(player.GetPhotonView().OwnerActorNr);
                }
            }

            var agents = GameObject.FindGameObjectsWithTag("Agent");
            foreach (var agent in agents)
            {
                AgentManager agentScript = agent.GetComponent<AgentManager>();
                if (agentScript.playerName == mostVoted)
                {
                    toDestroy = agentScript.GetComponent<PhotonView>();
                    break;
                }
            }

            if (mostVoted == PlayerManager.LocalPlayerManager.playerName)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.SetMasterClient(potentialMaster);
                }
                PhotonNetwork.Destroy(toDestroy);

                isDead = true;
                deathCanvas.enabled = true;
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                try
                {
                    PhotonNetwork.Destroy(toDestroy);
                }
                catch { }
            }
        }

        private IEnumerator DayRutine()
        {
            round++;

            venomNightCanvas.enabled = false;
            citizenNightCanvas.enabled = false;
            councilCanvas.enabled = false;
            chatPanel.SetActive(false);

            if (round > 1)
            {
                if (PlayerManager.LocalPlayerManager.playerRole == "Venom")
                {
                    CheckForPlayerElimination(venomVotePanelManager.MostVoted());
                }
            }
            eliminationCanvas.enabled = true;
            yield return new WaitForSeconds(5);
            eliminationCanvas.enabled = false;

            if (CheckWinCondition())
            {
                winCanvas.enabled = true;
                if (Globals.Results.gamePhase == Globals.GamePhase.CitizenWin)
                {
                    winText.text = "The colony has elimanated all the venoms!";
                }
                else if (Globals.Results.gamePhase == Globals.GamePhase.VenomWin)
                {
                    winText.text = "The Venoms has elimanated all the colony!";
                }

                if (isDead == false)
                {
                    isDead = true;
                    PhotonNetwork.Destroy(PlayerManager.LocalPlayerInstance.GetComponent<PhotonView>());
                }
            }

            if (Globals.Results.gamePhase != Globals.GamePhase.OnGoing && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(NextGameRound());
                yield break;
            }

            chatPanel.SetActive(true);
            venomNightCanvas.enabled = false;
            citizenNightCanvas.enabled = false;
            councilCanvas.enabled = false;

            actAndRoundText.text = "Day " + round;
            Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> It's a new day, enjoy it."));
        }

        private void VoteRutine()
        {
            chatPanel.SetActive(false);
            venomNightCanvas.enabled = false;
            citizenNightCanvas.enabled = false;
            if (isDead == true)
                return;

            if (round == 1)
            {
                gameAct = Globals.GameAct.Night;
                venomNightCanvas.enabled = false;
                citizenNightCanvas.enabled = false;
                councilCanvas.enabled = false;
                chatPanel.SetActive(false);
                eliminationCanvas.enabled = false;

                if (isDead == false)
                {
                    if (PlayerManager.LocalPlayerManager.playerRole == "Venom")
                    {
                        InitVotePanelForVenoms();
                        venomNightCanvas.enabled = true;
                    }
                    else
                    {
                        citizenNightCanvas.enabled = true;
                    }
                    actAndRoundText.text = "Night  " + round;
                    Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> Good night."));
                }
                return;
            }

            councilCanvas.enabled = true;
            InitVotePanel();

            actAndRoundText.text = "Council  " + round;
            Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> It's time for the Mercury council meeting."));

        }

        private IEnumerator NightCorutine()
        {
            venomNightCanvas.enabled = false;
            citizenNightCanvas.enabled = false;
            councilCanvas.enabled = false;
            chatPanel.SetActive(false);

            eliminationCanvas.enabled = true;
            string mostVoted = votePanelManager.MostVoted();
            var players = GameObject.FindGameObjectsWithTag("Player");
            PhotonView toDestroy = null;
            int playerAppear = -1;
            Photon.Realtime.Player potentialMaster = null;

            foreach (var player in players)
            {
                if (toDestroy != null && potentialMaster != null) break;

                PlayerManager playerScript = player.GetComponent<PlayerManager>();

                if (playerScript.playerName == mostVoted)
                {
                    playerAppear = playerScript.appearanceId;
                    toDestroy = player.GetComponent<PhotonView>();
                }
                else if (potentialMaster == null)
                {
                    potentialMaster = PhotonNetwork.CurrentRoom.GetPlayer(player.GetPhotonView().OwnerActorNr);
                }
            }
            if (playerAppear == -1) // No player found, search in Agents list
            {
                var agents = GameObject.FindGameObjectsWithTag("Agent");
                foreach (var agent in agents)
                {
                    AgentManager agentScript = agent.GetComponent<AgentManager>();
                    if (agentScript.playerName == mostVoted)
                    {
                        playerAppear = agentScript.appearanceId;
                        toDestroy = agent.GetComponent<PhotonView>();
                        break;
                    }
                }
            }
            eliminationScript.UpdateCanvas(mostVoted, playerAppear);

            if (mostVoted == PlayerManager.LocalPlayerManager.playerName)
            {
                PhotonNetwork.Destroy(toDestroy);

                isDead = true;
                deathCanvas.enabled = true;
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                try
                {
                    PhotonNetwork.Destroy(toDestroy);
                }
                catch { }
            }
            yield return new WaitForSeconds(5);
            eliminationCanvas.enabled = false;

            if (CheckWinCondition())
            {
                winCanvas.enabled = true;
                if (Globals.Results.gamePhase == Globals.GamePhase.CitizenWin)
                {
                    winText.text = "The colony has elimanated all the venoms!";
                }
                else if (Globals.Results.gamePhase == Globals.GamePhase.VenomWin)
                {
                    winText.text = "The Venoms has elimanated all the colony!";
                }

                if (isDead == false)
                {
                    isDead = true;
                    PhotonNetwork.Destroy(PlayerManager.LocalPlayerInstance.GetComponent<PhotonView>());
                }
            }

            if (Globals.Results.gamePhase != Globals.GamePhase.OnGoing && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(NextGameRound());
                yield break;
            }

            if (isDead == false)
            {
                if (Globals.LocalPlayerInfo.role == "Venom")
                {
                    venomNightCanvas.enabled = true;
                    InitVotePanelForVenoms();
                }
                else
                    citizenNightCanvas.enabled = true;

                actAndRoundText.text = "Night  " + round;
                Chat.Instance.SendMessageToChat(string.Format("<color=green>Mercury:</color> Good night."));
            }
        }

        private void PickClues()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            List<int> venomsAppearance = new List<int>();
            foreach (var player in players)
            {
                PlayerManager playerScript = player.GetComponent<PlayerManager>();
                if (playerScript.playerRole == "Venom")
                {
                    venomsAppearance.Add(playerScript.appearanceId);
                }
            }

            //Get All Clues Ids
            AppearancesClues appearanceClues = new AppearancesClues();
            List<int> allVenomsClues = new List<int>();
            foreach (int venomAppearance in venomsAppearance)
            {
                var clues = appearanceClues.GetCluesById(venomAppearance);
                foreach (int clue in clues)
                {
                    if (allVenomsClues.Contains(clue) == false)
                    {
                        allVenomsClues.Add(clue);
                    }
                }
            }

            //Get one clue for common and use LocalPlayer to RPC it
            int randomCommonClueIndex = UnityEngine.Random.Range(0, allVenomsClues.Count);
            int commonClue = allVenomsClues[randomCommonClueIndex];
            allVenomsClues.RemoveAt(randomCommonClueIndex);
            PlayerManager.LocalPlayerManager.UpdateCommonClue(commonClue);

            //Foreach player send through him RPC with a random hidden clue
            foreach (var player in players)
            {
                PlayerManager playerScript = player.GetComponent<PlayerManager>();
                int randomHiddenClueIndex = UnityEngine.Random.Range(0, allVenomsClues.Count);
                int hiddenClue = allVenomsClues[randomHiddenClueIndex];
                playerScript.UpdatePrivateClue(hiddenClue);
            }
        }

        private bool CheckWinCondition()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            var agents = GameObject.FindGameObjectsWithTag("Agent");

            int numCitizens = 0;
            int numVenoms = 0;

            foreach (var player in players)
            {
                PlayerManager playerScript = player.GetComponent<PlayerManager>();
                if (playerScript.playerRole == "Venom")
                    numVenoms++;
                else
                    numCitizens++;
            }

            foreach (var agent in agents)
            {
                numCitizens++;
            }

            if (numVenoms == 0)
            {
                Globals.Results.gamePhase = Globals.GamePhase.CitizenWin;
                return true;
            }
            else if (numVenoms >= numCitizens)
            {
                Globals.Results.gamePhase = Globals.GamePhase.VenomWin;
                return true;
            }

            return false;
        }

        #endregion
    }
}