using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Map : MonoBehaviour
{
    [SerializeField] private Tilemap _tileMap;
    [SerializeField] private BiomePreset[] _biomes;

    [Header("Dimensions")]
    public int width = 50;
    public int height = 50;
    public float scale = 1.0f;
    public Vector2 offset;

    [Header("Height Map")]
    public Wave[] heightWaves;
    public float[,] heightMap;

    [Header("Moisture Map")]
    public Wave[] moistureWaves;
    private float[,] moistureMap;

    [Header("Heat Map")]
    public Wave[] heatWaves;
    private float[,] heatMap;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // height map
        heightMap = NoiseGenerator.Generate(width, height, scale, heightWaves, offset);
        // moisture map
        moistureMap = NoiseGenerator.Generate(width, height, scale, moistureWaves, offset);
        // heat map
        heatMap = NoiseGenerator.Generate(width, height, scale, heatWaves, offset);

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                _tileMap.SetTile(new Vector3Int(x, y, 0), GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]).GetTile());
            }
        }
    }

    public void RandomizeSeeds()
    {
        for (int i = 0; i < heightWaves.Length; i++)
        {
            heightWaves[i].seed = Random.Range(0, 200);
        }
        for (int i = 0; i < moistureWaves.Length; i++)
        {
            moistureWaves[i].seed = Random.Range(0, 500);
        }
        for (int i = 0; i < heatWaves.Length; i++)
        {
            heatWaves[i].seed = Random.Range(0, 300);
        }
    }

    private BiomePreset GetBiome(float height, float moisture, float heat)
    {
        List<BiomeTempData> biomeTemp = new List<BiomeTempData>();
        foreach (BiomePreset biome in _biomes)
        {
            if (biome.MatchCondition(height, moisture, heat))
            {
                biomeTemp.Add(new BiomeTempData(biome));
            }
        }
        float curVal = 0.0f;

        BiomePreset biomeToReturn = null;
        foreach (BiomeTempData biome in biomeTemp)
        {
            if (biomeToReturn == null)
            {
                biomeToReturn = biome.biome;
                curVal = biome.GetDiffValue(height, moisture, heat);
            }
            else
            {
                if (biome.GetDiffValue(height, moisture, heat) < curVal)
                {
                    biomeToReturn = biome.biome;
                    curVal = biome.GetDiffValue(height, moisture, heat);
                }
            }
        }

        if (biomeToReturn == null)
            biomeToReturn = _biomes[0];

        return biomeToReturn;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Generate Map"))
        {
            GenerateMap();
        }

        if (GUILayout.Button("Randomize Seeds"))
        {
            RandomizeSeeds();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Map map = (Map)target;
        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
        if (GUILayout.Button("Randomize Seeds"))
        {
            map.RandomizeSeeds();
        }
        DrawDefaultInspector();
    }
}
#endif

public class BiomeTempData
{
    public BiomePreset biome;

    public BiomeTempData(BiomePreset preset)
    {
        biome = preset;
    }

    public float GetDiffValue(float height, float moisture, float heat)
    {
        return (height - biome.minHeight) + (moisture - biome.minMoisture) + (heat - biome.minHeat);
    }
}