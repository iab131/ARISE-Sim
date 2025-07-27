using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RobotSaveData
{
    public List<GroupSaveData> groups = new List<GroupSaveData>();
}

[System.Serializable]
public class GroupSaveData
{
    public string groupName;
    public List<PartSaveData> parts = new List<PartSaveData>();
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class PartSaveData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public bool isMotorHub;
    public List<PartSaveData> childParts = new List<PartSaveData>(); // used for MotorHubs
}
