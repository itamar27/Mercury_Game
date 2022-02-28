using System.Collections;
using System.Collections.Generic;

public class CluesManager
{
    #region Private Members
    private List<Clue> clues;
    #endregion

    #region Constructor
    public CluesManager()
    {
        clues = new List<Clue>();
        clues.Add(new Clue("Hello", "", ""));
        clues.Add(new Clue("Have a nice day", "", ""));
    }
    #endregion

    #region Public Methods

    public void startCluesMechanism(string name, int clueID)
    {
        clues.Clear();

        clues.Add(new Clue("I suspect ...", "", name));
        clues.Add(new Clue("I trust ...", "", name));
    }

    public void AddMessage(string message, string history, string name)
    {
        foreach(Clue clue in clues)
        {
            if (clue.GetMessage() == message)
                return;
        }

        clues.Add(new Clue(message, history, name));
    }

    public Clue GetClueAt(int index)
    {
        return clues[index];
    }

    public List<Clue> GetAllClues()
    {
        return new List<Clue>(clues);
    }
    #endregion
}

public class BuiltInMessage
{
    private string message;
    protected int score;

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

    public Clue(string message, string history, string name) : base(message)
    {
        if (name == "")
            messageHistory = "";
        else if (history == "")
            messageHistory = name;
        else
            messageHistory = history + "," + name;
    }

    public string GetHistory()
    {
        return messageHistory;
    }

    public string GetClueType()
    {
        return clueType;
    }
}
