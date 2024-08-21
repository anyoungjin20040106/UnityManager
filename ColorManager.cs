using System;
using System.Collections;
using UnityEngine;

public class ColorManager
{
    private float _r, _g, _b, _a;
    public ColorManager(){
        r=255;
        g=255;
        b=255;
        a=255;
    }
    public ColorManager(float r,float g,float b){
        this.r=r;
        this.g=g;
        this.b=b;
        this.a=255f;
    }
    public ColorManager(float r,float g,float b,float a){
        this.r=r;
        this.g=g;
        this.b=b;
        this.a=a;
    }
    public float r
    {
        set
        {
            if (value >= 0 && value <= 255)
                _r = value / 255f;
        }
        get
        {
            return _r * 255f;
        }
    }

    public float g
    {
        set
        {
            if (value >= 0 && value <= 255)
                _g = value / 255f;
        }
        get
        {
            return _g * 255f;
        }
    }

    public float b
    {
        set
        {
            if (value >= 0 && value <= 255)
                _b = value / 255f;
        }
        get
        {
            return _b * 255f;
        }
    }

    public float a
    {
        set
        {
            if (value >= 0 && value <= 255)
                _a = value / 255f;
        }
        get
        {
            return _a * 255f;
        }
    }

    public Color color
    {
        get
        {
            return new Color(_r, _g, _b, _a);
        }
        set
        {
            r = value.r * 255f;
            g = value.g * 255f;
            b = value.b * 255f;
            a = value.a * 255f;
        }
    }

    public string hex
    {
        get
        {
            return $"#{Mathf.RoundToInt(_r * 255f):X2}{ Mathf.RoundToInt(_g * 255f):X2}{Mathf.RoundToInt(_b * 255f):X2}";
        }
        set
        {
            try
            {

                int append = value.StartsWith("#") ? 1 : 0;
                r = Convert.ToInt32(value.Substring(append, 2), 16);
                g = Convert.ToInt32(value.Substring(2+append, 2), 16);
                b = Convert.ToInt32(value.Substring(4+append, 2), 16);
            }catch(Exception e){
                Debug.LogError(e.Message);
            }
        }
    }
    public IEnumerator Spoid(){
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();
        Vector3 mpos=Input.mousePosition;
        this.color=tex.GetPixel((int)mpos.x, (int)mpos.y);
    }
}
