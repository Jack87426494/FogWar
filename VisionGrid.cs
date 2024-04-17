// 表示所有玩家的视野网格
using System.Collections.Generic;
using UnityEngine;

public class VisionGrid
{
    //网格宽度和高度
    public int width;
    public int height;

    //存储每个网格部分的玩家视野信息
    public int[] values;

    //存储每个网格部分的玩家访问信息
    public int[] visited;

    public VisionGrid()
    {
        width = MapGenerator.Instance.width;
        height = MapGenerator.Instance.height;
        values = new int[width * height];
        visited = new int[width * height];
    }

    //设置网格部分为可见
    public void SetVisible(int i, int j, int players)
    {
        values[i + j * width] |= players;
        visited[i + j * width] |= players;
    }

    //清空网格数据
    public void Clear()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = 0;
            visited[i] = 0;
        }
    }

    //检查网格部分是否可见
    public bool IsVisible(int i, int j, int players)
    {
        return (values[i + j * width] & players) > 0;
    }

    //检查网格部分之前是否被访问
    public bool WasVisible(int i, int j, int players)
    {
        return (visited[i + j * width] & players) > 0;
    }
}