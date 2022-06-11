using UnityEngine;
using UnityEngine.UI;

public class EliminationScript : MonoBehaviour
{
    [SerializeField] public PlayerAppearanceManager appearanceManager;
    [SerializeField] public Image bloodPuddle;    
    [SerializeField] public Text elimination;
    [SerializeField] public Image player;

    public string playerName { get; set; }
    public int playerAppearanceId { get; set; }

    private void Awake()
    {
        playerName = "TEST";
        playerAppearanceId = 1;
    }

    public void UpdateCanvas(string name, int id, bool venomKill)
    {
        if (venomKill)
        {
            bloodPuddle.enabled = true;
            elimination.text = "The Venoms has killed " + name + " during the night...";
        }
        else
        {
            bloodPuddle.enabled = false;
            elimination.text = "The council has decided to eliminate " + name + " from the settlement...";
        }

        player.sprite = appearanceManager.GetDeadSprite(id);
    }
}
