using UnityEngine;
/// https://easings.net/
public class Easings
{
    public static float easeInOutCubic(float x) => x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
}
