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
    [SerializeField] private Text botIncludedText;
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
        Globals.gameRound++;
        PhotonNetwork.AutomaticallySyncScene = true;
        readyToLoadScene = true;

        CustomeValue = new ExitGames.Client.Photon.Hashtable();

        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time + 10;

            if (CustomeValue.ContainsKey("StartTime"))
                CustomeValue["StartTime"] = startTime;
            else
                CustomeValue.Add("StartTime", startTime);

            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
    }

    void Start()
    {
        loadingCanvas.enabled = false;

        if (Globals.gameRound == 3)
            botIncludedText.text = "This round you will play with Smart Agents";
        else
            botIncludedText.text = "";

        if (PhotonNetwork.IsMasterClient == false)
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient == false)
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());

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
                    foreach (var item in PhotonNetwork.CurrentRoom.CustomProperties)
                    {
                        Debug.Log("Key: " + item.Key + " | Value: " + item.Value);
                    }

                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PickVenoms();
                    if (Globals.gameRound == 1)
                    {
                        PickPlayersAppearance();
                        PickPlayersNames();
                    }
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

                Globals.Results.venoms.Add(venom1);
                Globals.Results.venoms.Add(venom2);
                Globals.Results.venoms.Add(venom3);
            }
            catch { }

            try
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players)
                {
                    string name = PhotonNetwork.CurrentRoom.CustomProperties[player.Value.ActorNumber.ToString()].ToString();
                    Globals.playersNames.Add(player.Value.ActorNumber, name);
                }

                Globals.GameConfig.botNameId1 = PhotonNetwork.CurrentRoom.CustomProperties["BotName1"].ToString();
                Globals.GameConfig.botNameId2 = PhotonNetwork.CurrentRoom.CustomProperties["BotName2"].ToString();
                Globals.GameConfig.botNameId3 = PhotonNetwork.CurrentRoom.CustomProperties["BotName3"].ToString();
            }
            catch { }

            try
            {
                int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                int appearId = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["appear" + localActorNumber].ToString());
                Globals.LocalPlayerInfo.appearanceId = appearId;

                int botAppear1 = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["botAppear" + 1].ToString());
                int botAppear2 = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["botAppear" + 2].ToString());
                int botAppear3 = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["botAppear" + 3].ToString());

                Globals.GameConfig.botAppearId1 = botAppear1;
                Globals.GameConfig.botAppearId2 = botAppear2;
                Globals.GameConfig.botAppearId3 = botAppear3;
            }
            catch { }
        }
    }

    private IEnumerator SwitchToGameRoom()
    {
        yield return new WaitForSeconds(10);
        PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel("GameRoomMain");
    }

    void LateUpdate()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("No room in LateUpdate");
            return;
        }

        playerCountText.text = "Players count: " + PhotonNetwork.CurrentRoom.PlayerCount;
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

    private void PickPlayersAppearance()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        List<int> appearances = new List<int>();
        for (int i = 1; i < 18; i++)
        {
            appearances.Add(i);
        }

        for (int i = 1; i < players.Count + 1; i++)
        {
            if (CustomeValue.ContainsKey("appear" + i) == false)
            {
                int selectedAppear = UnityEngine.Random.Range(0, appearances.Count);
                CustomeValue.Add("appear" + i, appearances[selectedAppear]);
                appearances.RemoveAt(selectedAppear);
            }
        }

        for (int i = 1; i < 4; i++)
        {
            if (CustomeValue.ContainsKey("botAppear" + i) == false)
            {
                int selectedAppear = UnityEngine.Random.Range(0, appearances.Count);
                CustomeValue.Add("botAppear" + i, appearances[selectedAppear]);
                appearances.RemoveAt(selectedAppear);
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
    }

    private void PickVenoms()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        int venomsNumber = 0;
        List<int> venomsList = new List<int>();

        if (players.Count >= 16)
        {
            venomsNumber = 3;
        }
        else if (players.Count >= 8)
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

                if (venomsList.Contains(newVenomIndex) || Globals.Results.venoms.Contains(newVenomIndex))
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

    private void PickPlayersNames()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        foreach (var player in players)
        {
            Globals.playersNames.Add(player.Value.ActorNumber, GeneratePlayerName());
        }

        Globals.GameConfig.botNameId1 = GeneratePlayerName();
        Globals.GameConfig.botNameId2 = GeneratePlayerName();
        Globals.GameConfig.botNameId3 = GeneratePlayerName();

        foreach (var playerName in Globals.playersNames)
        {
            CustomeValue.Add(playerName.Key.ToString(), playerName.Value);
        }

        CustomeValue.Add("BotName1", Globals.GameConfig.botNameId1);
        CustomeValue.Add("BotName2", Globals.GameConfig.botNameId2);
        CustomeValue.Add("BotName3", Globals.GameConfig.botNameId3);

        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
    }

    private string GeneratePlayerName()
    {
        int random = UnityEngine.Random.Range(0, Globals.names.Count);
        string name = Globals.names[random];
        CustomeValue = PhotonNetwork.CurrentRoom.CustomProperties;
        while (CustomeValue.ContainsKey(name))
        {
            random = UnityEngine.Random.Range(0, Globals.names.Count);
            name = Globals.names[random];
        }
        CustomeValue.Add(name, name);
        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        return name;
    }

    #endregion
}
