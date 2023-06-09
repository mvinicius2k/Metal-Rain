﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class Helpers
{
    public static uint GetLayerMaskValue(in NativeArray<Layer> layers)
    {
        LayerMask layerMask = default;

        

        for (int i = 0; i < layers.Length; i++)
        {
            int layerIndex = (int)math.log2((int)layers[i]);
            layerMask.value |= 1 << layerIndex;
        }

        return (uint) layerMask.value;
    }
}