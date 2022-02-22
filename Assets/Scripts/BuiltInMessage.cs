using System.Collections;
using System.Collections.Generic;

public class CluesManager
{
    #region Private Members
    private List<Clue> clues;
    private bool firstMessage;
    #endregion

    #region Constructor
    public CluesManager()
    {
        clues = new List<Clue>();
        clues.Add(new Clue("Hello", "", ""));
        clues.Add(new Clue("Have a nice day", "", ""));

        firstMessage = true;
    }
    #endregion

    #region Public Methods
    public void AddMessage(string message, string history, string name)
    {
        if (history == "")
            return;

        if(firstMessage == true)
        {
            firstMessage = false;

            clues.Clear();
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
}
