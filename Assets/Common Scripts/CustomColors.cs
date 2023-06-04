using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class CustomColors
{
    private const float Alpha1 = 0.5f;
    private const float Alpha2 = 0.3f;




    public static Color alphaBlue = new Color(0f, 0f, 1f, Alpha1);
    public static Color alpha2Blue = new Color(0f, 0f, 1f, Alpha2);
    public static Color alphaRed = new Color(1f, 0f, 0f, Alpha1);
    public static Color alpha2Red = new Color(1f, 0f, 0f, Alpha2);
    public static Color alphaWhite = new Color(1f, 1f, 1f, Alpha1);
    public static Color alphaGreen = new Color(0f, 1f, 0f, Alpha1);
    public static Color alphaCyan = new Color(0f, 1f, 1f, Alpha1);
}

