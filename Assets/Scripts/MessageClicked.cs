using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageClicked : MonoBehaviour
{
    [SerializeField] public int index;

    private void OnMouseDown()
    {
        GetComponentInParent<PlayerManager>().onMessageClicked(index);
    }
}
