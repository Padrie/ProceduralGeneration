using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;

public class DiffusionLimitedAggregation : MonoBehaviour
{
    public enum SeedType
    {
        Dot,
        Line,
        Circle
    }

    public enum LineType
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public List<GameObject> treeList = new();
    public List<GameObject> walkerList = new();
    public Resolution[] resolution;
    public int whichCanvas = 0;

    [Header("Spawn Walkers From")] public bool top;
    public bool bottom;
    public bool left;
    public bool right;

    [Space(10)] public SeedType seedType;
    public LineType lineType;
    [Range(2, 32)] public int radius = 10;

    [Space(10)] public int failSafe = 0;
    public int maxLoops = 10000;
    [Space(10)] public bool showGrid;

    bool stuck = false;
    int boolAmount;

    Vector2Int canvas;
    int walkers;

    private void OnValidate()
    {
        if (whichCanvas <= 0)
            whichCanvas = 0;
        if (whichCanvas >= resolution.Length)
            whichCanvas = resolution.Length - 1;
        
        for (int i = 0; i < resolution.Length; i++)
        {
            walkers = resolution[whichCanvas].walker;
            canvas = resolution[whichCanvas].canvas;
        }
    }

    private void Update()
    {
        if (whichCanvas <= 0)
            whichCanvas = 0;
        if (whichCanvas >= resolution.Length)
            whichCanvas = resolution.Length - 1;

        for (int i = 0; i < resolution.Length; i++)
        {
            walkers = resolution[whichCanvas].walker;
            canvas = resolution[whichCanvas].canvas;
        }
    }

    [ButtonMethod]
    public void Generate()
    {
        Dictionary<Vector3, GameObject> positions = new Dictionary<Vector3, GameObject>();

        boolAmount = 0;
        boolAmount += Convert.ToInt32(top);
        boolAmount = top ? 1 : 0;
        boolAmount += Convert.ToInt32(bottom);
        boolAmount += Convert.ToInt32(left);
        boolAmount += Convert.ToInt32(right);

        Vector3 topLeft = new();
        Vector3 topRight = new();
        Vector3 bottomLeft = new();
        Vector3 bottomRight = new();

        if (top || left)
            topLeft = new Vector3(-canvas.x, 0, canvas.y);
        if (top || right)
            topRight = new Vector3(canvas.x, 0, canvas.y);
        if (bottom || left)
            bottomLeft = new Vector3(-canvas.x, 0, -canvas.y);
        if (bottom || right)
            bottomRight = new Vector3(canvas.x, 0, -canvas.y);

        SpawnSeed();

        stuck = false;
        for (int i = 0; i < walkers / boolAmount; i++)
        {
            if (top)
                walkerList.Add(SpawnWalker(topLeft, topRight, transform));
            if (bottom)
                walkerList.Add(SpawnWalker(bottomRight, bottomLeft, transform));
            if (left)
                walkerList.Add(SpawnWalker(bottomLeft, topLeft, transform));
            if (right)
                walkerList.Add(SpawnWalker(topRight, bottomRight, transform));
        }

        StartCoroutine(WalkerMovement());

        foreach (GameObject obj in treeList)
        {
            Vector3 position = new Vector3((int)obj.transform.position.x, (int)obj.transform.position.y,
                (int)obj.transform.position.z);

            if (positions.ContainsKey(position))
                DestroyImmediate(obj);
            else
                positions.Add(position, obj);
        }

        treeList.RemoveAll(obj => obj == null);
    }

    IEnumerator WalkerMovement()
    {
        stuck = false;

        while (!stuck && failSafe < maxLoops)
        {
            failSafe++;
            for (int i = 0; i < walkerList.Count; i++)
            {
                GameObject walker = walkerList[i];
                Vector3 walkerPos = walker.transform.position;

                bool attached = false;

                for (int j = 0; j < treeList.Count; j++)
                {
                    float dis = Vector3.Distance(walkerPos, treeList[j].transform.position);

                    if (dis <= 1 && failSafe <= maxLoops)
                    {
                        treeList.Add(walker);
                        walkerList.RemoveAt(i);
                        attached = true;
                        break;
                    }
                }

                if (!attached)
                {
                    Vector3 newPos = walkerPos + Move();
                    newPos.x = Clamp(newPos.x, -canvas.x, canvas.x);
                    newPos.z = Clamp(newPos.z, -canvas.y, canvas.y);
                    walker.transform.position = newPos;
                }
            }

            if (failSafe >= maxLoops || walkerList.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                    foreach (GameObject obj in walkerList)
                        DestroyImmediate(obj.gameObject);

                walkerList.RemoveAll(obj => obj == null);
                NextResolution();
                print("Finished");
            }

            yield return null;
        }
    }

    Vector3 Move()
    {
        int randomDir = Random.Range(1, 5);
        Vector3 dir = new Vector3(0, 0, 0);

        if (randomDir == 1)
            dir = Vector3.right;
        else if (randomDir == 2)
            dir = Vector3.left;
        else if (randomDir == 3)
            dir = Vector3.forward;
        else if (randomDir == 4)
            dir = Vector3.back;

        return dir;
    }

    public GameObject SpawnWalker(Vector3 first, Vector3 second, Transform parent)
    {
        GameObject walker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walker.transform.position =
            new Vector3((int)Random.Range(first.x, first.z), 0, (int)Random.Range(second.x, second.z));
        walker.transform.parent = parent;
        walker.name = "walker";
        return walker;
    }

    [ButtonMethod]
    public void SpawnSeed()
    {
        switch (seedType)
        {
            case SeedType.Dot:

                var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                a.transform.parent = transform;
                a.name = "centre";
                treeList.Add(a);

                break;

            case SeedType.Line:
                int sideAmount = 0;

                if (lineType == LineType.Bottom || lineType == LineType.Top)
                    sideAmount = canvas.x * 2 + 1;
                if (lineType == LineType.Left || lineType == LineType.Right)
                    sideAmount = canvas.y * 2 + 1;

                for (int i = 0; i < sideAmount; i++)
                {
                    if (lineType == LineType.Bottom)
                    {
                        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        b.transform.position = new Vector3(-canvas.x + i, 0, -canvas.y);
                        b.transform.parent = transform;
                        b.name = "Bottom Line Segment";
                        treeList.Add(b);
                    }

                    if (lineType == LineType.Top)
                    {
                        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        b.transform.position = new Vector3(-canvas.x + i, 0, canvas.y);
                        b.transform.parent = transform;
                        b.name = "Top Line Segment";
                        treeList.Add(b);
                    }

                    if (lineType == LineType.Left)
                    {
                        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        b.transform.position = new Vector3(-canvas.x, 0, canvas.y - i);
                        b.transform.parent = transform;
                        b.name = "Left Line Segment";
                        treeList.Add(b);
                    }

                    if (lineType == LineType.Right)
                    {
                        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        b.transform.position = new Vector3(canvas.x, 0, -canvas.y + i);
                        b.transform.parent = transform;
                        b.name = "Right Line Segment";
                        treeList.Add(b);
                    }
                }

                break;

            case SeedType.Circle:

                Dictionary<Vector3, GameObject> positions = new Dictionary<Vector3, GameObject>();
                List<GameObject> circleList = new List<GameObject>();

                int x, y;
                float angle = 0f;
                float interval = 0.01f;

                angle += interval;

                while (angle < 2 * PI)
                {
                    x = (int)(radius * Cos(angle));
                    y = (int)(radius * Sin(angle));

                    var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = new Vector3(x, 0, y);
                    c.transform.parent = transform;
                    c.name = "centre";
                    treeList.Add(c);
                    circleList.Add(c);

                    angle += interval;
                }

                foreach (GameObject obj in circleList)
                {
                    Vector3 position = new Vector3((int)obj.transform.position.x, (int)obj.transform.position.y,
                        (int)obj.transform.position.z);

                    if (positions.ContainsKey(position))
                        DestroyImmediate(obj);
                    else
                        positions.Add(position, obj);
                }

                treeList.RemoveAll(obj => obj == null);

                break;
        }
    }

    [ButtonMethod]
    public void Clear()
    {
        StopAllCoroutines();
        for (int i = 0; i < 100; i++)
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
                treeList.Clear();
                walkerList.Clear();
                failSafe = 0;
                whichCanvas = 0;
            }
    }

    [ButtonMethod]
    public void NextResolution()
    {
        if (whichCanvas == resolution.Length - 1 || failSafe >= maxLoops)
        {
            for (int i = 0; i < 10; i++)
                foreach (GameObject obj in walkerList)
                    DestroyImmediate(obj.gameObject);
            return;
        }

        whichCanvas++;
        Generate();
    }

    // [ButtonMethod]
    // public void ToImage()
    // { 
    //     GameObject a = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //     
    //     Texture2D texture2D = new Texture2D(150, 150);
    //     Color[] colorMap = new Color[150 * 150];
    //     texture2D.filterMode = FilterMode.Point;
    //     texture2D.wrapMode = TextureWrapMode.Clamp;
    //
    //     for (int y = 0; y < 150; y++)
    //     {
    //         for (int x = 0; x < 150; x++)
    //         {
    //             colorMap
    //         }
    //     }
    // }

    private void OnDrawGizmos()
    {
        if (showGrid)
        {
            #region Outline

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(canvas.x * 2 + 1, 0, canvas.y * 2 + 1));

            #endregion

            #region Spawn Visualizations

            Gizmos.color = Color.yellow;
            if (seedType == SeedType.Line)
            {
                if (lineType == LineType.Bottom)
                {
                    Gizmos.DrawLine(new Vector3(-canvas.x - 0.5f, 0, -canvas.y - 0.5f),
                        new Vector3(canvas.x + 0.5f, 0, -canvas.y - 0.5f));
                }

                else if (lineType == LineType.Top)
                {
                    Gizmos.DrawLine(new Vector3(-canvas.x - 0.5f, 0, canvas.y + 0.5f),
                        new Vector3(canvas.x + 0.5f, 0, canvas.y + 0.5f));
                }

                else if (lineType == LineType.Left)
                {
                    Gizmos.DrawLine(new Vector3(-canvas.x - 0.5f, 0, canvas.y + 0.5f),
                        new Vector3(-canvas.x - 0.5f, 0, -canvas.y - 0.5f));
                }

                else if (lineType == LineType.Right)
                {
                    Gizmos.DrawLine(new Vector3(canvas.x + 0.5f, 0, canvas.y + 0.5f),
                        new Vector3(canvas.x + 0.5f, 0, -canvas.y - 0.5f));
                }
            }

            if (seedType == SeedType.Circle)
            {
                Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), radius);
            }

            #endregion

            #region Grid

            Gizmos.color = Color.black;

            for (int i = 0; i < canvas.y * 2; i++)
                Gizmos.DrawLine(new Vector3(-canvas.x - 0.5f, 0, canvas.y - i - 0.5f),
                    new Vector3(canvas.x + 0.5f, 0, canvas.y - i - 0.5f));

            for (int j = 0; j < canvas.x * 2; j++)
                Gizmos.DrawLine(new Vector3(canvas.x - j - 0.5f, 0, -canvas.y - 0.5f),
                    new Vector3(canvas.x - j - 0.5f, 0, canvas.y + 0.5f));

            #endregion

            #region Grid Points

            Gizmos.color = Color.green;

            for (int x = 0; x < canvas.x * 2 + 1; x += 2)
            for (int y = 0; y < canvas.y * 2 + 1; y += 2)
                Gizmos.DrawLine(new Vector3(canvas.x - x, 0, canvas.y - y), new Vector3(canvas.x - x, .2f, canvas.y - y));

            #endregion
        }
    }
}

[Serializable]
public class Resolution
{
    public Vector2Int canvas;
    public int walker;
}