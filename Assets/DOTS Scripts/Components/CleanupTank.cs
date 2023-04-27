using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

public struct CleanupTank : ICleanupComponentData
{
}
public struct AliveTankTag : IComponentData, IEnableableComponent { }
