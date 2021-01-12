using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Allows changing between viewing the full map and just the noise
    public enum DrawMode {
        NoiseMap,
        CubeMap
    };
    public DrawMode drawMode;

    // mapTileStuff
    public int mapWidth;
    public int mapHeight;
    public GameObject tileContainer;
    public GameObject mapTile;

    //Sheep stuff
    public GameObject sheep;
    public GameObject sheepContainer;
    public int numSheep;
    private static System.Random rng = new System.Random();
    private List<Vector3> spawnableTiles = new List<Vector3>();

    // Sebastian Lague's noise gen stuff
    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool autoUpdate;
    public bool useFalloff;
    private float[,] falloffMap;

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapWidth, mapHeight);
    }
    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        
        if (useFalloff) {
            for (int x = 0; x < mapWidth; x++) {
                for (int y = 0; y < mapHeight; y++) {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
            }
        }
        
        if (drawMode == DrawMode.NoiseMap) {
            MapDisplay display = FindObjectOfType<MapDisplay> ();
            display.DrawNoiseMap (noiseMap);
        } 
        else if (drawMode == DrawMode.CubeMap) {
            // Gets rid of any cube map that may already be there
            foreach(Transform child in tileContainer.transform) {
                Destroy(child.gameObject);
            }
            // Gets rid of any sheep that may already be there
            foreach(Transform child in sheepContainer.transform) {
                Destroy(child.gameObject);
            }

            // Loops through noise map array; creating cubes that store the noise level at each co-ordinate.
            for(int x = 0; x < mapWidth; x++) {
                for(int y = 0; y < mapHeight; y++) {
                    var newTile = Instantiate(mapTile, new Vector3(x + 0.5f - (mapWidth / 2), 0, y + 0.5f - (mapWidth / 2)), Quaternion.identity); // Division and "+0.5f" is to center the map at 0, 0
                    float noiseLevel = new float();
                    noiseLevel = noiseMap[x, y];
                    newTile.transform.parent = tileContainer.transform;

                    var tileController = newTile.GetComponent<TileController>();
                    tileController.noiseLevel = noiseLevel;
                    tileController.InitializeTile();

                    if (numSheep > 0 && noiseLevel >= 0.5) {
                        spawnableTiles.Add(new Vector3(x + 0.5f - (mapWidth / 2), 1, y + 0.5f - (mapWidth / 2)));
                    }
                }
            }

            // Calls spawn sheep method
            if (numSheep > 0) {
                SpawnSheep(noiseMap);
            }
        }
    }

    void SpawnSheep(float[,] noiseMap) {
        for(int i = spawnableTiles.Count - 1; i > 0; i--) {
            int pointer = rng.Next(i + 1);
            Vector3 value = spawnableTiles[pointer];
            spawnableTiles[pointer] = spawnableTiles[i];
            spawnableTiles[i] = value; 
        }
        // Makes sure there can't be more sheep than there is tiles
        if(spawnableTiles.Count < numSheep) {
            numSheep = spawnableTiles.Count;
        }
        // Places sheep on their tiles
        for(int i = 0; i < numSheep; i++) {
            var newSheep = Instantiate(sheep, spawnableTiles[i], Quaternion.identity);
            newSheep.transform.parent = sheepContainer.transform;
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
