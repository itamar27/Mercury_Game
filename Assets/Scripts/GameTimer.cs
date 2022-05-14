using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviourPunCallbacks
{
    #region Serialized Fields

    [SerializeField] private Text timeLeftText;

    #endregion

    #region Private Fields

    private double startTime;
    private double timeLeft;
    private ExitGames.Client.Photon.Hashtable CustomeValue;

    private int timeLeftMinutes;
    private string minutesLeftString;
    private int timeLeftSeconds;
    private string secondsLeftString;


    private double dayTime;
    private double voteTime;
    private double nightTime;

    #endregion

    #region Delegates

    public delegate void TimerDone();
    public static TimerDone timerDoneDelegate;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        dayTime = 30;
        voteTime = 30;
        nightTime = 30;
        CustomeValue = new ExitGames.Client.Photon.Hashtable();

        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time + dayTime;
            CustomeValue.Add("StartTime", startTime);
            CustomeValue.Add("NextAct", false);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
        }
    }

    private void Update()
    {
        timeLeft = startTime - PhotonNetwork.Time;

        if (timeLeft <= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                timerDoneDelegate();

                switch (Com.Mercury.Game.GameManager.gameAct)
                {
                    case Globals.GameAct.Day:
                        startTime = PhotonNetwork.Time + dayTime;
                        break;
                    case Globals.GameAct.Vote:
                        startTime = PhotonNetwork.Time + voteTime;
                        break;
                    case Globals.GameAct.Night:
                        startTime = PhotonNetwork.Time + nightTime;
                        break;
                }
                if (CustomeValue.ContainsKey("StartTime") == false)
                {
                    CustomeValue.Add("StartTime", startTime);
                }
                else
                {
                    CustomeValue["StartTime"] = startTime;
                }

                if (CustomeValue.ContainsKey("NextAct") == false)
                {
                    CustomeValue.Add("NextAct", true);
                }
                else
                {
                    CustomeValue["NextAct"] = true;
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
                StartCoroutine(SetNextActFalse());
            }
            else
            {
                try
                {
                    startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
                    if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["NextAct"])
                    {
                        timerDoneDelegate();
                    }
                }
                catch
                {
                    Debug.Log("Waiting for master");
                }
            }
        }
    }

    private IEnumerator SetNextActFalse()
    {
        yield return new WaitForSeconds(1);
        if (CustomeValue.ContainsKey("NextAct") == false)
        {
            CustomeValue.Add("NextAct", false);
        }
        else
        {
            CustomeValue["NextAct"] = false;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
    }

    void LateUpdate()
    {
        UpdateTimer();
    }

    #endregion

    #region Private Methods

    private void UpdateTimer()
    {
        timeLeftMinutes = (int)(timeLeft / 60);
        timeLeftSeconds = (int)(timeLeft % 60);

        if (timeLeftMinutes < 10)
            minutesLeftString = "0" + timeLeftMinutes;
        else minutesLeftString = timeLeftMinutes.ToString();
        if (timeLeftSeconds < 10)
            secondsLeftString = "0" + timeLeftSeconds;
        else secondsLeftString = timeLeftSeconds.ToString();

        timeLeftText.text = minutesLeftString + ":" + secondsLeftString;
    }

    #endregion

}
