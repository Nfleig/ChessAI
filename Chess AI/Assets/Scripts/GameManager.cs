using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private List<Piece> whitePieces = new List<Piece>();
    private List<Piece> blackPieces = new List<Piece>();
    private List<Piece> allPieces = new List<Piece>();
    private List<Piece> doubleMovedPawns = new List<Piece>();
    public List<GameObject> Pieces;
    public List<SimpleChess.Move> allMoves = new List<SimpleChess.Move>();
    public DeepGold WAI;
    public DeepGold BAI;
    //private bool WCheck = false;
    //private bool BCheck = false;
    public int turn = 1;
    private bool gameFinished;
    public TextMesh WPreviousMove;
    public TextMesh BPreviousMove;
    public TextMesh console;
    public GameObject BPromotionMenu;
    public GameObject WPromotionMenu;
    public bool displayLetters;
    public bool turnFinished = true;
    private int winner = 0;
    private bool isPaused;
    private static int[,] initBoard =
    {
        {2, 1, 0, 0, 0, 0, -1, -2 },
        {3, 1, 0, 0, 0, 0, -1, -3 },
        {4, 1, 0, 0, 0, 0, -1, -4 },
        {5, 1, 0, 0, 0, 0, -1, -5 },
        {6, 1, 0, 0, 0, 0, -1, -6 },
        {4, 1, 0, 0, 0, 0, -1, -4 },
        {3, 1, 0, 0, 0, 0, -1, -3 },
        {2, 1, 0, 0, 0, 0, -1, -2 }
    };
    public bool manualControl;
    public SimpleChess board;
    // Start is called before the first frame update
    void Awake()
    {
        Restart();
        /**
        DeepGold[] AIObjects = transform.parent.GetComponentsInChildren<DeepGold>();
        foreach(DeepGold obj in AIObjects)
        {
            if(obj.color > 0)
            {
                WAI = obj;
            }
            else
            {
                BAI = obj;
            }
        }
        **/
    }

    // Update is called once per frame
    void Update()
    {
        //tester.text = "Piece at " + board.lastMove.to.x + " " + board.lastMove.to.y + ": " + board.getPiece(board.lastMove.to).ToString();
        //BPreviousMove.text = "" + SimpleChess.toChessNotation(board, displayLetters);
    }
    public void makeMove(SimpleChess.Move move)
    {
        int intPiece = board.getPiece(move.from);
        board.movePiece(move);
        //tester.text = board.getPiece(6, 0).ToString();
        allMoves.Add(move);
        //board.criticalPieces = board.getCriticalPieces(System.Math.Sign(intPiece));
        foreach (Piece piece in allPieces)
        {
            if(piece.x == move.from.x && piece.y == move.from.y)
            {
                //print(piece.type);
                if (Mathf.Abs(intPiece) == 6)
                {
                    if (move.from.x - move.to.x == 2)
                    {
                        makeMove(new SimpleChess.Move(new SimpleChess.Coordinate(0, move.to.y), new SimpleChess.Coordinate(3, move.to.y)));
                        turn--;
                    }
                    else if (move.from.x - move.to.x == -2)
                    {
                        makeMove(new SimpleChess.Move(new SimpleChess.Coordinate(7, move.to.y), new SimpleChess.Coordinate(5, move.to.y)));
                        turn--;
                    }
                }
                piece.x = move.to.x;
                piece.y = move.to.y;
                //piece.showingMoves = false;
                
                if(Mathf.Abs(intPiece) == 1)
                {
                    Piece passent = piece.getPiece(new SimpleChess.Coordinate(move.to.x, move.to.y - intPiece));
                    if (passent && doubleMovedPawns.Contains(passent))
                    {
                        passent.capturePiece(passent);
                    }
                    doubleMovedPawns.Clear();
                    if (Mathf.Abs(move.from.y - move.to.y) == 2)
                    {
                        doubleMovedPawns.Add(piece);
                    }
                    /*
                    if(move.to.y == 7)
                    {
                        Piece newQueen = Instantiate(Pieces[board.getPiece(move.to) - 1], piece.transform).GetComponent<Piece>();
                        newQueen.transform.parent = transform.parent;
                        newQueen.transform.localScale = allPieces[0].transform.localScale;
                        newQueen.manager = this;
                        addPiece(newQueen);
                        removePiece(piece);
                        Destroy(piece.gameObject);
                        newQueen.x = move.to.x;
                        newQueen.y = move.to.y;
                    }
                    if(move.to.y == 0)
                    {
                        Piece newQueen = Instantiate(Pieces[System.Math.Abs(board.getPiece(move.to)) + 5], piece.transform).GetComponent<Piece>();
                        newQueen.transform.parent = transform.parent;
                        newQueen.transform.localScale = allPieces[0].transform.localScale;
                        addPiece(newQueen);
                        removePiece(piece);
                        Destroy(piece.gameObject);
                        newQueen.x = move.to.x;
                        newQueen.y = move.to.y;
                    }
                    */
                }
                else
                {
                    doubleMovedPawns.Clear();
                }
                break;
            }
        }
        if (intPiece < 0)
        {
            BPreviousMove.text = "Previous Move: " + SimpleChess.toChessNotation(board, displayLetters);
        }
        else
        {
            WPreviousMove.text = "Previous Move: " + SimpleChess.toChessNotation(board, displayLetters);
        }
        if (board.generateMoves(-System.Math.Sign(intPiece)).Count == 0)
        {
            if(-System.Math.Sign(intPiece) < 0 && board.BCheck)
            {
                winner = 1;
            }
            else if(-System.Math.Sign(intPiece) > 0 && board.WCheck)
            {
                winner = -1;
            }
            gameFinished = true;
            if (console)
            {
                console.text = "Checkmate";
            }
            //EndGame();
        }
        if(allMoves.Count > 10)
        {
            if(allMoves[allMoves.Count - 1].Equals(allMoves[allMoves.Count - 5]) && allMoves[allMoves.Count - 1].Equals(allMoves[allMoves.Count - 9]))
            {
                if(allMoves[allMoves.Count - 2].Equals(allMoves[allMoves.Count - 6]) && allMoves[allMoves.Count - 2].Equals(allMoves[allMoves.Count - 10]))
                {
                    gameFinished = true;
                    if (console)
                    {
                        console.text = "Draw by repetition";
                    }
                }
            }
        }
        board.updateCheck();
        turn++;
        turnFinished = true;
    }
    public void Toggle(DeepGold ai)
    {
        ai.enabled = !ai.enabled;
        ai.started = false;
    }
    public void ToggleButton(Text button)
    {
        if(button.text.Equals("Manual Control"))
        {
            button.text = "AI Control";
        }
        else
        {
            button.text = "Manual Control";
        }
    }
    public void EndGame()
    {
        gameFinished = true;
        WAI.gameObject.SetActive(false);
        BAI.gameObject.SetActive(false);
    }
    public void Restart()
    {
        List<Piece> pieces = new List<Piece>();
        foreach (Piece piece in allPieces)
        {
            pieces.Add(piece);
        }
        foreach (Piece piece in pieces)
        {
            removePiece(piece);
            Destroy(piece.gameObject);
        }
        allMoves.Clear();
        board = new SimpleChess(initBoard);
        for (int y = 0; y < 8; y++)
        {
            for(int x = 0; x < 8; x++)
            {
                if(initBoard[x,y] != 0)
                {
                    GameObject newPiece;
                    if (initBoard[x, y] < 0)
                    {
                        newPiece = Instantiate(Pieces[Mathf.Abs(initBoard[x, y]) + 5], transform.parent.transform);
                    }
                    else
                    {
                        newPiece = Instantiate(Pieces[initBoard[x, y] - 1], transform.parent.transform);
                    }
                    newPiece.transform.SetParent(transform.parent);
                    Piece tempPiece = newPiece.GetComponent<Piece>();
                    tempPiece.x = x;
                    tempPiece.y = y;
                }
            }
        }
        turn = 1;
        if (manualControl)
        {
            WAI.gameObject.SetActive(true);
            BAI.gameObject.SetActive(true);
        }
        gameFinished = false;
    }
    
    public void promotePiece(int x, int y, int promotion)
    {
        board.promotePiece(new SimpleChess.Coordinate(x, y), promotion);
        isPaused = false;
        foreach(Piece piece in allPieces)
        {
            if(piece.x == x && piece.y == y)
            {
                if(y == 7)
                {
                    Piece newQueen = Instantiate(Pieces[board.getPiece(x, y) - 1], piece.transform).GetComponent<Piece>();
                    newQueen.transform.parent = transform.parent;
                    newQueen.transform.localScale = allPieces[0].transform.localScale;
                    newQueen.manager = this;
                    addPiece(newQueen);
                    removePiece(piece);
                    Destroy(piece.gameObject);
                    newQueen.x = x;
                    newQueen.y = y;
                }
                else
                {
                    Piece newQueen = Instantiate(Pieces[System.Math.Abs(board.getPiece(x, y)) + 5], piece.transform).GetComponent<Piece>();
                    newQueen.transform.parent = transform.parent;
                    newQueen.transform.localScale = allPieces[0].transform.localScale;
                    addPiece(newQueen);
                    removePiece(piece);
                    Destroy(piece.gameObject);
                    newQueen.x = x;
                    newQueen.y = y;
                }

                if (y == 0)
                {
                    BPreviousMove.text = "Previous Move: " + SimpleChess.toChessNotation(board, displayLetters);
                }
                else
                {
                    WPreviousMove.text = "Previous Move: " + SimpleChess.toChessNotation(board, displayLetters);
                }
                break;
            }
        }

    }

    public void addPiece(Piece piece)
    {
        allPieces.Add(piece);
        if(piece.color == Piece.Color.White)
        {
            whitePieces.Add(piece);
        }
        else
        {
            blackPieces.Add(piece);
        }
    }

    public void removePiece(Piece piece)
    {
        allPieces.Remove(piece);
        if (piece.color == Piece.Color.White)
        {
            whitePieces.Remove(piece);
        }
        else
        {
            blackPieces.Remove(piece);
        }
    }
    public void takeTurn()
    {
        turnFinished = false;
        if(turn % 2 == 0)
        {
            BAI.TakeTurn();
        }
        else
        {
            WAI.TakeTurn();
        }    
    }
    public int getWinner()
    {
        return winner;
    }
    public bool isTurnFinished()
    {
        return turnFinished;
    }
    public bool isGameFinished()
    {
        return gameFinished;
    }
    public void Pause()
    {
        isPaused = true;
    }
    public void Unpause()
    {
        isPaused = false;
    }
    public bool getPaused()
    {
        return isPaused;
    }
}
