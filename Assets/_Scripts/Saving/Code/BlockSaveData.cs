using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BlockSaveData
{
    public string prefabName;
    public SerializableVector2 position;
    public Dictionary<string, string> inputs = new();
    public BlockSaveData nextBlock; // for vertical chaining
}

[System.Serializable]
public struct SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public SerializableVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2 ToVector2() => new Vector2(x, y);
}
