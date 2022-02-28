using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageClicked : MonoBehaviour
{
    [SerializeField] public int index;

    private void OnMouseDown()
    {
        try
        {
            GetComponentInParent<PlayerManager>().onMessageClicked(index);
        }
        catch
        {
            GetComponentInParent<AgentManager>().onMessageClicked(index);
        }
    }
}
