using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// https://forum.unity.com/threads/contribution-texture2d-blur-in-c.185694/#post-1637841
public class Blur : MonoBehaviour
{
    static float avg;
    static float blurPixelCount = 0;

    public static Texture2D FasterBlur(Texture2D image, int radius, int iterations)
    {
        var task = Addressables.LoadAssetAsync<ComputeShader>("Assets/Scripts/Blur.compute");
        var shader = task.WaitForCompletion();

        int kernelHandle = shader.FindKernel("CSMain");
        RenderTexture tex = new RenderTexture(image.width, image.height, 0);
        tex.enableRandomWrite = true;
        tex.Create();

        Texture2D output = new Texture2D(image.width, image.height);
        Graphics.CopyTexture(image, output);
        shader.SetTexture(kernelHandle, "Result", tex);
        shader.SetInt("blurSize", radius);
        for (var i = 0; i < iterations; i++)
        {
            shader.SetTexture(kernelHandle, "Input", output);
            shader.SetBool("horizontal", true);
            shader.Dispatch(kernelHandle, 1028, 1028, 1);
            RenderTexture.active = tex;
            output.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            output.Apply();
            shader.SetTexture(kernelHandle, "Input", output);
            shader.SetBool("horizontal", false);
            shader.Dispatch(kernelHandle, 1028, 1028, 1);
            RenderTexture.active = tex;
            output.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            output.Apply();
        }
        return output;
    }

    public static Texture2D FastBlur(Texture2D image, int radius, int iterations)
    {
        Texture2D tex = image;

        for (var i = 0; i < iterations; i++)
        {
            tex = BlurImage(tex, radius, true);
            tex = BlurImage(tex, radius, false);
        }
        return tex;
    }

    static Texture2D BlurImage(Texture2D image, int blurSize, bool horizontal)
    {

        Texture2D blurred = new Texture2D(image.width, image.height);
        int _W = image.width;
        int _H = image.height;
        int xx, yy, x, y;

        if (horizontal)
        {
            for (yy = 0; yy < _H; yy++)
            {
                for (xx = 0; xx < _W; xx++)
                {
                    ResetPixel();

                    //Right side of pixel
                    for (x = xx; x < xx + blurSize && x < _W; x++)
                        AddPixel(image.GetPixel(x, yy));

                    //Left side of pixel
                    for (x = xx; x > xx - blurSize && x > 0; x--)
                        AddPixel(image.GetPixel(x, yy));

                    CalcPixel();

                    blurred.SetPixel(xx, yy, new Color(0, 0, 0, avg));
                }
            }
        }

        else
        {
            for (xx = 0; xx < _W; xx++)
            {
                for (yy = 0; yy < _H; yy++)
                {
                    ResetPixel();

                    //Over pixel
                    for (y = yy; (y < yy + blurSize && y < _H); y++)
                        AddPixel(image.GetPixel(xx, y));
                    //Under pixel

                    for (y = yy; (y > yy - blurSize && y > 0); y--)
                        AddPixel(image.GetPixel(xx, y));

                    CalcPixel();

                    blurred.SetPixel(xx, yy, new Color(0, 0, 0, avg));
                }
            }
        }

        blurred.Apply();
        return blurred;
    }
    static void AddPixel(Color pixel)
    {
        avg += pixel.a;
        blurPixelCount++;
    }

    static void ResetPixel()
    {
        avg = 0.0f;
        blurPixelCount = 0;
    }

    static void CalcPixel() => avg = avg / blurPixelCount;
}
