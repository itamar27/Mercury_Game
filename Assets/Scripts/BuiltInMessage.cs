using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CluesManager
{
    #region Private Members
    private List<Clue> clues;
    #endregion

    #region Constructor
    public CluesManager()
    {
        clues = new List<Clue>();
        clues.Add(new Clue("Hello", "", "", 0));
        clues.Add(new Clue("Have a nice day", "", "", 0));
    }
    #endregion

    #region Public Methods

    public void InitClues(string name)
    {
        clues.Add(new Clue("What a hot day", "", name, 0));
        clues.Add(new Clue("I am feeling tried", "", name, 0));
    }

    public void AddMessage(string message, string history, string name, int score, bool addToPanel = false)
    {
        foreach (Clue clue in clues)
        {
            if (clue.GetMessage() == message)
                return;
        }

        if (addToPanel)
            if (score == 1 || score == 2)
                CluesPanel.Instance.AddClue(message);

        clues.Add(new Clue(message, history, name, score));
    }

    public Clue GetClueAt(int index)
    {
        return clues[index];
    }

    public List<Clue> GetAllClues()
    {
        return new List<Clue>(clues);
    }

    public void ClearAllClues()
    {
        clues.Clear();
    }
    #endregion

    #region Private Methods

    #endregion
}

public class BuiltInMessage
{
    private string message;

    public BuiltInMessage(string _message)
    {
        this.message = _message;
    }

    public string GetMessage()
    {
        return message;
    }
}

public class Clue : BuiltInMessage
{
    private string messageHistory;
    private string clueType;
    private int score;

    public Clue(string message, string history, string name, int score) : base(message)
    {
        this.score = score;

        if (score == 0)
            clueType = "trivial";
        else if (score == 1)
            clueType = "common";
        else if (score == 2)
            clueType = "hidden";

        if (name == "")
        {
            messageHistory = "";
            //score = 1;
            //clueType = "common";
        }
        else
        {
            if (history == "")
                messageHistory = name;
            else
                messageHistory = history + "," + name;

            //score = 2;
            //clueType = "hidden";
        }
    }

    public string GetHistory()
    {
        return messageHistory;
    }

    public string GetClueType()
    {
        return clueType;
    }

    public int GetScore()
    {
        return score;
    }
}

public class CluesFactory
{
    private Dictionary<int, string> clues;
    private Dictionary<int, string> cluesProperties;
    public CluesFactory()
    {
        clues = new Dictionary<int, string>();
        cluesProperties = new Dictionary<int, string>();

        //Hair color
        clues.Add(1, "The venom has green hair");
        clues.Add(2, "The venom has blue hair");
        clues.Add(3, "The venom has pink hair");
        //Gender
        clues.Add(4, "The venom is a male");
        clues.Add(5, "The venom is a female");
        //Skin color
        clues.Add(6, "The venom has light skin");
        clues.Add(7, "The venom has dark skin");
        //Holding
        clues.Add(8, "The venom has a knife");
        clues.Add(9, "The venom has an axe");
        clues.Add(10, "The venom has a microscope");

        //Hair color
        cluesProperties.Add(1, "Green");
        cluesProperties.Add(2, "Blue");
        cluesProperties.Add(3, "Pink");
        //Gender
        cluesProperties.Add(4, "Male");
        cluesProperties.Add(5, "Female");
        //Skin color
        cluesProperties.Add(6, "Light");
        cluesProperties.Add(7, "Dark");
        //Holding
        cluesProperties.Add(8, "Knife");
        cluesProperties.Add(9, "Axe");
        cluesProperties.Add(10, "Microscope");
    }

    public KeyValuePair<string, string> GetCluePropertyById(int clueId)
    {
        if (clueId < 4)
        {
            string type = "hair";
            string property = cluesProperties[clueId];
            return new KeyValuePair<string, string>(type, property);
        }
        else if (clueId < 6)
        {
            string type = "gender";
            string property = cluesProperties[clueId];
            return new KeyValuePair<string, string>(type, property);
        }
        else if (clueId < 8)
        {
            string type = "color";
            string property = cluesProperties[clueId];
            return new KeyValuePair<string, string>(type, property);
        }
        else
        {
            string type = "items";
            string property = cluesProperties[clueId];
            return new KeyValuePair<string, string>(type, property);
        }
    }
    public string GetClueById(int clueId)
    {
        return clues[clueId];
    }
}

public class AppearancesClues
{
    private Dictionary<int, List<int>> cluesByAppearanceId;

    public AppearancesClues()
    {
        cluesByAppearanceId = new Dictionary<int, List<int>>();

        cluesByAppearanceId.Add(1, new List<int> { 1, 4, 6, 9 });
        cluesByAppearanceId.Add(2, new List<int> { 3, 4, 6, 9 });
        cluesByAppearanceId.Add(3, new List<int> { 2, 4, 6, 9 });
        cluesByAppearanceId.Add(4, new List<int> { 2, 4, 6, 10 });
        cluesByAppearanceId.Add(5, new List<int> { 1, 4, 6, 10 });
        cluesByAppearanceId.Add(6, new List<int> { 3, 4, 6, 10 });
        cluesByAppearanceId.Add(7, new List<int> { 3, 4, 6, 8 });
        cluesByAppearanceId.Add(8, new List<int> { 1, 4, 6, 8 });
        cluesByAppearanceId.Add(9, new List<int> { 2, 4, 6, 8 });
        cluesByAppearanceId.Add(10, new List<int> { 2, 5, 6, 8 });
        cluesByAppearanceId.Add(11, new List<int> { 1, 5, 6, 8 });
        cluesByAppearanceId.Add(12, new List<int> { 3, 5, 6, 8 });
        cluesByAppearanceId.Add(13, new List<int> { 3, 5, 6, 10 });
        cluesByAppearanceId.Add(14, new List<int> { 2, 5, 6, 10 });
        cluesByAppearanceId.Add(15, new List<int> { 1, 5, 6, 10 });
        cluesByAppearanceId.Add(16, new List<int> { 3, 5, 6, 9 });
        cluesByAppearanceId.Add(17, new List<int> { 1, 5, 6, 9 });
        cluesByAppearanceId.Add(18, new List<int> { 2, 5, 6, 9 });
        cluesByAppearanceId.Add(19, new List<int> { 2, 5, 7, 9 });
        cluesByAppearanceId.Add(20, new List<int> { 1, 5, 7, 9 });
        cluesByAppearanceId.Add(21, new List<int> { 3, 5, 7, 9 });
        cluesByAppearanceId.Add(22, new List<int> { 3, 5, 7, 8 });
        cluesByAppearanceId.Add(23, new List<int> { 2, 5, 7, 8 });
        cluesByAppearanceId.Add(24, new List<int> { 1, 5, 7, 8 });
        cluesByAppearanceId.Add(25, new List<int> { 1, 5, 7, 10 });
        cluesByAppearanceId.Add(26, new List<int> { 2, 5, 7, 10 });
        cluesByAppearanceId.Add(27, new List<int> { 3, 5, 7, 10 });
        cluesByAppearanceId.Add(28, new List<int> { 3, 4, 7, 10 });
        cluesByAppearanceId.Add(29, new List<int> { 2, 4, 7, 10 });
        cluesByAppearanceId.Add(30, new List<int> { 1, 4, 7, 10 });
        cluesByAppearanceId.Add(31, new List<int> { 1, 4, 7, 9 });
        cluesByAppearanceId.Add(32, new List<int> { 2, 4, 7, 9 });
        cluesByAppearanceId.Add(33, new List<int> { 3, 4, 7, 9 });
        cluesByAppearanceId.Add(34, new List<int> { 3, 4, 7, 8 });
        cluesByAppearanceId.Add(35, new List<int> { 2, 4, 7, 8 });
        cluesByAppearanceId.Add(36, new List<int> { 1, 4, 7, 8 });
    }

    public Dictionary<string, string> GetCluesPropertiesById(int id)
    {
        CluesFactory cluesFactory = new CluesFactory();
        Dictionary<string, string> properties = new Dictionary<string, string>();
        foreach (int propertyId in cluesByAppearanceId[id])
        {
            KeyValuePair<string, string> property = cluesFactory.GetCluePropertyById(propertyId);
            properties.Add(property.Key, property.Value);
        }
        return properties;
    }

    public List<int> GetCluesById(int appearanceId)
    {
        return cluesByAppearanceId[appearanceId];
    }
}
