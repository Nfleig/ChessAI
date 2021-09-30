using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using System;
using static ChessAI;

public class DeepGold : MonoBehaviour
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
    public GameManager manager;
    public int simulatedTurns;
    private SimpleChess currentBoard;
    private bool thinking = false;
    public int color;
    public float turnDelay;
    private float turnTimer;
    

    public List<List<string>> bestLines = new List<List<string>>();
    public float grainSize;
    public TextMesh MoveCounter;
    public TextMesh NodeCounter;
    public TextMesh PreviousMove;
    public ChessAI AI;
    public float PieceWeight;
    public float CenterWeight;
    public float DevelopmentWeight;
    public float PressureWeight;
    public float KingWeight;
    public float PawnWeight;
    private bool displayLetters;

    List<SimpleChess> calcMoves;
    int moveNum = 0;
    public bool started = false;
    public bool randomFirstMove = false;
    public Gene gene;

    void Awake()
    {
        AI = new ChessAI(simulatedTurns, color, grainSize, PieceWeight, CenterWeight, DevelopmentWeight, PressureWeight, KingWeight, PawnWeight);
        turnTimer = turnDelay;
        displayLetters = manager.displayLetters;
    }
    private void OnDisable()
    {
        Stop();
    }
    private void OnEnable()
    {
        StartAI();
    }
    public void StartAI()
    {
        if ((manager.turn % 2 == 0) == (color == -1) && !manager.manualControl)
        {
            TakeTurn();
        }
    }
    public string getLine(List<string> moves)
    {
        int turn = manager.turn;
        int index = 0;
        string line = "";
        print(moves.Count);
        if (moves.Count <= 0)
        {
            return "";
        }
        if (color == -1)
        {
            line = turn + "... " + moves[index] + " ";
            index++;
            turn++;
        }
        while (index < moves.Count)
        {
            string segment = turn + ". " + moves[index];
            if (index + 1 < moves.Count)
            {
                segment += " " + moves[index + 1] + " ";
            }
            line += segment;
            turn++;
            index += 2;
        }
        return line;
    }
    
    void Update()
    {
        if (manager.getPaused())
        {
            return;
        }
        if ((manager.turn % 2 == 0) == (color == -1) && !started && !manager.isGameFinished() && !manager.manualControl)
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer <= 0 && !started)
            {
                TakeTurn();
                turnTimer = turnDelay;
            }
        }
        int failedTasks = 0;
        foreach(Task task in tasks)
        {
            if(task.Status == TaskStatus.Faulted)
            {
                failedTasks++;
                if(moveNum == calcMoves.Count - failedTasks)
                {
                    print(failedTasks + " Tasks failed!");
                    thinking = false;
                }
            }
        }
        if (started && (manager.turn % 2 == 0) == (color == -1))
        {
            if (manager.turn == 1 && randomFirstMove)
            {
                int randIndex = UnityEngine.Random.Range(0, calcMoves.Count - 1);
                //PreviousMove.text = "Previous move: " + toChessNotation(calcMoves[randIndex]);
                manager.makeMove(calcMoves[randIndex].lastMove);
                MoveCounter.text = "Played random move";
                started = false;
                return;
            }
            if (thinking)
            {
                MoveCounter.text = "Calculating move " + (moveNum + 1) + " / " + (calcMoves.Count) + ":\n" + SimpleChess.toChessNotation(calcMoves[moveNum], displayLetters);
                NodeCounter.text = AI.getNodeCount() + " nodes calculated";
                return;
            }
            //print("Calculated move " + moveNum + " of " + calcMoves.Count + " in " + (float)((timer.ElapsedMilliseconds - previousTime) / 1000) + "s: " + toChessNotation(calcMoves[moveNum - 1], displayLetters));
            if (!thinking)
            {
                started = false;
                moveNum = 0;
                calcMoves.Sort(CompareFitnesses);
                if (calcMoves.Count > 2)
                {
                    //print("1st place: " + toChessNotation(calcMoves[0], displayLetters) + " Fitness: " + calcMoves[0].fitness + "\n2nd place: " + toChessNotation(calcMoves[1], displayLetters) + " Fitness: " + calcMoves[1].fitness + "\n3rd place: " + toChessNotation(calcMoves[2], displayLetters) + " Fitness: " + calcMoves[2].fitness);

                }
                manager.makeMove(calcMoves[0].lastMove);
            }
        }
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

    int CompareFitnesses(SimpleChess x, SimpleChess y)
    {
        if (x.fitness == y.fitness)
        {
            return 0;
        }
        else if (x.fitness > y.fitness)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    CancellationTokenSource cancelSource;
    CancellationToken token;
    private void OnApplicationQuit()
    {
        //print("stopping");
        Stop();
    }
    public void Stop()
    {
        if (cancelSource != null)
        {
            cancelSource.Cancel();
        }
        started = false;
        thinking = false;
        moveNum = 0;
    }
    Stopwatch timer = new Stopwatch();
    List<Task> tasks = new List<Task>();
    private double lastThreadCompleted = 0;
    public void TakeTurn()
    {
        timer.Reset();
        AI.reset();
        timer.Start();
        moveNum = 0;
        currentBoard = new SimpleChess(manager.board);
        calcMoves = generateMoves(manager.board, color);
        started = true;
        bestLines = new List<List<string>>(calcMoves.Count);
        if (manager.turn == 1 && randomFirstMove)
        {
            return;
        }
        tasks.Clear();
        cancelSource = new CancellationTokenSource();
        token = cancelSource.Token;
        foreach (SimpleChess move in calcMoves)
        {
            thinking = true;
            Task calcTask = Task.Factory.StartNew(() => { 
                AI.IterativeDeepening(move, token);
                /*print("finished move " + moveNum);*/
                if (Interlocked.Increment(ref moveNum) == calcMoves.Count) { thinking = false; }
                Interlocked.Exchange(ref lastThreadCompleted, timer.ElapsedMilliseconds);
            }, token);
            tasks.Add(calcTask);
        }
    }
    public void setWeights(Gene gene)
    {
        this.gene = gene;
        PieceWeight = gene.Weights[0];
        CenterWeight = gene.Weights[1];
        DevelopmentWeight = gene.Weights[2];
        PressureWeight = gene.Weights[3];
        KingWeight = gene.Weights[4];
        PawnWeight = gene.Weights[5];
        AI.setWeights(PieceWeight, CenterWeight, DevelopmentWeight, PressureWeight, KingWeight, PawnWeight);
    }
}
