using UnityEngine;

public static class SimRobotManager
{

    public static Vector3 robotRegPos;
    //public static GameObject SimRobot;
    public static void SpawnSimulationRobot(
        NavBarController.View targetView,
        GameObject buildingRoot,
        GameObject simulationRoot,
        GameObject arRoot)
    {
        Transform robot = FindRobot(buildingRoot.transform);
        if (robot == null) return;

        Transform groupToCopy = null;
        foreach (Transform group in robot)
        {
            foreach (Transform child in group.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "ControlHub")
                {
                    groupToCopy = group;
                    break;
                }
            }
            if (groupToCopy != null) break;
        }

        if (groupToCopy == null)
        {
            Debug.LogWarning("No group with ControlHub found under robot.");
            return;
        }

        Transform parent = (targetView == NavBarController.View.Simulation)
            ? simulationRoot.transform : arRoot.transform;

        Transform existing = FindRobot(parent);
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        GameObject groupCopy = Object.Instantiate(groupToCopy.gameObject, parent);
        //SimRobot = groupCopy;
        groupCopy.name = "Robot";
        groupCopy.transform.localPosition = groupToCopy.localPosition;
        groupCopy.transform.localRotation = groupToCopy.localRotation;
        groupCopy.transform.localScale = groupToCopy.localScale;
        CenterGroupOnChildren(groupCopy);

        // Adjust height if intersecting ground
        float groundY = 0f;
        Transform ground = simulationRoot.transform.Find("Plane");
        if (ground != null) groundY = ground.position.y;

        Bounds bounds = GetBounds(groupCopy.transform);
        float offset = groundY - bounds.min.y;
        if (offset > 0f)
        {
            groupCopy.transform.position += new Vector3(0, offset, 0);
            robotRegPos = groupCopy.transform.position;
        }

        Rigidbody rb = groupCopy.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.None;

        SimMotor.SetConnectedBodyForAll(rb);

        if (targetView == NavBarController.View.Simulation)
        {
            SimCameraController simCam = simulationRoot.GetComponentInChildren<SimCameraController>(true);
            if (simCam != null)
            {
                simCam.target = groupCopy.transform;
            }
        }
    }
    public static void CenterGroupOnChildren(GameObject group)
    {
        if (group.transform.childCount == 0) return;

        Vector3 avg = Vector3.zero;
        foreach (Transform child in group.transform)
            avg += child.position;

        avg /= group.transform.childCount;

        // Move group to center
        Vector3 offset = group.transform.position - avg;
        group.transform.position = avg;

        // Keep children in the same world positions
        foreach (Transform child in group.transform)
            child.position += offset;
    }

    public static Transform FindRobot(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Robot"))
                return child;
        }
        return null;
    }

    private static Bounds GetBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.zero);

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }
        return combinedBounds;
    }
}
