﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct RandomGenerator : IComponentData
{
    public Unity.Mathematics.Random Value;
}

