using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public void Reposition(Vector3 pos)
    {
        transform.position = pos;
    }
}
