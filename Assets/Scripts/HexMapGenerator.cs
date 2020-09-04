using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

// ReSharper disable All

public class HexMapGenerator : MonoBehaviour
{
    //Tilesizes
    private static float tilesPer1UnitY = 4.5312f;
    private static float tilesPer1UnitX = 0.75f;

    private static float tilesPer1UnitZ = 0.865f;

    //Scale settings
    public int length;
    public int width;
    public int scale;
    public float yScale;

    //Map Settings
    public MapSettings heightMapSettings;
    public MapSettings humidityMapSettings;
    public MapSettings temperatureMapSettings;
    public GameObject waterPlane;
    public float waterHeight;

    public bool useFalloff;
    public bool heightAffectsTemperature;
    public bool temperatureAffectsHumidity;

    //Tiles
    public TileConditions[] tileConditions;

    //Navmesh
    public NavMeshSurface surface;

    void Start()
    {
        GenerateTileMap();

        surface.BuildNavMesh();
    }

    private void GenerateTileMap()
    {
        //Maps
        Map falloffMap = GenerateFalloffMap(length);
        Map heightMap = null;
        Map humidityMap = null;
        Map temperatureMap = null;

        //Create Height Map
        if (useFalloff)
        {
            heightMap = GenerateMapWithMulitplier(length, width, heightMapSettings, Vector2.zero, falloffMap, false);
        }
        else
        {
            heightMap = GenerateMap(length, width, heightMapSettings, Vector2.zero);
        }


        //Create Temperature Map
        if (heightAffectsTemperature)
        {
            temperatureMap =
                GenerateMapWithMulitplier(length, width, temperatureMapSettings, Vector2.zero, heightMap, true);
        }
        else
        {
            temperatureMap = GenerateMap(length, width, temperatureMapSettings, Vector2.zero);
        }

        //Create Humidity Map
        if (temperatureAffectsHumidity)
        {
            humidityMap =
                GenerateMapWithMulitplier(length, width, humidityMapSettings, Vector2.zero, temperatureMap, false);
        }
        else
        {
            humidityMap = GenerateMap(length, width, humidityMapSettings, Vector2.zero);
        }

        //Variables for scaling
        float zScale = tilesPer1UnitZ * scale;
        float xScale = tilesPer1UnitX * scale;
        yScale *= scale;
        float maxYDifference = yScale * tilesPer1UnitY;

        //Positioning Water plane
        waterPlane.transform.position = new Vector3(tilesPer1UnitX * scale * length / 2f, waterHeight * yScale,
            tilesPer1UnitZ * scale * width / 2f);
        waterPlane.transform.localScale = new Vector3(tilesPer1UnitX * length / 2f, 1, tilesPer1UnitZ * width / 2f);

        //Variables for max Map values
        float maxHeight = heightMap.maxValue;
        float maxTemperature = temperatureMap.maxValue;
        float maxHumidity = humidityMap.maxValue;

        //Generate Tiles by looping over x and z
        for (int xCoord = 0; xCoord < length; xCoord++)
        {
            float zOffset = (xCoord % 2) / 2.0f;
            float xCoordNormalized = xCoord * xScale;
            float zCoordNormalized;
            float yCoordNormalized;
            for (int zCoord = 0; zCoord < width; zCoord++)
            {
                //Get map data at position
                zCoordNormalized = (zCoord + zOffset) * zScale;
                float height = heightMap.values[xCoord, zCoord];
                float humidity = humidityMap.values[xCoord, zCoord];
                float temperature = temperatureMap.values[xCoord, zCoord];

                yCoordNormalized = yScale * height;
                Vector3 coordsForTile = new Vector3(xCoordNormalized, yCoordNormalized - maxYDifference / 20,
                    zCoordNormalized);

                //Find valid Tile and instantiate it
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

        //Should never happen 
        Debug.Log("no valid tile found");
        return null;
    }


    //Code by Sebastian Lague: https://github.com/SebLague/Procedural-Landmass-Generation
    // Modified by me to suit my needs and make the code more perfomant and more modular
    public Map GenerateMap(int width, int height, MapSettings settings, Vector2 sampleCentre)
    {
        //Initializing map values
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        //Generate mapvalue from noise map
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]);

                //Update max/min value
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

    public Map GenerateMapWithMulitplier(int width, int height, MapSettings settings, Vector2 sampleCentre,
        Map otherMap, bool inverse = false)
    {
        //Initializing map values
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        //Generate mapvalue from noise map
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //Get value at postion
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]);
                values[i, j] *= (inverse) ? Mathf.Lerp(1f, 0f, otherMap.values[i, j]) : otherMap.values[i, j];

                //Update max/min value
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

    public Map GenerateFalloffMap(int size)
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

        return new Map(map, 0f, 1f);
    }

    // Sigmoid curve evaluation for falloff maps
    static float Evaluate(float value)
    {
        const float a = 2f;
        const float b = 5f;

        float returnValue = Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        return Mathf.Lerp(1f, 0f, returnValue);
    }
}

//Map class for storing world data
public class Map
{
    public float maxValue;
    public float minValue;
    public float[,] values;

    public Map(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}