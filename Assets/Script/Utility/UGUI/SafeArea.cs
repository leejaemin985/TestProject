using UnityEngine;

public class SafeArea : MonoBehaviour
{
    public static ScreenOrientation ScreenOrientation { get; set; } = ScreenOrientation.LandscapeRight;

    private enum Direction
    {
        Both,
        BottomOnly, // Landscape일 때는 오른쪽
        TopOnly,    // Ladnscape일 때는 왼쪽
    }

    private RectTransform rectTransform;
    private Rect safeArea;

    private Vector2 minAnchor = Vector2.zero;  // safe area의 왼쪽 아래 
    private Vector2 maxAnchor = Vector2.zero;  // safe area의 오른쪽 위 

    [SerializeField] private Direction applyDirection = Direction.Both;


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        safeArea = Screen.safeArea;

        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        //스크린 사이즈 기준으로 safe area까지 비율 설정
        minAnchor.x /= UnityEngine.Device.Screen.width;
        minAnchor.y /= UnityEngine.Device.Screen.height;
        maxAnchor.x /= UnityEngine.Device.Screen.width;
        maxAnchor.y /= UnityEngine.Device.Screen.height;

        switch (ScreenOrientation)
        {
            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
                if (applyDirection == Direction.BottomOnly || applyDirection == Direction.Both)
                    rectTransform.anchorMin = minAnchor;
                if (applyDirection == Direction.TopOnly || applyDirection == Direction.Both)
                    rectTransform.anchorMax = maxAnchor;
                return;
            default:
                break;
        }

        var tmpMinAnchor = minAnchor;
        var tmpMaxAnchor = maxAnchor;
        switch (ScreenOrientation)
        {
            default:
            case ScreenOrientation.LandscapeLeft:
                minAnchor = new Vector2(tmpMinAnchor.x, 0f);
                maxAnchor = new Vector2(tmpMaxAnchor.x, 1f);
                break;
            case ScreenOrientation.LandscapeRight:
                minAnchor = new Vector2(1f - tmpMaxAnchor.x, 0f);
                maxAnchor = new Vector2(1f - tmpMinAnchor.x, 1f);
                break;
        }

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
    }
}