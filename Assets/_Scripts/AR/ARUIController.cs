using UnityEngine;

public class ARUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

#if UNITY_IOS || UNITY_ANDRIOD
    Destroy(gameObject);
#endif
    }
}
