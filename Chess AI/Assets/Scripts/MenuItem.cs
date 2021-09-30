using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public int option;
    private void OnMouseDown()
    {
        Piece parent = transform.parent.GetComponentInParent<Piece>();
        parent.manager.promotePiece(parent.x, parent.y, option);
    }
}
