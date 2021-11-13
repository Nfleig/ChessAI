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
    public TextMesh NodeSpeed;
    public ChessAI AI;
    public float PieceWeight;
    public float CenterWeight;
    public float DevelopmentWeight;
    public float PressureWeight;
    public float KingWeight;
    public float PawnWeight;
    public float PawnAdvancementWeight;
    public bool displayLetters;

    List<SimpleChess> calcMoves;
    int moveNum = 0;
    public bool started = false;
    public bool randomFirstMove = false;
    public bool rollingEvaluation = false;
    private bool isActive = false;
    public Gene gene;

    public static int[] pieceValue = { 0, 1, 3, 4, 4, 7, 10 };

    void Awake()
    {
        AI = new ChessAI(simulatedTurns, color, grainSize, PieceWeight, CenterWeight, DevelopmentWeight, PressureWeight, KingWeight, PawnWeight);
        AI.setFitnessAlgorithm(CalculateFitness);
        turnTimer = turnDelay;
        makeGene(true);
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
        if (manager.getPaused() || !isActive)
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
                    //thinking = false;
                }
            }
        }
        if (started && (manager.turn % 2 == 0) == (color == -1))
        {
            if (manager.turn < 3 && randomFirstMove)
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
                if (moveNum == calcMoves.Count && rollingEvaluation)
                {
                    moveNum = 0;
                    tasks.Clear();
                    foreach(AIState state in states)
                    {
                        Task calcTask = Task.Factory.StartNew(() => {
                            AI.IterativeDeepening(state, token);
                            /*print("finished move " + moveNum);*/
                            Interlocked.Increment(ref moveNum);
                        }, token);
                        tasks.Add(calcTask);
                    }
                }
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
                if (calcMoves[0].lastMove.promotion != 0)
                {
                    manager.promotePiece(calcMoves[0].lastMove.to.x, calcMoves[0].lastMove.to.y, calcMoves[0].lastMove.promotion);
                }
                float time = (float) timer.ElapsedMilliseconds / 1000;
                int nps = (int) (AI.getNodeCount() / time);
                NodeSpeed.text = "Average speed:\n" + nps + " Nodes per second";
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

    public bool getActive()
    {
        return isActive;
    }

    public void Activate(bool active)
    {
        this.isActive = active;
        if(isActive)
        {
            StartAI();
        }
        else
        {
            Stop();
        }
    }


    Stopwatch timer = new Stopwatch();
    List<Task> tasks = new List<Task>();
    List<AIState> states = new List<AIState>();
    private double lastThreadCompleted = 0;
    public void TakeTurn()
    {
        timer.Reset();
        AI.reset();
        timer.Start();
        moveNum = 0;
        currentBoard = new SimpleChess(manager.board);
        currentBoard.setAIControlled(true);
        calcMoves = generateMoves(currentBoard, color);
        started = true;
        bestLines = new List<List<string>>(calcMoves.Count);
        if (manager.turn < 3 && randomFirstMove)
        {
            return;
        }
        tasks.Clear();
        cancelSource = new CancellationTokenSource();
        token = cancelSource.Token;
        foreach (SimpleChess move in calcMoves)
        {
            thinking = true;
            Task calcTask;
            if (rollingEvaluation)
            {
                AIState state = new AIState(move, AI.CalculateFitness(move));
                states.Add(state);
                calcTask = Task.Factory.StartNew(() => {
                    AI.IterativeDeepening(state, token);
                    /*print("finished move " + moveNum);*/
                    Interlocked.Increment(ref moveNum);
                }, token);
            }
            else
            {
                calcTask = Task.Factory.StartNew(() => {
                    AI.IterativeDeepening(move, token);
                    /*print("finished move " + moveNum);*/
                    if (Interlocked.Increment(ref moveNum) == calcMoves.Count) { thinking = false; }
                }, token);
            }
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

    public Gene makeGene(bool setGene)
    {
        Gene gene = new Gene();
        gene.Weights.Add(PieceWeight);
        gene.Weights.Add(CenterWeight);
        gene.Weights.Add(DevelopmentWeight);
        gene.Weights.Add(PressureWeight);
        gene.Weights.Add(KingWeight);
        gene.Weights.Add(PawnWeight);
        if (setGene)
        {
            this.gene = gene;
            setWeights(gene);
        }
        return gene;
    }

    public void setSimulatedTurns(int simulatedTurns)
    {
        AI.simulatedTurns = simulatedTurns;
        this.simulatedTurns = simulatedTurns;
    }

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

    //Fitness algorithm based on chess board evaluation guide at https://chessfox.com/example-of-the-complete-evaluation-process-of-chess-a-chess-position/
    
    public float testFitness(SimpleChess board)
    {
        float fitness = 0;
        List<SimpleChess.Coordinate> newBoard = new List<SimpleChess.Coordinate>(board.getPieces(1));
        newBoard.AddRange(board.getPieces(-1));
        foreach(SimpleChess.Coordinate location in newBoard)
        {
            if(board.testSpace(location) > 0)
            {
                fitness++;
            }
            else if(board.testSpace(location) < 0)
            {
                fitness++;
            }
            else
            {
                print("found 0");
            }
        }
        return fitness;
    }

    public float CalculateFitness(SimpleChess board)
    {
        //print("Start");
        float fitness = 0;
        List<SimpleChess.Coordinate> newBoard = new List<SimpleChess.Coordinate>(board.getPieces(1));
        newBoard.AddRange(board.getPieces(-1));
        foreach (SimpleChess.Coordinate location in newBoard)
        {
            int piece = board.testSpace(location);
            //print(piece);
            if (Math.Abs(piece) > 6)
            {
                print(piece);
                continue;
            }
            if (Math.Abs(piece) == 1)
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
                    if(Math.Abs(hitPiece) > 6)
                    {
                        continue;
                    }
                    int centerIndex = System.Math.Min(System.Math.Abs(4 - move.to.x), System.Math.Abs(3 - move.to.x)) + System.Math.Min(System.Math.Abs(4 - move.to.y), System.Math.Abs(3 - move.to.y));
                    if (piece * color > 0)
                    {
                        fitness += pieceValue[Math.Abs(hitPiece)] * PressureWeight;
                        if (centerIndex <= 2)
                        {
                            fitness += (3 - centerIndex) * CenterWeight * (1 / 3);
                        }
                    }
                    else
                    {
                        fitness -= pieceValue[Math.Abs(hitPiece)] * PressureWeight;
                        if (centerIndex <= 2)
                        {
                            fitness -= (3 - centerIndex) * CenterWeight * (1 / 3);
                        }
                    }
                }
                if (piece * color > 0)
                {
                    fitness += pieceValue[Math.Abs(piece)] * PieceWeight / 47f;
                    fitness += moveCount * DevelopmentWeight;

                }
                else
                {
                    fitness -= pieceValue[Math.Abs(piece)] * PieceWeight / 47;
                    fitness -= moveCount * DevelopmentWeight;
                }
            }

        }
        //print("End");
        return fitness;
    }
}
