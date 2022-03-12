using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    #region Serialized Fields

    [SerializeField] private Text playerCountText;
    [SerializeField] private Text timeLeftText;
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private Canvas waitingRoomCanvas;

    #endregion

    #region Private Fields

    private bool readyToLoadScene;
    private double startTime;
    private double timeLeft;

    private ExitGames.Client.Photon.Hashtable CustomeValue;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        readyToLoadScene = true;
    }

    void Start()
    {
        loadingCanvas.enabled = false;
        CustomeValue = new ExitGames.Client.Photon.Hashtable();
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time + 20;
            CustomeValue.Add("StartTime", startTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
        else
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
        }
    }

    private void Update()
    {
        timeLeft = startTime - PhotonNetwork.Time;
        if (timeLeft <= 0)
        {
            if (readyToLoadScene == true)
            {
                readyToLoadScene = false;
                loadingCanvas.enabled = true;
                waitingRoomCanvas.enabled = false;

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PickVenoms();
                    StartCoroutine(SwitchToGameRoom());
                }
            }

            try
            {
                int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

                int venom1 = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Venom1"].ToString());
                int venom2 = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Venom2"].ToString());
                int venom3 = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Venom3"].ToString());

                if (venom1 == localActorNumber || venom2 == localActorNumber || venom3 == localActorNumber)
                {
                    Globals.LocalPlayerInfo.role = "Venom";
                }
                else
                {
                    Globals.LocalPlayerInfo.role = "Citizen";
                }
            }
            catch(Exception exc) {}
        }
    }

    private IEnumerator SwitchToGameRoom()
    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel("GameRoomMain");
    }

    void LateUpdate()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        playerCountText.text = "Players count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + Globals.GameConfig.maxPlayers;
        timeLeftText.text = "Starting in: " + (int)timeLeft;
    }

    #endregion

    #region Public Methods  

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    #endregion

    #region Private Methods

    private void PickVenoms()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        int venomsNumber = 0;
        List<int> venomsList = new List<int>();

        if(players.Count > 20)
        {
            venomsNumber = 3;
        }
        else if(players.Count > 10)
        {
            venomsNumber = 2;
        }
        else
        {
            venomsNumber = 1;
        }

        while (venomsList.Count < venomsNumber)
        {
            venomsList.Clear();
            for (int i = 0; i < venomsNumber; i++)
            {
                int newVenomIndex = UnityEngine.Random.Range(1, players.Count + 1);

                if (venomsList.Contains(newVenomIndex))
                    break;
                else
                    venomsList.Add(newVenomIndex);
            }
        }

        if (CustomeValue.ContainsKey("Venom1") == false)
        {
            CustomeValue.Add("Venom1", venomsList[0]);
        }        
        if (CustomeValue.ContainsKey("Venom2") == false)
        {
            if (venomsNumber > 1)
                CustomeValue.Add("Venom2", venomsList[1]);
            else
                CustomeValue.Add("Venom2", -1);
        }
        if (CustomeValue.ContainsKey("Venom3") == false)
        {
            if (venomsNumber > 2)
                CustomeValue.Add("Venom3", venomsList[2]);
            else
                CustomeValue.Add("Venom3", -1);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
    }

    #endregion
}
