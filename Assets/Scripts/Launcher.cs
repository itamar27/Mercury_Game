using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Com.Mercury.Game
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField] private byte maxPlayersPerRoom = 4;
        [SerializeField] private InputField emailInput;
        [SerializeField] private InputField roomKeyInput;
        [SerializeField] private Text statusText;

        #endregion

        #region Private Fields

        string gameVersion = "1";
        bool isConnecting;

        #endregion

        #region MonoBehaviour CallBacks

        void Awake()
        {
            Globals.gameRound = 0;
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        #endregion

        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            if (isConnecting)
            {
                //The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRoom(Globals.GameConfig.roomKey);
                isConnecting = false;
            }
        }
        
        public override void OnDisconnected(DisconnectCause cause)
        {
            isConnecting = false;
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            maxPlayersPerRoom = System.Convert.ToByte(Globals.GameConfig.maxPlayers);
            PhotonNetwork.CreateRoom(Globals.GameConfig.roomKey, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            statusText.text = string.Format("<color=red>Game already started...</color>");
        }

        public override void OnJoinedRoom()
        {
            //We only load if we are the first player.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LoadLevel("WaitingRoom");
            }
        }

        #endregion

        #region Public Methods

        public void ExitGame()
        {
            Application.Quit();
        }

        public void Connect()
        {
            StartCoroutine(tryToConnect());
        }

        public IEnumerator tryToConnect()
        {
            statusText.text = "Connecting...";

            HttpService.Instance.result = "";
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("email", emailInput.text);
            HttpService.Instance.Get("game/player", queryParams);
            yield return new WaitUntil(() => HttpService.Instance.result != "");
            var parsed = HttpService.Instance.GetParsedResult();
            if(parsed == null)
            {
                if (emailInput.text == "")
                {
                    statusText.text = string.Format("<color=red>Enter your email</color>");
                    yield break;
                }
                else
                    statusText.text = string.Format("<color=red>Email not valid</color>", HttpService.Instance.result);
                yield break;
            }
            StorePlayerProfile(parsed);

            HttpService.Instance.result = "";
            queryParams = new Dictionary<string, string>();
            queryParams.Add("configuration", roomKeyInput.text);
            HttpService.Instance.Get("game/configuration", queryParams);
            yield return new WaitUntil(() => HttpService.Instance.result != "");
            parsed = HttpService.Instance.GetParsedResult();
            if (parsed == null)
            {
                if (roomKeyInput.text == "")
                {
                    statusText.text = string.Format("<color=red>Enter room key</color>");
                    yield break;
                }
                else
                    statusText.text = string.Format("<color=red>Room key not valid</color>", HttpService.Instance.result);
                yield break;
            }
            StoreGameConfig(parsed);

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRoom(Globals.GameConfig.roomKey);
            }
            else
            {
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion

        #region Private Methods

        private void StorePlayerProfile(Dictionary<string, object> playerProfile)
        {
            Globals.PlayerProfile.id = Convert.ToInt32(playerProfile["id"]);
            Globals.GameConfig.email = emailInput.text.Trim();
        }

        private void StoreGameConfig(Dictionary<string, object> gameConfig)
        {
            Globals.GameConfig.roomKey = roomKeyInput.text.Trim();
            Globals.GameConfig.startTime = Convert.ToString(gameConfig["start_time"]);
            Globals.GameConfig.agentsBehaviour = (Globals.AgentBehaviour)Convert.ToInt32(gameConfig["agents_behaviors"]);
            Globals.GameConfig.researchId = Convert.ToInt32(gameConfig["research"]);
        }

        #endregion
    }
}