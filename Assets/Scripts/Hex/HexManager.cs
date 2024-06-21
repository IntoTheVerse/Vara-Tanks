using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

[System.Serializable]
public class VectorListWrapper
{
    public int[] l;
    public int[] w;
    public int[] h;
}

public struct HexData
{
    public Transform transform;
    public Vector3 coordinate;
    public Hex hex;
}

public class HexManager : MonoBehaviour
{
    public GameObject Hex;
    public Transform HexSpawner;
    public Vector2 size;
    public bool gameScene = false;
    private List<Vector3> hexDatasScale = new();
    private List<HexData> hexDatas = new();

    private void Start()
    {
        SpawnHex();
        SetData("H4sIAAAAAAAAA+1ROQ7DMAz7S2YPluuzXymyd+jeoejfS4p2gP4gQwDZkURKlOLP9truj9LDZee2PWxvvlQMl53b8FJPvFRLAZY771sJFqclD3HLAWr4gFcB4yiwOFmqVhsSRw+9ODHBzR5JimkQxP7LkkxBDoDDFMKxWCqSETDqN/FBMlM5ujS/AWs2OMTRZc7eVLyERp9DHNvOGQkKoeeazGaJeYs1Fiskq/5d1RRNWmKudGhoL1lzxlpEI6u7fnCNayRh1Yly1NDi/v0B5DibwiUFAAA=");
        GetComponent<GameManager>().GameInit();
    }

    private void SpawnHex()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                float xGap = i;
                if (j % 2 == 0 || j == 0) xGap -= 0.5f;

                SpawnHexInPosition(xGap, j != 0 ? j - (j * 0.25f) : j, new Vector3(i, 0, j));
            }
        }
        SetNeighbours();
    }

    private void SpawnHexInPosition(float i, float j, Vector3 coord)
    {
        Transform goLeft = Instantiate(Hex, HexSpawner).transform;
        goLeft.position = new Vector3(i, 0, j);

        HexData hexData = new()
        {
            transform = goLeft,
            coordinate = coord,
            hex = goLeft.GetComponent<Hex>()
        };
        hexDatas.Add(hexData);
        hexData.hex.SetHexData(hexData);
    }

    public void GetData()
    {
        UpdateScaleFromObject();
        var compressedData = GetCompressedData(hexDatasScale);
        Debug.Log(compressedData);
        Debug.Log($"Size Compressed: {Encoding.Unicode.GetByteCount(compressedData)}");
    }

    public void SetData(string compressedData)
    {
        List<Vector3> decompressedData = GetDecompressedData(compressedData);
        UpdateScaleFromData(decompressedData);
    }

    private void UpdateScaleFromObject()
    {
        hexDatasScale.Clear();
        foreach (var hex in hexDatas) 
        {
            hexDatasScale.Add(hex.transform.localScale);
        }
    }

    private void UpdateScaleFromData(List<Vector3> data)
    {
        hexDatasScale = data;
        for (int i = 0; i < hexDatas.Count; i++)
        {
            hexDatas[i].transform.localScale = hexDatasScale[i];
        }
    }

    private string GetCompressedData(List<Vector3> array)
    {
        VectorListWrapper wrapper = new()
        {
            l = new int[array.Count],
            w = new int[array.Count],
            h = new int[array.Count]
        };

        for (int i = 0; i < array.Count; i++)
        {
            wrapper.l[i] = Mathf.RoundToInt(array[i].x * 100);
            wrapper.w[i] = Mathf.RoundToInt(array[i].y * 100);
            wrapper.h[i] = Mathf.RoundToInt(array[i].z * 100);
        }

        string jsonData = JsonUtility.ToJson(wrapper);
        byte[] byteCompression = CompressString(jsonData);
        string base64String = System.Convert.ToBase64String(byteCompression);

        return base64String;
    }

    private List<Vector3> GetDecompressedData(string data)
    {
        byte[] byteCompression = System.Convert.FromBase64String(data);
        string decompressedString = DecompressString(byteCompression);
        VectorListWrapper wrapper = JsonUtility.FromJson<VectorListWrapper>(decompressedString);

        List<Vector3> transforms = new();
        for (int i = 0; i < wrapper.h.Length; i++)
        {
            transforms.Add(new Vector3(wrapper.l[i] / 100f, wrapper.w[i] / 100f, wrapper.h[i] / 100f));
        }

        return transforms;
    }

    private byte[] CompressString(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);
        using MemoryStream memoryStream = new();
        using (GZipStream gzipStream = new(memoryStream, CompressionMode.Compress, true))
        {
            gzipStream.Write(data, 0, data.Length);
        }
        return memoryStream.ToArray();
    }

    private string DecompressString(byte[] compressedData)
    {
        using MemoryStream memoryStream = new(compressedData);
        using GZipStream gzipStream = new(memoryStream, CompressionMode.Decompress);
        using StreamReader reader = new(gzipStream);
        return reader.ReadToEnd();
    }

    private void SetNeighbours()
    {
        foreach (var hex in hexDatas)
        {
            Vector3[] neighbourCoords;

            if (hex.coordinate.z % 2 == 0 || hex.coordinate.z == 0)
            {
                neighbourCoords = new Vector3[]
                {
                    new(-1, 0, 1),
                    new(-1, 0, 0),
                    new(-1, 0, -1),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, -1),
                };
            }
            else
            { 
                neighbourCoords = new Vector3[]
                {
                    new(1, 0, 1),
                    new(1, 0, 0),
                    new(1, 0, -1),
                    new(-1, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, -1),
                };
            }

            List<Hex> neighbours = new();
            foreach (Vector3 neighbourCoord in neighbourCoords)
            {
                Vector3 tileCoord = hex.coordinate;

                for (int i = 0; i < hexDatas.Count; i++)
                {
                    if (hexDatas[i].coordinate == tileCoord + neighbourCoord)
                    {
                        neighbours.Add(hexDatas[i].hex);
                    }
                }
            }

            hex.hex.SetNeighbours(neighbours.ToArray());
        }
    }

    private void Update()
    {
        if (!gameScene)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButton(0))
            {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.transform.TryGetComponent(out IScalable hex))
                    {
                        hex.ModifyHeight();
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                foreach (var hex in hexDatas)
                {
                    hex.hex.InfluenceLost();
                }
            }
        }
    }

    public HexData GetHexDataByPos(int pos)
    {
        return hexDatas[pos];
    }
}