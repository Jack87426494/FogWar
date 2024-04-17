using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//计算并更新战争迷雾
public class VisionCalculator : MonoBehaviour
{
    private Texture2D renderTexture;

    private VisionGrid visionGrid;
    private Terrain terrain;

    private SpriteRenderer sr;

    [Header("格子的大小")]
    public float gridSize = 2.54f;
    [Header("完全遮蔽的颜色")]
    public Color hideColor = new Color(0, 0, 0, 0.7f);
    [Header("半遮蔽的颜色,访问过得位置但不可见")]
    public Color visiteColor = new Color(0,0,0,0.3f);
    [Header("完全可见的颜色")]
    public Color canSeeColor = new Color(0, 0, 0, 0);

    private void Start()
    {
        //等待地图数据被设置完毕后再加载战争迷雾
        Invoke("Init", 1f);
    }

    private void Init()
    {
        sr = GetComponent<SpriteRenderer>();
        visionGrid = new VisionGrid();
        //创建一个新的Texture2D，大小与VisionGrid相同
        if (renderTexture == null)
        {
            renderTexture = new Texture2D(visionGrid.width, visionGrid.height);
        }

        sr.sprite = Sprite.Create(renderTexture, new Rect(0, 0, renderTexture.width, renderTexture.height), Vector2.zero);
        transform.localScale = new Vector3(visionGrid.width * gridSize, visionGrid.height * gridSize);
        InvokeRepeating("CalculateVision", 0, 0.5f);
    }

    /// <summary>
    /// 计算视野
    /// </summary>
    public void CalculateVision()
    {
        foreach (UnitVision unitVision in FindObjectsOfType<UnitVision>())
        {
            //计算单位视野的矩形范围
            int startX = Mathf.Max(0, Mathf.FloorToInt((unitVision.transform.position.x - unitVision.range) / gridSize));
            int startY = Mathf.Max(0, Mathf.FloorToInt((unitVision.transform.position.y - unitVision.range) / gridSize));
            int endX = Mathf.Min(visionGrid.width - 1, Mathf.CeilToInt((unitVision.transform.position.x + unitVision.range) / gridSize));
            int endY = Mathf.Min(visionGrid.height - 1, Mathf.CeilToInt((unitVision.transform.position.y + unitVision.range) / gridSize));

            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    Vector2 gridPosition = new Vector2(i, j) * gridSize;
                    float distance = Vector2.Distance(unitVision.transform.position, gridPosition);
                    if (distance <= unitVision.range)
                    {
                        visionGrid.SetVisible(i, j, unitVision.players);
                    }
                }
            }
        }
        //渲染
        RenderVision();
    }

    private Texture2D preTex;

    private void RenderVision()
    {
        preTex = renderTexture;
        for (int i = 0; i < visionGrid.width; i++)
        {
            for (int j = 0; j < visionGrid.height; j++)
            {
                Color pixelColor = hideColor;

                //检查该网格部分是否可见
                if (visionGrid.IsVisible(i, j, 1)) //假设只有一个玩家
                {
                    visionGrid.values[i + j * visionGrid.width] = 0;
                    pixelColor = canSeeColor;
                }
                //检查该网格部分是否被访问过
                else if (visionGrid.WasVisible(i, j, 1)) //假设只有一个玩家
                {
                    pixelColor = visiteColor;
                }

                //设置renderTexture的像素颜色
                renderTexture.SetPixel(i, j, pixelColor);
            }
        }

        sr.material.SetTexture("_LastTex", preTex);
        //应用设置的像素颜色
        renderTexture.Apply();
    }
}
