using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetails : MonoBehaviour
{
    [SerializeField] PlayerAppearanceManager appearanceManager;

    [SerializeField] Text nameTxt;
    [SerializeField] Text roleTxt;
    [SerializeField] Image playerImage;

    void Start()
    {
        nameTxt.text = "Name: " + Globals.playersNames[PhotonNetwork.LocalPlayer.ActorNumber];

        if (Globals.LocalPlayerInfo.role == "Venom")
            roleTxt.text = string.Format("<color=red>Role: Venom</color>");
        else
            roleTxt.text = string.Format("<color=green>Role: Citizen</color>");
        playerImage.sprite = appearanceManager.GetProfileSprite(Globals.LocalPlayerInfo.appearanceId);
    }
}
