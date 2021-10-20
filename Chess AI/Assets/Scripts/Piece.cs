using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public enum Color {White, Black}
    public Color color;
    public int x, y;
    private List<GameObject> markers;
    public GameObject marker;
    private bool wasClicked = false;
    private bool showingMoves = false;
    private bool myTurn = false;
    public GameManager manager;
    private Vector3 velocity = Vector3.zero;

    public void Awake()
    {

        markers = new List<GameObject>();
        manager = transform.parent.GetComponentInChildren<GameManager>();
        if (manager)
        {
            manager.addPiece(this);
        }
        //scale = transform.parent.localScale;
        //transform.localScale = new Vector3(transform.localScale.x * scale.x, transform.localScale.y * scale.y, transform.localScale.z * scale.z);
        //scale = transform.parent.TransformVector(scale);
    }
    private void Start()
    {
        if (!manager)
        {
            manager = transform.parent.GetComponentInChildren<GameManager>();
            //manager.addPiece(this);
        }
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!wasClicked)
            {
                foreach (GameObject marker in markers)
                {
                    marker.SetActive(false);
                }
                showingMoves = false;
            }
            wasClicked = false;
        }
        transform.position = Vector3.SmoothDamp(transform.position, toWorldSpace(new SimpleChess.Coordinate(x, y)), ref velocity, 0.1f);
    }
    public void OnMouseDown()
    {
        wasClicked = true;
        myTurn = (manager.turn % 2 == 0 && color == Color.Black) || (manager.turn % 2 != 0 && color == Color.White);
        if (!showingMoves && myTurn && !manager.getPaused())
        {
            //manager.tester.text = SimpleChess.toChessNotation(manager.board, false);
            List<SimpleChess.Move> moves = manager.board.generateMoves(new SimpleChess.Coordinate(x, y));
            if (moves.Count > markers.Count)
            {
                while (moves.Count > markers.Count)
                {
                    GameObject newMarker = Instantiate(marker, transform);
                    Marker markerController = newMarker.GetComponent<Marker>();
                    markerController.move = moves[markers.Count];
                    markers.Add(newMarker);
                }
            }
            for (int i = 0; i < moves.Count; i++)
            {
                markers[i].transform.position = toWorldSpace(moves[i].to);
                markers[i].GetComponent<Marker>().move = moves[i];
                markers[i].SetActive(true);
            }
            showingMoves = true;
        }
        else
        {
            foreach (GameObject marker in markers)
            {
                marker.SetActive(false);
            }
            showingMoves = false;
        }
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        //print(turn - 1);
        myTurn = (((manager.turn - 1) % 2 == 0) == (color == Color.Black));
        if(!(collision.gameObject.tag == "Marker"))
        {
            Piece otherPiece = collision.gameObject.GetComponent<Piece>();
            //print(this + " " + myTurn);
            if (otherPiece.color != color && myTurn && otherPiece.x == x && otherPiece.y == y)
            {
                capturePiece(otherPiece);
            }
        }
    }
    public Piece testSpace(Vector2 move)
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position + ((Vector3) move * 1.1f), 0.55f);
        if (collider)
        {
            return collider.gameObject.GetComponent<Piece>();
        }
        else
        {
            return null;
        }
    }
    public Vector2 toWorldSpace(SimpleChess.Coordinate space)
    {
        Vector2 newSpace = (Vector2) (transform.parent.TransformPoint(new Vector3(-3.85f + (space.x * 1.1f), -3.85f + (space.y * 1.1f))));
        return newSpace;
    }
    public void capturePiece(Piece other)
    {
        manager.removePiece(other);
        Destroy(other.gameObject);
    }
    public Piece getPiece(SimpleChess.Coordinate space)
    {
        Collider2D collider = Physics2D.OverlapCircle(toWorldSpace(space), 0.55f);
        if (collider)
        {
            return collider.gameObject.GetComponent<Piece>();
        }
        else
        {
            return null;
        }
    }
    public void makeMove(SimpleChess.Move move)
    {
        manager.makeMove(move);
        if (manager.board.getPiece(move.to) == 1 && move.to.y == 7)
        {
            GameObject menu = Instantiate(manager.WPromotionMenu, transform);
            manager.Pause();
            //promote(5);
        }
        else if (manager.board.getPiece(move.to) == -1 && move.to.y == 0)
        {
            GameObject menu = Instantiate(manager.BPromotionMenu, transform);
            manager.Pause();
            //promote(5);
        }
    }
}
