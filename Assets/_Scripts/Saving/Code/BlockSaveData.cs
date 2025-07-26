using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BlockSaveData
{
    public string prefabName;
    public Vector2 position;
    public Dictionary<string, string> inputs = new();
    public BlockSaveData nextBlock; // for vertical chaining
}
