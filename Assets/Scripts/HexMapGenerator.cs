using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

// ReSharper disable All

public class HexMapGenerator : MonoBehaviour
{
    public int length;
    public int width;
    public int scale;
    public float yScale;
    public MapSettings heightMapSettings;
    public MapSettings humidityMapSettings;
    public MapSettings temperatureMapSettings;
    public GameObject waterPlane;
    public float waterHeight;

    public TileConditions[] tileConditions;

    public NavMeshSurface surface;

    private static float tilesPer1UnitY = 4.5312f;
    private static float tilesPer1UnitX = 0.75f;
    private static float tilesPer1UnitZ = 0.865f;
    void Start()
    {
        GenerateTileMap();
        
        surface.BuildNavMesh();
    }

    private void GenerateTileMap()
    {
        Map heightMap = GenerateMap(length, width, heightMapSettings, Vector2.zero);
        Map humidityMap = GenerateMap(length, width, humidityMapSettings, Vector2.zero);
        Map temperatureMap = GenerateMap(length, width, temperatureMapSettings, Vector2.zero);

        float[,] falloffMap = GenerateFalloffMap(length);
        float zScale = tilesPer1UnitZ * scale;
        float xScale = tilesPer1UnitX * scale;
        yScale *= scale;
        float maxYDifference = yScale * tilesPer1UnitY;
        Debug.Log("MaxYDif: " + maxYDifference.ToString());
        
        waterPlane.transform.position= new Vector3(tilesPer1UnitX *scale*length/2f, waterHeight* yScale, tilesPer1UnitZ *scale*width/2f);
        waterPlane.transform.localScale= new Vector3(tilesPer1UnitX *length/2f, 1, tilesPer1UnitZ *width/2f);
        
        
        float maxHeight = heightMap.maxValue;
        float maxTemperature = temperatureMap.maxValue;
        float maxHumidity = humidityMap.maxValue;
        for (int xCoord = 0; xCoord < length; xCoord++)
        {
            float zOffset = (xCoord % 2) / 2.0f;
            float xCoordNormalized = xCoord * xScale;
            float zCoordNormalized;
            for (int zCoord = 0; zCoord < width; zCoord++)
            {
                zCoordNormalized = (zCoord + zOffset) * zScale;
                float height = heightMap.values[xCoord, zCoord]  * falloffMap[xCoord, zCoord];
                float humidity = humidityMap.values[xCoord, zCoord];
                float temperature = temperatureMap.values[xCoord, zCoord];
                /*Debug.Log("Height Map value: " + (heightMap.values[xCoord, zCoord] / maxHeight).ToString());
                Debug.Log("Falloff Map Value: " + falloffMap[xCoord, zCoord].ToString());
                Debug.Log("Height Value: " + height.ToString());*/
                
                float yCoordNormalized = yScale * height;
                //(maxYDifference/2.0f)
                Vector3 coordsForTile = new Vector3(xCoordNormalized, yCoordNormalized-maxYDifference/20, zCoordNormalized);

                GameObject rightPrefab = findValidTile(height, humidity, temperature);

                GameObject newTile = Instantiate(rightPrefab, coordsForTile, Quaternion.identity);

                newTile.transform.localScale = new Vector3(scale, maxYDifference, scale);
            }
        }
    }

    public GameObject findValidTile(float height, float humidity, float temperature)
    {
        for (int i = 0; i < tileConditions.Length; i++)
        {
            if (tileConditions[i].CheckIfValid(height, humidity, temperature)) return tileConditions[i].prefabTile;
        }

        Debug.Log("no valid tile found");
        return null;
    }


    public Map GenerateMap(int width, int height, MapSettings settings, Vector2 sampleCentre)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]);

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }

                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new Map(values, minValue, maxValue);
    }

    public float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float) size * 2 - 1;
                float y = j / (float) size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 2f;
        float b = 1f;

        float returnValue = Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        return Mathf.Lerp(1f, 0f, returnValue);
    }
}

public struct Map
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public Map(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}