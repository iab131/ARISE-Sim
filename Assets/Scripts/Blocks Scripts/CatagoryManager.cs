using UnityEngine;
using UnityEngine.UI;
public class CategoryManager : MonoBehaviour
{
    public RectTransform motionText;
    public RectTransform motorsText;
    public RectTransform eventsText;
    public RectTransform controlText;
    public RectTransform sensorsText;

    public RectTransform content;
    public float offset = 47.5f;

    public void SetContentY(RectTransform target)
    {
        Vector2 pos = content.anchoredPosition;
        pos.y = -target.anchoredPosition.y - offset;
        content.anchoredPosition = pos;
    }

    public void MotionHeight() => SetContentY(motionText);
    public void MotorsHeight() => SetContentY(motorsText);
    public void EventsHeight() => SetContentY(eventsText);
    public void ControlHeight() => SetContentY(controlText);
    public void SensorsHeight() => SetContentY(sensorsText);

}
