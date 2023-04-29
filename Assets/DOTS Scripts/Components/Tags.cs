using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct AliveTankTag : IComponentData, IEnableableComponent { }
public struct StandbyTankTag : IComponentData, IEnableableComponent { }
public struct TankCleanup : ICleanupComponentData { }

