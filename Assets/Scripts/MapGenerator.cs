using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    /*Lớp MapGenerator kế thừa từ MonoBehaviour, cho phép
    //nó được gắn vào các đối tượng (GameObject) trong Unity và sử dụng các sự kiện Unity như Start, UpdateTerrainChunk,
    hoặc OnValidate.*/
    public enum DrawMode {NoiseMap, ColourMap, Mesh};
    public DrawMode drawMode;
    /*DrawMode là một kiểu liệt kê, xác định các kiểu hiển thị khác nhau của bản đồ:
    NoiseMap: Chế độ hiển thị bản đồ dựa trên giá trị độ cao.
    ColourMap: Chế độ hiển thị dựa trên màu sắc của từng vùng.
    Mesh: Chế độ hiển thị lưới 3D, tạo mô hình địa hình từ bản đồ nhiễu.*/

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;//Phóng to, thu nhỏ cho bản đồ nhiễu, điều chỉnh chi tiết của địa hình.

    public int octaves;//Số lần lặp để tạo độ chi tiết trong nhiễu.
    [Range(0,1)]
    public float persistance;//Mức ảnh hưởng của từng lần lặp.
    public float lacunarity;//Tần số biến đổi của nhiễu.

    public int seed;//Hạt giống để tạo bản đồ nhiễu, tạo ngẫu nhiên hóa bản đồ.
    public Vector2 offset;//Độ lệch của bản đồ, giúp di chuyển bản đồ theo các hướng.

    public float meshHeightMultiplier;//Điều chỉnh độ cao của mô hình lưới.
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;//Cho phép tự động cập nhật bản đồ khi thay đổi các thông số.

    public TerrainType[] regions;//Mảng các TerrainType, xác định các loại địa hình với độ cao và màu sắc khác nhau.

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData. colourMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback) {
    
    }

    MapData GenerateMapData() {
        float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        /*GenerateMapData tạo ra bản đồ dựa trên các thông số cấu hình và hiển thị bản đồ theo chế độ đã chọn:
        noiseMap là mảng 2D lưu các giá trị độ cao được tạo bằng hàm Noise.GenerateNoiseMap.*/

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x,y];
                for (int i = 0; i < regions.Length; i++) {
                    if(currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions [i].colour;
                        break;
                    }
                }
            }
        }


        return new MapData(noiseMap, colourMap);
    }

    private void OnValidate()
    {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
