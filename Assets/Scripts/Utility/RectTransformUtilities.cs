using UnityEngine;

public static class RectTransformUtilities
{
    public static Vector2 GetMinVector2(this RectTransform recTrans)
    {
        Vector2 min = recTrans.anchorMin;
        min.x *= Screen.width;
        min.y *= Screen.height;

        min += recTrans.offsetMin;
        return min;
    }

    public static Vector2 GetMaxVector2(this RectTransform recTrans)
    {
        Vector2 max = recTrans.anchorMax;
        max.x *= Screen.width;
        max.y *= Screen.height;

        max += recTrans.offsetMax;
        return max;
    }
}
