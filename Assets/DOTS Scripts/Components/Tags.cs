using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct AliveTankTag : IComponentData, IEnableableComponent { }

public struct TankCleanup : ICleanupComponentData { }
public struct RedTeamTag : IComponentData
{
    public bool ValueThatWillNeverBeUsed; //Bug? ComponentTag precisa de algum campo para ser verificavel se é válido ou nao dentro de aspect
}
public struct GreenTeamTag : IComponentData
{
    public bool ValueThatWillNeverBeUsed;
}

public struct StandbyTankTag : IComponentData, IEnableableComponent
{
    public bool ValueThatWillNeverBeUsed;
}