using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;


namespace Com.Mercury.Game
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

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

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
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
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
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

        #endregion
    }
}