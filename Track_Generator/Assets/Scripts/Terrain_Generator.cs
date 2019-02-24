using UnityEngine;

public class Terrain_Generator : MonoBehaviour {

    public int depth = 20;

    public int width = 256;
    public int height = 256;

    public float scale = 20f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    public Terrain terrain; 

    public void GenerateTerrain()
    {
        terrain = GetComponent<Terrain>();

        terrain.terrainData.heightmapResolution = width + 1;

        terrain.terrainData.size = new Vector3(width, depth, height);

        terrain.terrainData.SetHeights(0, 0, GenerateHeights());
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord); 
    }


}
