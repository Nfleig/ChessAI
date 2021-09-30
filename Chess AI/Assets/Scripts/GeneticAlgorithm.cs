using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Gene
{
    //The order of Weights in the Weights list is:
    //PieceWeight, CenterWeight, DevelopmentWeight, PressureWeight, KingWeight, PawnWeight
    public List<float> Weights = new List<float>();
    public float fitness = 0;


    public Gene(List<float> Weights)
    {
        this.Weights = Weights;
        fitness = 0;
    }
    public Gene(Gene parent)
    {
        this.Weights = new List<float>(parent.Weights);
        fitness = 0;
    }
}

public class GeneticAlgorithm : MonoBehaviour
{

    /// <summary>
    /// 
    /// GENETIC ALGORITHM RUNTIME STRUCTURE
    /// 
    /// Data structures:
    ///     Gene: 
    ///         List<float> weightValues;
    ///         float fitness;
    /// 
    /// Setup(){
    ///     Instantiate batchSize boards, then scale and space them out so that they fit in the screen;
    ///     
    /// }
    /// 
    /// Start(){
    ///     Initialize a pool of AI's with randomized weight values;
    ///     Assign two AI's to play each other at each board by their place in the list of AI's;
    ///     
    /// }
    /// 
    /// Update(){
    ///     Cycle through each board in the batch and simulate one move at a time;
    ///     Once a game is done evaluate each AI and record their fitness
    ///         Add the finished AI's to a list of finished games and then put the next two waiting AI's on that board
    ///     Once every game is done, run NextGeneration();
    ///     Once the next generation is ready, randomly assign AI's to games and repeat
    /// }
    /// 
    /// NextGeneration(){
    ///     Pair together sets of Genes using roulette method
    ///     BreedParents();
    /// }
    /// 
    /// BreedParents(){
    ///     Select points A and B where 0 <= A < B && A < B <= Gene length
    ///     Child 1 get's the weight values from 0 to A from parent 1, from A to B from parent 2, and from B to end from parent 1
    ///     Child 2 get's the weight values from 0 to A from parent 2, from A to B from parent 1, and from B to end from parent 2
    ///     Mutate children randomly
    ///     Children replace the Genes with the lowest fitness
    /// }
    /// </summary>

    

    //The prefab for the chessboard
    //public GameObject boardPrefab;

    public TextMesh generationCount;
    public TextMesh AICount;
    public TextMesh turnCount;

    //private List<GameObject> allBoards = new List<GameObject>();

    public GameObject board;
    private DeepGold[] AIs;
    private GameManager manager;

    private List<Gene> genePool = new List<Gene>();
    private List<Gene> unfinishedGenes = new List<Gene>();
    private List<Gene> finishedGenes = new List<Gene>();
    private int evaluatedGenes = 0;

    private List<GameObject> finishedBoards = new List<GameObject>();
    
    //The size of our genepool
    public int populationSize;

    //Simulating every board at once is stupid so the population will by divided up into batches
   // public int batchSize;

    //How many sets of genes should be allowed to breed for each generation
    public int families;

    //How many turns should the games go? (Checkmate probably wont happen)
    public int gameLength;
    public float turnDelay;
    private float turnTimer;

    //private int boardNum;

    private bool simulatingTurn = false;

    //The chance for a child to mutate
    public float mutateChance;

    private int generation = 0;

    //Instantiates all of the board objects
    //It's absolutely brute force but I don't care
    /*
    public void SetupBoards()
    {
        float leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0)).x;
        float bottomEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0)).y;
        float rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 1)).x;
        float topEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 1)).y;

        
        //print(topEdge);
        float size = (topEdge - bottomEdge) / (16 * 1.5f);
        float startingPoint = leftEdge * 0.75f;
        float startingY = bottomEdge * 0.75f;
        for(int i = 0; i < 16; i++)
        {
            GameObject newBoard = Instantiate(boardPrefab, new Vector3(startingPoint + ((i % 4) * 4.75f), startingY * ((int)(i / 4) * 0.6f) + 3, 0f), Quaternion.identity);
            newBoard.transform.localScale = new Vector3(size / 2, size / 2);
            //allBoards.Add(newBoard);
        }
    }
    */

    public void InitializePopulation()
    {
        for(int i = 0; i < populationSize; i++)
        {
            Gene newGene = new Gene(new List<float>());
            for(int c = 0; c < 6; c++)
            {
                newGene.Weights.Add(Random.value * 100);
            }
            genePool.Add(newGene);
            unfinishedGenes.Add(newGene);
        }
        PopulateBoard();
        
    }
    void PopulateBoard()
    {
        finishedGenes.Clear();
        DeepGold[] AIs = board.GetComponentsInChildren<DeepGold>();
        if(AIs.Length > 1 && unfinishedGenes.Count > 1)
        {
            AIs[0].setWeights(unfinishedGenes[0]);
            unfinishedGenes.Remove(unfinishedGenes[0]);
            AIs[1].setWeights(unfinishedGenes[0]);
            unfinishedGenes.Remove(unfinishedGenes[0]);
        }
        evaluatedGenes = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        AIs = board.GetComponentsInChildren<DeepGold>();
        manager = board.GetComponentInChildren<GameManager>();
        SaveSystem.InitFolder();
        //SetupBoards();
        InitializePopulation();
        turnTimer = turnDelay;
        //marker.transform.localScale = allBoards[0].transform.localScale;
    }
    private int number;
    // Update is called once per frame
    void Update()
    {
        if (!simulatingTurn)
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer >= 0)
            {
                return;
            }
            else
            {
                turnTimer = turnDelay;
            }
            if(manager.isGameFinished() || manager.turn > gameLength)
            {
                foreach(DeepGold AI in AIs)
                {
                    AI.gene.fitness = EvaluateFitness(AI);
                    SaveSystem.SaveGene(AI.gene, generation, evaluatedGenes);
                    evaluatedGenes++;
                    if (unfinishedGenes.Count > 0)
                    {
                        AI.setWeights(unfinishedGenes[0]);
                        finishedGenes.Add(unfinishedGenes[0]);
                        unfinishedGenes.Remove(unfinishedGenes[0]);
                        manager.Restart();
                        simulatingTurn = false;
                    }
                    else
                    {
                        NextGeneration();
                        simulatingTurn = false;
                        break;
                    }
                }
            }
            else
            {
                manager.takeTurn();
                simulatingTurn = true;
            }
        }
        if (manager.isTurnFinished())
        {
            simulatingTurn = false;
        }
        generationCount.text = "Generation " + generation;
        AICount.text = "Evaluated AI's " + evaluatedGenes + "/" + populationSize;
        turnCount.text = "Turn " + (board.GetComponentInChildren<GameManager>().turn);
    }

    void NextGeneration()
    {
        double sumFitness = 0;
        genePool.Sort(CompareFitnesses);
        for(int i = 0; i < genePool.Count; i++)
        {
            SaveSystem.SaveGene(genePool[i], generation, i);
            sumFitness += genePool[i].fitness;
        }
        List<Gene> parentGenes = new List<Gene>();
        for (int i = 0; i < parentGenes.Count; i++)
        {
            double target = sumFitness * Random.value;
            int index = genePool.Count - 1;
            double partialsum = 0;
            while ((target > 0 && partialsum < target) || (target < 0 && partialsum > target) && index >= 0)
            {
                partialsum += genePool[index].fitness;
                if ((target > 0 && partialsum >= target) || (target <= 0 && partialsum <= target))
                {
                    parentGenes.Add(genePool[index]);
                }
                index--;
            }
        }
        shredderIndex = 0;
        for(int i = 0; i < parentGenes.Count - 1; i += 2)
        {
            BreedParents(parentGenes[i], parentGenes[i + 1]);
        }
        unfinishedGenes = new List<Gene>(genePool);
        PopulateBoard();
        generation++;
    }
    int shredderIndex = 0;
    void BreedParents(Gene parent1, Gene parent2)
    {
        if (parent1.Weights.Count > 3)
        {
            int pointA = 0;
            int pointB = 0;
            while (pointA == pointB)
            {
                pointA = (int)(Random.value * (parent1.Weights.Count - 1));
                pointB = (int)(Random.value * (parent1.Weights.Count - pointA - 1)) + pointA;
            }
            Gene[] children = new Gene[2];
            children[0] = new Gene(new List<float>());
            children[1] = new Gene(new List<float>());
            for (int i = 0; i < pointA; i++)
            {
                children[0].Weights.Add(parent1.Weights[i]);
                children[1].Weights.Add(parent2.Weights[i]);
            }
            for (int i = pointA; i < pointB; i++)
            {
                children[0].Weights.Add(parent2.Weights[i]);
                children[1].Weights.Add(parent1.Weights[i]);
            }
            for (int i = pointB; i < parent1.Weights.Count; i++)
            {
                children[0].Weights.Add(parent1.Weights[i]);
                children[1].Weights.Add(parent2.Weights[i]);
            }

            //This conditional statement will then potentially mutate one of the children as described earlier.
            if (Random.value <= mutateChance)
            {
                if (Random.value <= 0.5)
                {
                    MutateGene(children[0]);
                }
                else
                {
                    MutateGene(children[1]);
                }
            }

            genePool[genePool.Count - shredderIndex] = children[0];
            genePool[genePool.Count - shredderIndex - 1] = children[1];
            shredderIndex += 2;

        }
    }
    public void MutateGene(Gene gene)
    {
        int mutateTimes = (int) (Random.value * 5) + 1;
        for(int i = 0; i < mutateTimes; i++)
        {
            int index = (int)(Random.value * 5);
            gene.Weights[index] = Random.value * 100;
        }
    }

    float EvaluateFitness(DeepGold AI)
    {
        float fitness = 0;

        if(manager.getWinner() == AI.color)
        {
            fitness += 100;
            fitness += gameLength - AI.manager.turn;
        }
        if(manager.getWinner() == -AI.color)
        {
            fitness -= 100;
            fitness += AI.manager.turn;
        }
        else
        {
            fitness += AI.manager.turn;
        }
        List<SimpleChess.Coordinate> newBoard = new List<SimpleChess.Coordinate>(manager.board.getPieces(1));
        newBoard.AddRange(manager.board.getPieces(-1));
        foreach (SimpleChess.Coordinate location in newBoard)
        {
            int piece = manager.board.getPiece(location);
            if(System.Math.Sign(piece) == AI.color)
            {
                fitness += ChessAI.pieceValue[System.Math.Abs(piece)];
            }
            else
            {
                fitness -= ChessAI.pieceValue[System.Math.Abs(piece)];
            }
        }

        return fitness;
    }

    int CompareFitnesses(Gene x, Gene y)
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
}
