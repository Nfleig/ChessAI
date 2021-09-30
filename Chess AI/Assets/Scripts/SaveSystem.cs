
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    static int attempts;
    public static void SaveGene(Gene gene, int generation, int number)
    {
        string jsonGene = JsonUtility.ToJson(gene);
        if (!Directory.Exists(Application.persistentDataPath + "/Attempt " + attempts + "/Generation " + generation))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Attempt " + attempts + "/Generation " + generation);
        }
        string path = Application.persistentDataPath + "/Attempt " + attempts + "/Generation " + generation + "/Gene" + number + ".json";
        File.WriteAllText(path, jsonGene);
        
    }

    public static void InitFolder()
    {
        attempts = 0;
        while (Directory.Exists(Application.persistentDataPath + "/Attempt " + attempts))
        {
            attempts++;
        }
        Directory.CreateDirectory(Application.persistentDataPath + "/Attempt " + attempts);
    }
    public static void LoadGene(DeepGold AI, int attempt, int generation, int number)
    {
        string path = Application.persistentDataPath + "/Attempt " + attempt + "/Generation " + generation + "/Gene" + number + ".json";
        if(File.Exists(path))
        {
            Gene savedGene = JsonUtility.FromJson<Gene>(path);
            AI.setWeights(savedGene);
        }
        else
        {
            throw new System.Exception("Could not find File: " + path);
        }
    }
}
