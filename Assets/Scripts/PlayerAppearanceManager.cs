using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAppearanceManager : MonoBehaviour
{
    [SerializeField] private List<Sprite> playersProfile;
    [SerializeField] private List<Sprite> playersDead;

    public static RuntimeAnimatorController GetAnimatorController(int id)
    {
        return Resources.Load("Animators/Character_" + (id)) as RuntimeAnimatorController;   
    }

    public Sprite GetProfileSprite(int id)
    {
        return playersProfile[id - 1];
    }

    public Sprite GetDeadSprite(int id)
    {
        return playersDead[id - 1];
    }
}
