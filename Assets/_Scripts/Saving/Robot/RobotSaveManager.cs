using SFB;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class RobotSaveManager : MonoBehaviour
{
    public GameObject robotRoot; // The top-level "Robot" GameObject
    public static RobotSaveManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        Instance = this;
    }
    public string SaveRobot()
    {
        Transform robot = robotRoot.transform.Find("Robot");
        if (robot == null)
            return "";
        RobotSaveData robotData = new RobotSaveData();
        foreach (Transform group in robot)
        {
            GroupSaveData groupData = new GroupSaveData
            { 
                groupName = group.name,
                position = group.localPosition,
                rotation = group.localRotation,
            };


            foreach (Transform part in group)
            {
                if (part.parent == group)
                {
                    groupData.parts.Add(SavePart(part));
                }
            }

            robotData.groups.Add(groupData);
        }

        return JsonUtility.ToJson(robotData, true);
    }


    private PartSaveData SavePart(Transform part)
    {
       


        PartSaveData data = new PartSaveData
        {
            prefabName = part.name.Replace("(Clone)", "").Trim(),
            position = part.localPosition,
            rotation = part.localRotation,
            isMotorHub = false
        };

        // Check for a MotorHub child
        Transform motorHub = part.Find("MotorHub");
        if (motorHub != null)
        {
            data.isMotorHub = true;

            foreach (Transform child in motorHub)
            {
                if (LayerMask.LayerToName(child.gameObject.layer) == "Parts")
                data.childParts.Add(SavePart(child)); // not recursive (wheels under motorhub are flat)
            }
        }

        return data;
    }


    private PartSaveData SavePartRecursive(Transform motor)
    {
        PartSaveData data = SavePart(motor);
        data.isMotorHub = true;

        foreach (Transform child in motor)
        {
            if (child != motor) // Skip self
                data.childParts.Add(SavePartRecursive(child));
        }

        return data;
    }

    public void SaveRobotToFile()
    {
        string json = SaveRobot();
        string encrypted = JsonEncryptor.Encrypt(json);

#if UNITY_WEBGL && !UNITY_EDITOR
    WebGLFileDownloader.DownloadJson("robot-save.json", encrypted);
#else
        var path = StandaloneFileBrowser.SaveFilePanel("Save Robot", "", "MyRobot", "fllrobot");
        if (!string.IsNullOrEmpty(path))
        {
            if (!path.EndsWith(".fllrobot"))
                path += ".fllrobot";
            //Debug.Log(json);
            File.WriteAllText(path, encrypted);
            Debug.Log("Saved to: " + path);
        }
#endif
    }

}
