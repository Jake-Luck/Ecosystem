using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {
        NoiseMap,
        CubeMap
    };
    public DrawMode drawMode;

    public GameObject tileContainer;
    public GameObject mapTile;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public bool autoUpdate;

    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        if (drawMode == DrawMode.NoiseMap) {
            MapDisplay display = FindObjectOfType<MapDisplay> ();
            display.DrawNoiseMap (noiseMap);
        } 
        else if (drawMode == DrawMode.CubeMap) {
            foreach(Transform child in tileContainer.transform) {
                Destroy(child.gameObject);
            }
            for (int x = 0; x < mapWidth; x++) {
                for (int y = 0; y < mapHeight; y++) {
                    var newTile = Instantiate(mapTile, new Vector3(x + 0.5f - (mapWidth / 2), 0, y + 0.5f - (mapWidth / 2)), Quaternion.identity) as GameObject;
                    newTile.transform.parent = tileContainer.transform;

                    var tileController = newTile.GetComponent<TileController>();                  
                    float noiseValue = noiseMap[x,y];
                    tileController.noiseLevel = noiseValue;
                    tileController.updateTile();
                }
            }
        }
    }

    void OnValidate() {
        if (mapWidth < 1) {
            mapWidth = 1;
        }
        if (mapHeight < 1) {
            mapHeight = 1;
        }
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }

}
