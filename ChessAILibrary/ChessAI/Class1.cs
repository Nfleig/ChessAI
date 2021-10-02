using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class SimpleChess
{
    private int[,] boardArray;
    public Coordinate WKing;
    public Coordinate BKing;
    public Move lastMove;
    public float fitness;
    public bool WCheck;
    public bool BCheck;
    public bool capture = false;
    private List<Coordinate> WhitePieces = new List<Coordinate>();
    private List<Coordinate> BlackPieces = new List<Coordinate>();
    private List<string> DoubleMovedPawns = new List<string>();
    private List<string> pinnedPieces = new List<string>();
    private List<string> moves = new List<string>();
    bool WCastle = true;
    bool BCastle = true;
    public int calcOrder = 0;
    private List<Move> allMoves = new List<Move>();
    private bool isAIControlled = false;

    public struct Coordinate
    {
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Coordinate(Coordinate other)
        {
            this.x = other.x;
            this.y = other.y;
        }
        public int x;
        public int y;

        public bool Equals(Coordinate other)
        {
            return (this.x == other.x && this.y == other.y);
        }
    }
    public string toString(Coordinate coord)
    {
        string coordinate = string.Concat(coord.x, coord.y);
        return coordinate;
    }

    public struct Move
    {
        public Move(Coordinate from, Coordinate to)
        {
            this.from = from;
            this.to = to;
            this.promotion = 0;
        }
        public Move(Coordinate from, Coordinate to, int promotion)
        {
            this.from = from;
            this.to = to;
            this.promotion = promotion;
        }
        public Coordinate from;
        public Coordinate to;
        public int promotion;

        public bool Equals(Move other)
        {
            return (this.from.Equals(other.from) && this.to.Equals(other.to));
        }
    }
    public SimpleChess()
    {
        boardArray = new int[8, 8];
        capture = false;
    }
    public SimpleChess(int[,] otherBoard)
    {
        int[,] newBoard = new int[8, 8];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                int piece = otherBoard[x, y];
                newBoard[x, y] = piece;
                if (piece != 0)
                {
                    if (System.Math.Sign(piece) > 0)
                    {
                        WhitePieces.Add(new Coordinate(x, y));
                    }
                    else
                    {
                        BlackPieces.Add(new Coordinate(x, y));
                    }
                    if (System.Math.Abs(piece) == 6)
                    {
                        if (piece > 0)
                        {
                            WKing = new Coordinate(x, y);
                        }
                        else
                        {
                            BKing = new Coordinate(x, y);
                        }
                    }
                }

            }
        }
        boardArray = newBoard;
        capture = false;
        WCheck = isCheck(1);
        BCheck = isCheck(-1);
    }

    public SimpleChess(SimpleChess otherBoard)
    {
        this.boardArray = otherBoard.boardArray.Clone() as int[,];
        this.WhitePieces = new List<Coordinate>(otherBoard.WhitePieces);
        this.BlackPieces = new List<Coordinate>(otherBoard.BlackPieces);
        this.WKing = new Coordinate(otherBoard.WKing);
        this.BKing = new Coordinate(otherBoard.BKing);
        this.WCheck = otherBoard.WCheck;
        this.BCheck = otherBoard.BCheck;
        this.capture = false;
        this.pinnedPieces = getPinnedPieces(System.Math.Sign(boardArray[otherBoard.lastMove.to.x, otherBoard.lastMove.to.y]) * -1);
        this.WCastle = otherBoard.WCastle;
        this.BCastle = otherBoard.BCastle;
        this.isAIControlled = true;
    }

    int[] cosValue = { 1, 0, -1, 0, 1, -1, -1, 1 };
    int[] sinValue = { 0, 1, 0, -1, 1, 1, -1, -1 };

    public List<Move> generateMoves(int color)
    {
        List<Coordinate> pieces = BlackPieces;
        List<Move> moves = new List<Move>();
        if (color == 1)
        {
            pieces = WhitePieces;
            //pinnedPieces = getPinnedPieces(1);
        }
        else
        {

            //pinnedPieces = getPinnedPieces(-1);
        }
        foreach (Coordinate location in pieces)
        {
            moves.AddRange(generateMoves(location));
            //Debug.Log(board[location.x, location.y] + " " + moves.Count);
        }
        return moves;
    }
    public List<Move> generateMoves(Coordinate location)
    {
        /**
        if(Piece.turn % 2 == 0)
        {
            pinnedPieces = getPinnedPieces(-1);
        }
        else
        {
            pinnedPieces = getPinnedPieces(1);
        }
        **/
        pinningPieces.Clear();
        pinnedPieces = getPinnedPieces(System.Math.Sign(boardArray[location.x, location.y]));
        List<Move> moves = new List<Move>();
        int piece = testSpace(location);
        if (pinnedPieces.Contains(location.ToString()) && System.Math.Abs(piece) == 3)
        {
            return moves;
        }
        switch (System.Math.Abs(piece))
        {
            case 1:
                if ((location.y < 7 && location.y > 0) && testSpace(location.x, location.y + piece) == 0)
                {
                    moves.Add(new Move(location, new Coordinate(location.x, location.y + piece)));
                    if (((piece == 1 && location.y == 1) || (piece == -1 && location.y == 6)) && (testSpace(location.x, location.y + (2 * piece)) == 0))
                    {
                        moves.Add(new Move(location, new Coordinate(location.x, location.y + (2 * piece))));
                    }
                }
                if (testSpace(location.x - 1, location.y + piece) * piece < 0)
                {
                    moves.Add(new Move(location, new Coordinate(location.x - 1, location.y + piece)));
                }
                if (testSpace(location.x + 1, location.y + piece) * piece < 0)
                {
                    moves.Add(new Move(location, new Coordinate(location.x + 1, location.y + piece)));
                }
                if (testSpace(location.x - 1, location.y) * piece < 0)
                {
                    if (DoubleMovedPawns.Contains(toString(new Coordinate(location.x - 1, location.y))))
                    {
                        moves.Add(new Move(location, new Coordinate(location.x - 1, location.y + piece)));
                    }
                }
                if (testSpace(location.x + 1, location.y) * piece < 0)
                {
                    if (DoubleMovedPawns.Contains(toString(new Coordinate(location.x + 1, location.y))))
                    {
                        moves.Add(new Move(location, new Coordinate(location.x + 1, location.y + piece)));
                    }
                }
                List<Move> oldMoves = new List<Move>();
                List<Move> newMoves = new List<Move>();
                foreach (Move move in moves)
                {
                    if ((piece == 1 && move.to.y == 7) || (piece == -1 && location.y == 0))
                    {

                        if (isAIControlled)
                        {
                            oldMoves.Add(move);
                            for (int i = 2; i < 6; i++)
                            {
                                newMoves.Add(new Move(location, move.to, i * piece));
                            }
                        }
                    }
                }
                foreach (Move move in oldMoves)
                {
                    moves.Remove(move);
                }
                foreach (Move move in newMoves)
                {
                    moves.Add(move);
                }
                break;
            case 2:
                for (int i = 0; i < 4; i++)
                {
                    int hit = 0;
                    int c = 1;
                    int cos = cosValue[i];
                    int sin = sinValue[i];
                    while (location.x + (c * cos) > -1 && location.x + (c * cos) < 8 && location.y + (c * sin) > -1 && location.y + (c * sin) < 8 && hit == 0)
                    {
                        Coordinate potentialMove = new Coordinate(location.x + (c * cos), location.y + (c * sin));
                        hit = testSpace(potentialMove);
                        if (hit * piece <= 0)
                        {
                            moves.Add(new Move(location, potentialMove));
                        }
                        c++;
                    }
                }
                break;

            case 3:
                for (int i = 0; i < 4; i++)
                {
                    int cos = cosValue[i + 4];
                    int sin = sinValue[i + 4];
                    if (location.x + cos > -1 && location.x + cos < 8 && location.y + (2 * sin) > -1 && location.y + (2 * sin) < 8)
                    {
                        Coordinate potentialMove = new Coordinate(location.x + cos, location.y + (2 * sin));
                        if (testSpace(potentialMove) * piece <= 0)
                        {
                            moves.Add(new Move(location, potentialMove));
                        }
                    }
                    if (location.x + (2 * cos) > 0 && location.x + (2 * cos) < 8 && location.y + sin > 0 && location.y + sin < 8)
                    {
                        Coordinate potentialMove = new Coordinate(location.x + (2 * cos), location.y + sin);
                        if (testSpace(potentialMove) * piece <= 0)
                        {
                            moves.Add(new Move(location, potentialMove));
                        }
                    }
                }
                break;
            case 4:
                for (int i = 0; i < 4; i++)
                {
                    int hit = 0;
                    int c = 1;
                    int cos = cosValue[i + 4];
                    int sin = sinValue[i + 4];
                    while (location.x + (c * cos) > -1 && location.x + (c * cos) < 8 && location.y + (c * sin) > -1 && location.y + (c * sin) < 8 && hit == 0)
                    {
                        Coordinate potentialMove = new Coordinate(location.x + (c * cos), location.y + (c * sin));
                        hit = testSpace(potentialMove);
                        if (hit * piece <= 0)
                        {
                            moves.Add(new Move(location, potentialMove));
                        }
                        c++;
                    }
                }
                break;
            case 5:
                for (int i = 0; i < 8; i++)
                {
                    int hit = 0;
                    int c = 1;
                    int cos = cosValue[i];
                    int sin = sinValue[i];
                    while (location.x + (c * cos) > -1 && location.x + (c * cos) < 8 && location.y + (c * sin) > -1 && location.y + (c * sin) < 8 && hit == 0)
                    {
                        Coordinate potentialMove = new Coordinate(location.x + (c * cos), location.y + (c * sin));
                        hit = testSpace(potentialMove);
                        if (hit * piece <= 0)
                        {
                            moves.Add(new Move(location, potentialMove));
                        }
                        c++;
                    }
                }
                break;
            case 6:
                for (int i = 0; i < 8; i++)
                {
                    int cos = cosValue[i];
                    int sin = sinValue[i];
                    Coordinate potentialMove = new Coordinate(location.x + cos, location.y + sin);
                    if (location.x + cos > -1 && location.x + cos < 8 && location.y + sin > -1 && location.y + sin < 8 && testSpace(potentialMove) * piece <= 0)
                    {
                        moves.Add(new Move(location, potentialMove));

                    }
                }
                int rank = 0;
                if (piece < 0)
                {
                    rank = 7;
                }
                if ((piece > 0 && WCastle) || (piece < 0 && BCastle))
                {
                    if (boardArray[5, rank] == 0 && boardArray[6, rank] == 0 && boardArray[7, rank] == 2 * System.Math.Sign(piece))
                    {
                        if (((piece > 0 && !WCheck) || (piece < 0 && !BCheck)) && !isThreatened(new Coordinate(5, rank), -System.Math.Sign(piece)))
                        {
                            moves.Add(new Move(location, new Coordinate(location.x + 2, location.y)));
                        }
                    }
                    if (boardArray[3, rank] == 0 && boardArray[2, rank] == 0 && boardArray[1, rank] == 0 && boardArray[0, rank] == 2 * System.Math.Sign(piece))
                    {
                        if (((piece > 0 && !WCheck) || (piece < 0 && !BCheck)) && !isThreatened(new Coordinate(3, rank), -System.Math.Sign(piece)))
                        {
                            moves.Add(new Move(location, new Coordinate(location.x - 2, location.y)));
                        }
                    }
                }
                break;
        }
        string loc = toString(location);
        if (pinnedPieces.Contains(loc))
        {
            List<Move> illegalMoves = new List<Move>();
            foreach (Coordinate pin in pinningPieces)
            {
                if ((location.x - pin.x == 0 || location.y - pin.y == 0) && (System.Math.Abs(testSpace(pin)) == 2 || System.Math.Abs(testSpace(pin)) == 5))
                {
                    foreach (Move move in moves)
                    {
                        if (!((move.to.x - pin.x == 0 && location.x - pin.x == 0) || (move.to.y - pin.y == 0 && location.y - pin.y == 0)))
                        {
                            illegalMoves.Add(move);
                        }
                    }
                }
                if (!(location.x == pin.x) && (System.Math.Abs((float)(location.y - pin.y) / (float)(location.x - pin.x)) == 1f) && (System.Math.Abs(testSpace(pin)) == 4 || System.Math.Abs(testSpace(pin)) == 5))
                {
                    float angle = (float)(location.y - pin.y) / (float)(location.x - pin.x);
                    foreach (Move move in moves)
                    {
                        bool stop = move.to.x == pin.x;
                        if (stop && !(move.to.y == pin.y))
                        {
                            illegalMoves.Add(move);
                        }
                        if (!stop && !(((float)(move.to.y - pin.y) / (float)(move.to.x - pin.x)) == angle))
                        {
                            illegalMoves.Add(move);
                        }
                    }
                }
            }
            foreach (Move move in illegalMoves)
            {
                moves.Remove(move);
            }
        }
        if (System.Math.Abs(piece) == 6 || (piece > 0 && WCheck) || (piece < 0 && BCheck))
        {
            //Debug.Log("Binding Moves");
            bindMoves(moves);
            //specialCase = false;
        }
        //Debug.Log(piece + ": " + moves.Count);
        return moves;
    }
    public void bindMoves(List<Move> moves)
    {
        List<Move> illegalMoves = new List<Move>();
        foreach (Move move in moves)
        {
            SimpleChess newBoard = new SimpleChess(this);
            int piece = boardArray[move.from.x, move.from.y];
            newBoard.movePiece(move);
            //Debug.Log(WKing.x + " " + WKing.y);
            if ((piece > 0 && newBoard.WCheck) || (piece < 0 && newBoard.BCheck))
            {
                illegalMoves.Add(move);
            }
        }
        foreach (Move move in illegalMoves)
        {
            moves.Remove(move);
        }
    }
    /*
    public void movePiece(Move move)
    {
        movePiece(move.from, move.to);
        lastMove = move;
        allMoves.Add(move);
    }
    */
    public void movePiece(Move move)
    {
        lastMove = move;
        allMoves.Add(move);
        int x1 = move.from.x;
        int y1 = move.from.y;
        int x2 = move.to.x;
        int y2 = move.to.y;
        capture = false;
        if (boardArray[x2, y2] != 0)
        {
            capture = true;
            WhitePieces.Remove(move.to);
            BlackPieces.Remove(move.to);
        }

        boardArray[x2, y2] = boardArray[x1, y1];
        boardArray[x1, y1] = 0;
        int endPiece = boardArray[x2, y2];
        if (endPiece > 0)
        {
            WhitePieces.Remove(move.from);
            WhitePieces.Add(move.to);
        }
        else
        {
            BlackPieces.Remove(move.from);
            BlackPieces.Add(move.to);
        }
        if (System.Math.Abs(endPiece) == 1)
        {
            Coordinate passent = new Coordinate(move.to.x, move.to.y - endPiece);
            if (DoubleMovedPawns.Contains(toString(passent)))
            {
                if (testSpace(passent) > 0)
                {
                    WhitePieces.Remove(passent);
                    capture = true;
                }
                else if (testSpace(passent) < 0)
                {
                    BlackPieces.Remove(passent);
                    capture = true;
                }
                boardArray[move.to.x, move.to.y - endPiece] = 0;
            }
            DoubleMovedPawns.Clear();
            if (System.Math.Abs(move.to.y - move.from.y) == 2)
            {
                DoubleMovedPawns.Add(toString(move.to));
            }
            if (move.promotion != 0)
            {
                boardArray[x2, y2] = move.promotion;
            }
        }
        if (System.Math.Abs(endPiece) == 6)
        {
            if (move.from.x - move.to.x == 2)
            {
                if (boardArray[0, move.to.y] == 2 * System.Math.Sign(endPiece))
                {
                    movePiece(new Move(new Coordinate(0, move.to.y), new Coordinate(3, move.to.y)));
                }
            }
            else if (move.from.x - move.to.x == -2)
            {
                if (boardArray[7, move.to.y] == 2 * System.Math.Sign(endPiece))
                {
                    movePiece(new Move(new Coordinate(7, move.to.y), new Coordinate(5, move.to.y)));
                }
            }
            if (endPiece < 0)
            {
                BKing = move.to;
                BCastle = false;
            }
            else
            {
                WKing = move.to;
                WCastle = false;
            }
        }
        //pinnedPieces = getPinnedPieces(endPiece / System.Math.Abs(endPiece));
        if (pinnedPieces.Count == 0)
        {
            pinnedPieces = getPinnedPieces(System.Math.Sign(endPiece));
        }
        if (endPiece > 0 || BCheck || pinnedPieces.Contains(toString(move.to)) || pinnedPieces.Contains(toString(move.from)) || endPiece == -6)
        {
            BCheck = isCheck(-1);
        }
        if (endPiece < 0 || WCheck || pinnedPieces.Contains(toString(move.to)) || pinnedPieces.Contains(toString(move.from)) || endPiece == 6)
        {
            WCheck = isCheck(1);
        }
        //moves.Add(DeepGold.toChessNotation(this));
    }
    public void promotePiece(Coordinate location, int promotion)
    {
        boardArray[location.x, location.y] = promotion;
        updateCheck();
        lastMove.promotion = promotion;
    }
    public int testSpace(Coordinate space)
    {
        return testSpace(space.x, space.y);
    }
    public int testSpace(int x, int y)
    {
        if (x > -1 && x < 8 && y > -1 && y < 8)
        {
            return boardArray[x, y];
        }
        else
        {
            return 0;
        }
    }
    public int getPiece(Coordinate space)
    {
        return boardArray[space.x, space.y];
    }
    public int getPiece(int x, int y)
    {
        return boardArray[x, y];
    }
    public void updateCheck()
    {
        WCheck = isCheck(1);
        BCheck = isCheck(-1);
    }
    public bool isCheck(int color)
    {
        if (color > 0)
        {
            return isThreatened(WKing, -1);
        }
        else
        {
            //Debug.Log(WKing.x + " " + WKing.y);
            return isThreatened(BKing, 1);
        }
    }
    public bool isDraw()
    {
        if (allMoves.Count <= 10)
        {
            return false;
        }
        if (allMoves[allMoves.Count - 1].Equals(allMoves[allMoves.Count - 5]) && allMoves[allMoves.Count - 1].Equals(allMoves[allMoves.Count - 9]))
        {
            if (allMoves[allMoves.Count - 2].Equals(allMoves[allMoves.Count - 6]) && allMoves[allMoves.Count - 2].Equals(allMoves[allMoves.Count - 10]))
            {
                return true;
            }
        }
        return false;
    }
    public List<Coordinate> pinningPieces = new List<Coordinate>();
    public List<string> getPinnedPieces(int color)
    {
        Coordinate king;
        List<string> pieces = new List<string>();
        if (color > 0)
        {
            king = WKing;
        }
        else
        {
            king = BKing;
        }
        //pieces.Add(toString(king));
        for (int i = 0; i < 8; i++)
        {
            int hit1 = 0;
            int hit2 = 0;
            int c = 1;
            int cos = cosValue[i];
            int sin = sinValue[i];
            Coordinate potentialPiece = new Coordinate();
            Coordinate otherPiece = new Coordinate();
            while (king.x + (c * cos) > -1 && king.x + (c * cos) < 8 && king.y + (c * sin) > -1 && king.y + (c * sin) < 8)
            {
                int generalHit = testSpace(king.x + (c * cos), king.y + (c * sin));
                if (generalHit * color > 0)
                {
                    //pieces.Add(toString(new Coordinate(king.x + (c * cos), king.y + (c * sin))));
                    if (hit1 != 0)
                    {
                        break;
                    }
                    else
                    {
                        hit1 = generalHit;
                        potentialPiece = new Coordinate(king.x + (c * cos), king.y + (c * sin));
                    }
                }
                else if (generalHit * color < 0)
                {
                    if ((i < 4 && (System.Math.Abs(generalHit) == 2 || System.Math.Abs(generalHit) == 5)) || (i >= 4 && (System.Math.Abs(generalHit) == 4 || System.Math.Abs(generalHit) == 5)))
                    {
                        hit2 = generalHit;
                        otherPiece = new Coordinate(king.x + (c * cos), king.y + (c * sin));

                    }
                }
                c++;
            }
            if (hit1 != 0 && hit2 != 0)
            {
                pieces.Add(toString(potentialPiece));
                pinningPieces.Add(otherPiece);
            }
        }
        return pieces;
    }
    public bool isThreatened(Coordinate location, int color)
    {
        /*
        int x = location.x;
        int y = location.y;
        int piece = boardArray[location.x, location.y];

        for (int i = 0; i < 8; i++)
        {
            int hit = 0;
            int c = 1;
            int cos = cosValue[i];
            int sin = sinValue[i];
            while (x + (c * cos) > -1 && x + (c * cos) < 8 && y + (c * sin) > -1 && y + (c * sin) < 8 && hit == 0)
            {
                hit = testSpace(x + (c * cos), y + (c * sin));
                if (hit * color > 0)
                {
                    if (i < 4 && (hit * color == 2 || hit * color == 5))
                    {
                        //Debug.Log(0);
                        return true;
                    }
                    else if (i >= 4 && (hit * color == 4 || hit * color == 5))
                    {
                        //Debug.Log(1);
                        return true;
                    }
                }
                c++;
            }
        }
        int[] hits = { 0, 0, 0, 0, 0, 0, 0, 0 };
        hits[0] = testSpace(x + 1, y + 2);
        hits[1] = testSpace(x - 1, y + 2);
        hits[2] = testSpace(x + 1, y - 2);
        hits[3] = testSpace(x - 1, y - 2);
        hits[4] = testSpace(x + 2, y + 1);
        hits[5] = testSpace(x + 2, y - 1);
        hits[6] = testSpace(x - 2, y + 1);
        hits[7] = testSpace(x - 2, y - 1);

        for (int i = 0; i < 8; i++)
        {
            if (hits[i] * color == 3)
            {
                //Debug.Log("Knight");
                //Debug.Log(2);
                return true;
            }
        }
        if (color < 0)
        {
            hits[0] = testSpace(x - 1, y + 1);
            hits[1] = testSpace(x + 1, y + 1);
            if (hits[0] == -1 || hits[1] == -1)
            {
                //Debug.Log(3);
                return true;
            }
        }
        else
        {
            hits[0] = testSpace(x - 1, y - 1);
            hits[1] = testSpace(x + 1, y - 1);
            if (hits[0] == 1 || hits[1] == 1)
            {
                //Debug.Log(4);
                return true;
            }
        }
        if ((color < 0 && (System.Math.Abs(BKing.x - x) < 2 && System.Math.Abs(BKing.y - y) < 2) || (color > 0 && (System.Math.Abs(WKing.x - x) < 2 && System.Math.Abs(WKing.y - y) < 2))))
        {
            //Debug.Log(WKing.x + " " + WKing.y + " " + x + " " + y);
            return true;
        }
        return false;
        */
        List<Coordinate> pieces = getThreateningPieces(location, color);
        if (pieces.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<Coordinate> getThreateningPieces(Coordinate location, int color)
    {
        List<Coordinate> pieces = new List<Coordinate>();
        int x = location.x;
        int y = location.y;
        int piece = boardArray[location.x, location.y];

        for (int i = 0; i < 8; i++)
        {
            int hit = 0;
            int c = 1;
            int cos = cosValue[i];
            int sin = sinValue[i];
            while (x + (c * cos) > -1 && x + (c * cos) < 8 && y + (c * sin) > -1 && y + (c * sin) < 8 && hit == 0)
            {
                hit = testSpace(x + (c * cos), y + (c * sin));
                if (hit * color > 0)
                {
                    if (i < 4 && (hit * color == 2 || hit * color == 5))
                    {
                        //Debug.Log(0);
                        pieces.Add(new Coordinate(x + (c * cos), y + (c * sin)));
                        break;
                    }
                    else if (i >= 4 && (hit * color == 4 || hit * color == 5))
                    {
                        //Debug.Log(1);
                        pieces.Add(new Coordinate(x + (c * cos), y + (c * sin)));
                        break;
                    }
                }
                c++;
            }
        }
        Coordinate[] hits = new Coordinate[8];
        hits[0] = new Coordinate(x + 1, y + 2);
        hits[1] = new Coordinate(x - 1, y + 2);
        hits[2] = new Coordinate(x + 1, y - 2);
        hits[3] = new Coordinate(x - 1, y - 2);
        hits[4] = new Coordinate(x + 2, y + 1);
        hits[5] = new Coordinate(x + 2, y - 1);
        hits[6] = new Coordinate(x - 2, y + 1);
        hits[7] = new Coordinate(x - 2, y - 1);

        for (int i = 0; i < 8; i++)
        {
            if (testSpace(hits[i]) * color == 3)
            {
                //Debug.Log("Knight");
                //Debug.Log(2);
                pieces.Add(hits[i]);
            }
        }
        if (color < 0)
        {
            if (testSpace(x - 1, y + 1) == -1)
            {
                pieces.Add(new Coordinate(x - 1, y + 1));
            }
            if (testSpace(x + 1, y + 1) == -1)
            {
                pieces.Add(new Coordinate(x + 1, y + 1));
            }
            /*
            hits[0] = new Coordinate(x - 1, y + 1);
            hits[1] = new Coordinate(x + 1, y + 1);
            if (hits[0] == -1 || hits[1] == -1)
            {
                //Debug.Log(3);
                return true;
            }
            */
        }
        else
        {
            if (testSpace(x - 1, y - 1) == 1)
            {
                pieces.Add(new Coordinate(x - 1, y - 1));
            }
            if (testSpace(x + 1, y - 1) == 1)
            {
                pieces.Add(new Coordinate(x + 1, y - 1));
            }
        }
        if (color < 0 && System.Math.Abs(BKing.x - x) < 2 && System.Math.Abs(BKing.y - y) < 2)
        {
            pieces.Add(BKing);
        }
        if (color > 0 && System.Math.Abs(WKing.x - x) < 2 && System.Math.Abs(WKing.y - y) < 2)
        {
            pieces.Add(WKing);

        }

        return pieces;
    }
    public string toString()
    {
        string boardString = "";
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                boardString = string.Concat(boardString, this.boardArray[x, y]);
            }
        }
        return boardString;
    }
    public List<Coordinate> getPieces(int color)
    {
        if (color == 1)
        {
            return WhitePieces;
        }
        else if (color == -1)
        {
            return BlackPieces;
        }
        else
        {
            return null;
        }
    }

    private static string[] xNotation = { "a", "b", "c", "d", "e", "f", "g", "h" };
    private static string[] pieceNotation = { "\u265F", "\u265C", "\u265E", "\u265D", "\u265B", "\u265A", "\u2659", "\u2656", "\u2658", "\u2657", "\u2655", "\u2654" };
    private static string[] letterNotation = { "P", "R", "N", "B", "Q", "K" };

    public static string toChessNotation(SimpleChess board, bool displayLetters)
    {
        Move lastMove = board.lastMove;
        int piece = board.getPiece(board.lastMove.to);
        string move = "";
        if (board.lastMove.promotion != 0)
        {
            if (board.capture)
            {
                move = xNotation[board.lastMove.from.x] + "x" + xNotation[board.lastMove.to.x] + (board.lastMove.to.y + 1);
            }
            else
            {
                move = xNotation[board.lastMove.to.x] + (board.lastMove.to.y + 1);
            }
            move += "=";
            if (piece > 1)
            {
                // move += piece;
                //return "Piece at " + board.lastMove.to.x.ToString() + " " + board.lastMove.to.y.ToString() + ": " + piece;
                move += pieceNotation[System.Math.Abs(piece) + 5];
            }
            else
            {
                //return "Piece at " + board.lastMove.to.x.ToString() + " " + board.lastMove.to.y.ToString() + ": " + piece;
                // move += piece;
                move += pieceNotation[System.Math.Abs(piece) - 1];
            }
            if (board.BCheck || board.WCheck)
            {
                if (board.BCheck && board.generateMoves(-1).Count == 0)
                {
                    move += "#";
                }
                else if (board.WCheck && board.generateMoves(1).Count == 0)
                {
                    move += "#";
                }
                else
                {
                    move += "+";
                }
            }
            return move;
        }
        if (System.Math.Abs(piece) == 6)
        {
            if (lastMove.from.x - lastMove.to.x == 2)
            {
                return "O-O-O";
            }
            if (lastMove.from.x - lastMove.to.x == -2)
            {
                return "O-O";
            }
        }
        if (piece > 1)
        {
            move = displayLetters ? letterNotation[piece - 1] : pieceNotation[piece + 5];
        }
        else if (piece < -1)
        {
            move = displayLetters ? letterNotation[System.Math.Abs(piece) - 1] : pieceNotation[System.Math.Abs(piece) - 1];
        }
        board.boardArray[board.lastMove.from.x, board.lastMove.from.y] = 99;
        List<Coordinate> otherPieces = board.getThreateningPieces(board.lastMove.to, System.Math.Sign(piece));
        board.boardArray[board.lastMove.from.x, board.lastMove.from.y] = 0;
        if (System.Math.Abs(piece) != 1 && System.Math.Abs(piece) != 6)
        {
            foreach (Coordinate other in otherPieces)
            {
                if (board.getPiece(other) == piece && !other.Equals(board.lastMove))
                {
                    if (other.x != board.lastMove.from.x)
                    {
                        move += xNotation[board.lastMove.from.x];
                    }
                    else
                    {
                        move += (board.lastMove.from.y + 1);
                    }
                }
            }
        }

        if (board.capture)
        {
            if (System.Math.Abs(piece) == 1)
            {
                move += xNotation[lastMove.from.x];
            }
            move += "x";
        }
        move += xNotation[lastMove.to.x] + (lastMove.to.y + 1);
        if (board.BCheck || board.WCheck)
        {
            if (board.BCheck && board.generateMoves(-1).Count == 0)
            {
                move += "#";
            }
            else if (board.WCheck && board.generateMoves(1).Count == 0)
            {
                move += "#";
            }
            else
            {
                move += "+";
            }
        }
        return move;
    }
}
public class AIState
{
    public SimpleChess board;
    public List<string> transpositionTable;
    public List<ChessAI.MTDNode> nodes;
    public float currentGuess;
    public int depth;

    public AIState(SimpleChess board, float currentGuess)
    {
        this.board = board;
        this.transpositionTable = new List<string>();
        this.nodes = new List<ChessAI.MTDNode>();
        this.currentGuess = currentGuess;
        this.depth = 1;
    }

}
public class ChessAI
{
    public struct MTDNode
    {
        public MTDNode(SimpleChess board, string boardString)
        {
            this.board = board;
            this.boardString = boardString;
            this.upperBound = 0;
            this.lowerBound = 0;
            this.hasUpper = false;
            this.hasLower = false;
        }
        public string boardString;
        public SimpleChess board;
        public bool hasUpper;
        public bool hasLower;
        public float upperBound;
        public float lowerBound;
    }


    public ChessAI(int simulatedTurns, int color, float grainSize, float PieceWeight, float CenterWeight, float DevelopmentWeight, float PressureWeight, float KingWeight, float PawnWeight)
    {
        this.simulatedTurns = simulatedTurns;
        this.color = color;
        this.grainSize = grainSize;
        this.PieceWeight = PieceWeight;
        this.CenterWeight = CenterWeight;
        this.DevelopmentWeight = DevelopmentWeight;
        this.PressureWeight = PressureWeight;
        this.KingWeight = KingWeight;
        this.PawnWeight = PawnWeight;
    }

    public int simulatedTurns;
    public int color;
    public float grainSize;
    int nodeCount;
    public void IterativeDeepening(SimpleChess board, CancellationToken token)
    {
        float firstGuess = CalculateFitness(board);
        List<string> transpositionTable = new List<string>();
        List<MTDNode> nodes = new List<MTDNode>();
        for (int d = 1; d < simulatedTurns; d++)
        {
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }
            firstGuess = MTD(board, firstGuess, d, transpositionTable, nodes);
        }
        board.fitness = firstGuess;
    }
    public void IterativeDeepening(AIState state, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
        }
        state.currentGuess = MTD(state.board, state.currentGuess, state.depth, state.transpositionTable, state.nodes);
        state.board.fitness = state.currentGuess;
        state.depth++;
    }
    public void IterativeDeepening(SimpleChess board)
    {
        float firstGuess = CalculateFitness(board);
        List<string> transpositionTable = new List<string>();
        List<MTDNode> nodes = new List<MTDNode>();
        for (int d = 1; d < simulatedTurns; d++)
        {
            firstGuess = MTD(board, firstGuess, d, transpositionTable, nodes);
        }
        board.fitness = firstGuess;
    }
    int searches = 0;
    float MTD(SimpleChess board, float firstGuess, int depth, List<string> transpositionTable, List<MTDNode> nodes)
    {
        float fitness = firstGuess;
        float upperBound = float.PositiveInfinity;
        float lowerBound = float.NegativeInfinity;
        float beta;
        while (!(lowerBound >= upperBound) && System.Math.Abs(fitness) != float.PositiveInfinity)
        {
            if (fitness == lowerBound)
            {
                beta = fitness + grainSize;
            }
            else
            {
                beta = fitness;
            }
            fitness = AlphaBeta(board, false, beta - grainSize, beta, depth, transpositionTable, nodes);
            Interlocked.Increment(ref nodeCount);
            Interlocked.Increment(ref searches);
            if (fitness < beta)
            {
                upperBound = fitness;
            }
            else
            {
                lowerBound = fitness;
            }
        }
        return fitness;
    }
    int TTU;

    float AlphaBeta(SimpleChess board, bool ourTurn, float alpha, float beta, int depth, List<string> transpositionTable, List<MTDNode> nodes)
    {
        string boardString = board.toString();
        int index = transpositionTable.IndexOf(boardString);
        if (index != -1)
        {
            Interlocked.Increment(ref TTU);
            MTDNode node = nodes[index];
            if (node.hasLower && node.lowerBound >= beta)
            {

                return node.lowerBound;
            }
            if (node.hasUpper && node.upperBound <= alpha)
            {
                return node.upperBound;
            }

            if (node.hasLower)
            {
                alpha = System.Math.Max(node.lowerBound, alpha);
            }
            if (node.hasUpper)
            {
                beta = System.Math.Min(node.upperBound, beta);
            }
        }
        if (depth <= 0)
        {
            return CalculateFitness(board);
        }
        else
        {
            float fitness;
            if (ourTurn)
            {
                if (board.isDraw())
                {
                    return 0;
                }
                List<SimpleChess> newMoves = generateMoves(board, color);
                if (newMoves.Count == 0)
                {
                    if ((color < 0 && board.BCheck) || (color > 0 && board.WCheck))
                    {
                        return -100000;
                    }
                    else
                    {
                        return 0;
                    }
                }
                assignMoveOrder(newMoves);
                //newMoves.Sort(MoveOrder);
                fitness = float.NegativeInfinity;
                float tempAlpha = alpha;
                foreach (SimpleChess move in newMoves)
                {
                    if (fitness >= beta)
                    {
                        break;
                    }
                    else
                    {
                        fitness = System.Math.Max(fitness, AlphaBeta(move, false, tempAlpha, beta, depth - 1, transpositionTable, nodes));
                        Interlocked.Increment(ref nodeCount);
                        tempAlpha = System.Math.Max(tempAlpha, fitness);
                    }
                }
            }
            else
            {
                if (board.isDraw())
                {
                    return 0;
                }
                List<SimpleChess> newMoves = generateMoves(board, -color);
                if (newMoves.Count == 0)
                {
                    if ((color < 0 && board.WCheck) || (color > 0 && board.BCheck))
                    {
                        return 100000;
                    }
                    else
                    {
                        return 0;
                    }
                }
                assignMoveOrder(newMoves);
                //newMoves.Sort(MoveOrder);
                fitness = float.PositiveInfinity;
                float tempBeta = beta;
                foreach (SimpleChess move in newMoves)
                {
                    if (fitness <= alpha)
                    {
                        break;
                    }
                    else
                    {
                        fitness = System.Math.Min(fitness, AlphaBeta(move, true, alpha, tempBeta, depth - 1, transpositionTable, nodes));
                        Interlocked.Increment(ref nodeCount);
                        tempBeta = System.Math.Min(fitness, tempBeta);
                    }
                }
            }
            MTDNode node = new MTDNode(board, boardString);
            if (fitness <= alpha)
            {
                node.upperBound = fitness;
                node.hasUpper = true;
            }
            if (fitness > alpha && fitness < beta)
            {
                node.lowerBound = fitness;
                node.upperBound = fitness;
                node.hasUpper = true;
                node.hasLower = true;
            }
            if (fitness >= beta)
            {
                node.lowerBound = fitness;
                node.hasLower = true;
            }
            if (index > 0)
            {
                nodes[index] = node;
            }
            else
            {
                transpositionTable.Add(boardString);
                nodes.Add(node);
            }
            return fitness;
        }
    }
    public float PieceWeight;
    public float CenterWeight;
    public float DevelopmentWeight;
    public float PressureWeight;
    public float KingWeight;
    public float PawnWeight;
    public static int[] pieceValue = { 0, 1, 3, 4, 4, 7, 10 };

    public float PawnAdvancementWeight;

    //Fitness algorithm based on chess board evaluation guide at https://chessfox.com/example-of-the-complete-evaluation-process-of-chess-a-chess-position/

    float CalculateEndgameFitness(SimpleChess board)
    {
        float fitness = 0;

        List<SimpleChess.Coordinate> newBoard = new List<SimpleChess.Coordinate>(board.getPieces(1));
        newBoard.AddRange(board.getPieces(-1));
        foreach (SimpleChess.Coordinate location in newBoard)
        {
            int piece = board.getPiece(location);
            if (System.Math.Abs(piece) == 1)
            {
                if (System.Math.Sign(piece) == color)
                {
                    if (color == 1)
                    {
                        fitness += location.y * PawnAdvancementWeight;
                    }
                    else
                    {
                        fitness += (7 - location.y) * PawnAdvancementWeight;
                    }
                }
                else
                {
                    if (color == 1)
                    {
                        fitness -= location.y * PawnAdvancementWeight;
                    }
                    else
                    {
                        fitness -= (7 - location.y) * PawnAdvancementWeight;
                    }
                }
            }

        }

        return fitness;
    }

    public float CalculateFitness(SimpleChess board)
    {
        float fitness = 0;
        List<SimpleChess.Coordinate> newBoard = new List<SimpleChess.Coordinate>(board.getPieces(1));
        newBoard.AddRange(board.getPieces(-1));
        foreach (SimpleChess.Coordinate location in newBoard)
        {
            int piece = board.testSpace(location);
            if (System.Math.Abs(piece) > 6 && piece != 0)
            {
                continue;
            }
            if (System.Math.Abs(piece) == 1)
            {
                int pawnScore = 0;
                if (board.isThreatened(location, color))
                {
                    pawnScore += 2;
                }
                if (board.testSpace(location.x + 1, location.y + piece) * piece < 0 || board.testSpace(location.x - 1, location.y + piece) * piece < 0)
                {
                    pawnScore += 3;
                }
                fitness += pawnScore * (PawnWeight * color * piece);
            }
            else if (System.Math.Abs(piece) == 6)
            {
                int protectionScore = 0;
                if (location.x < 3)
                {
                    protectionScore = 3 - location.x;
                }
                else if (location.x > 4)
                {
                    protectionScore = location.x - 4;
                }
                else
                {
                    protectionScore = 0;
                }
                if (location.y < 3)
                {
                    protectionScore = 3 - location.y;
                }
                else if (location.y > 4)
                {
                    protectionScore = location.y - 4;
                }
                else
                {
                    protectionScore = 0;
                }
                fitness += protectionScore * (1 / 6) * KingWeight;
            }
            else
            {
                List<SimpleChess.Move> moves = board.generateMoves(location);
                int moveCount = moves.Count;
                foreach (SimpleChess.Move move in moves)
                {
                    int hitPiece = board.testSpace(move.to);
                    int centerIndex = System.Math.Min(System.Math.Abs(4 - move.to.x), System.Math.Abs(3 - move.to.x)) + System.Math.Min(System.Math.Abs(4 - move.to.y), System.Math.Abs(3 - move.to.y));
                    if (piece * color > 0)
                    {
                        fitness += pieceValue[System.Math.Abs(hitPiece)] * PressureWeight;
                        if (centerIndex <= 2)
                        {
                            fitness += (3 - centerIndex) * CenterWeight * (1 / 3);
                        }
                    }
                    else
                    {
                        fitness -= pieceValue[System.Math.Abs(hitPiece)] * PressureWeight;
                        if (centerIndex <= 2)
                        {
                            fitness -= (3 - centerIndex) * CenterWeight * (1 / 3);
                        }
                    }
                }
                if (piece * color > 0)
                {
                    fitness += pieceValue[System.Math.Abs(piece)] * PieceWeight / 47f;
                    fitness += moveCount * DevelopmentWeight;

                }
                else
                {
                    fitness -= pieceValue[System.Math.Abs(piece)] * PieceWeight / 47;
                    fitness -= moveCount * DevelopmentWeight;
                }
            }

        }
        return fitness;
    }
    List<SimpleChess> generateMoves(SimpleChess board, int color)
    {
        List<SimpleChess> boards = new List<SimpleChess>();
        List<SimpleChess.Move> moves = board.generateMoves(color);
        foreach (SimpleChess.Move move in moves)
        {
            SimpleChess newBoard = new SimpleChess(board);
            newBoard.movePiece(move);
            boards.Add(newBoard);
        }
        return boards;
    }
    public int getNodeCount()
    {
        return nodeCount;
    }
    public int getTTU()
    {
        return TTU;
    }
    public int getSearches()
    {
        return searches;
    }
    public void reset()
    {
        nodeCount = 0;
        TTU = 0;
        searches = 0;
    }
    public void setWeights(float PieceWeight, float CenterWeight, float DevelopmentWeight, float PressureWeight, float KingWeight, float PawnWeight)
    {
        this.PieceWeight = PieceWeight;
        this.CenterWeight = CenterWeight;
        this.DevelopmentWeight = DevelopmentWeight;
        this.PressureWeight = PressureWeight;
        this.KingWeight = KingWeight;
        this.PawnWeight = PawnWeight;
    }

    public void assignMoveOrder(List<SimpleChess> moves)
    {
        foreach (SimpleChess move in moves)
        {
            int order = 0;
            if (move.WCheck || move.BCheck)
            {
                order += 2;
            }
            if (move.capture)
            {
                order++;
            }
            move.calcOrder = order;
        }
        moves.Sort(MoveOrder);
    }
    int MoveOrder(SimpleChess x, SimpleChess y)
    {

        if (x.calcOrder == y.calcOrder)
        {
            return 0;
        }
        if ((x.calcOrder > y.calcOrder))
        {
            return -1;
        }
        return 1;
    }
}

