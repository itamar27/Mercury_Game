using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHover : MonoBehaviour
{
    [SerializeField] private Image image;
    private Color onEnterColor;
    private Color onExitColor;

    private void Awake()
    {
        onEnterColor = new Color(0, 0, 0, 90);
        onExitColor = new Color(0, 0, 0, 150);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = onEnterColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = onExitColor;
    }
}
