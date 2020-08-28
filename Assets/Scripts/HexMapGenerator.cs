using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// ReSharper disable All

public class HexMapGenerator : MonoBehaviour
{
    
    public int length;
    public int height;
    public int scale;
    public float yScale;
    public HeightMapSettings heightMapSettings;
    
    public GameObject prefabTile;

    public NavMeshSurface surface;
    // Start is called before the first frame update
    void Start()
    {     
        HeightMap heightMap = GenerateHeightMap(length, height, heightMapSettings, Vector2.zero);
        float[,] falloffMap = GenerateFalloffMap(length);
        //float xIntervall = prefabTile;
        for (int xCoord =0; xCoord<=length-1; xCoord++)
        {
            float zOffset = (xCoord % 2)/2.0f;
            for (int zCoord = 0; zCoord <= length - 1; zCoord++)
            {
               

                float yCoord = yScale* (heightMap.values[xCoord, zCoord]/heightMap.maxValue - falloffMap[xCoord, zCoord]);
                GameObject newTile = Instantiate(prefabTile, scale* new Vector3(0.75f*xCoord, yCoord, 0.865f*(zCoord+zOffset)), Quaternion.identity);
                newTile.transform.localScale = new Vector3(scale, heightMap.maxValue*yScale, scale);
            }
            
            
        }
        
        surface.BuildNavMesh(); 

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("shit");
    }
    
    public HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre) {
        float[,] values = Noise.GenerateNoiseMap (width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values [i, j] *= heightCurve_threadsafe.Evaluate (values [i, j]) * settings.heightMultiplier;

                if (values [i, j] > maxValue) {
                    maxValue = values [i, j];
                }
                if (values [i, j] < minValue) {
                    minValue = values [i, j];
                }
            }
        }

        return new HeightMap (values, minValue, maxValue);
    }
    
    public float[,] GenerateFalloffMap(int size) {
        float[,] map = new float[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max (Mathf.Abs (x), Mathf.Abs (y));
                map [i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value) {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow (value, a) / (Mathf.Pow (value, a) + Mathf.Pow (b - b * value, a));
    }

}

    public struct HeightMap {
        public readonly float[,] values;
        public readonly float minValue;
        public readonly float maxValue;

        public HeightMap (float[,] values, float minValue, float maxValue)
        {
            this.values = values;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }


    }
