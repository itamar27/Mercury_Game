using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CluesPanel : MonoBehaviour
{
    [SerializeField] private Canvas cluesCanvas;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Image notificationImage;
    [SerializeField] private Text cluesText;

    private bool isOpen;
    private bool panelEnabled;

    private static CluesPanel instance;
    public static CluesPanel Instance
    {
        get
        {
            if (instance == null)
                return new CluesPanel();

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cluesCanvas.enabled = false;
        this.GetComponent<Button>().interactable = false;
        UnityEngine.Color color = this.GetComponent<Image>().color;
        color.a = 0;
        this.GetComponent<Image>().color = color;
        color = buttonImage.color;
        color.a = 0;
        buttonImage.color = color;
        notificationImage.enabled = false;
        panelEnabled = false;
    }

    public void OnClick()
    {
        notificationImage.enabled = false;

        cluesCanvas.enabled = !cluesCanvas.enabled;
    }

    public void AddClue(string clue)
    {
        notificationImage.enabled = true;

        if (panelEnabled == false)
        {
            panelEnabled = true;
            this.GetComponent<Button>().interactable = true;
            UnityEngine.Color color = this.GetComponent<Image>().color;
            color.a = 1;
            this.GetComponent<Image>().color = color;
            color = buttonImage.color;
            color.a = 1;
            buttonImage.color = color;
            cluesText.text = clue;
            return;
        }

        cluesText.text += "\n" + clue;
    }
}
