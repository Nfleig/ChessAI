using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    private Piece parent;
    public SimpleChess.Move move;
    bool canMove = false;
    bool done = false;

    void Awake()
    {
        parent = GetComponentInParent<Piece>();
    }

    private void Update()
    {
        canMove = true;
    }
    private void OnEnable()
    {
        done = false;
    }

    void OnMouseDown()
    {
        //print("Moving piece");
        if (canMove && !done)
        {
            parent.makeMove(move);
            done = true;
        }
    }
}
