using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public struct TankSpawnFieldModel
{
    public const string EventName = "spawnField";
    public int elapsedTimeMs, tanksCount;
    public string team;
    public bool DOTS;

    public IDictionary<string, object> GetEventParams()
    {
        return new Dictionary<string, object>()
        {
            { nameof(elapsedTimeMs), elapsedTimeMs },
            { nameof(tanksCount), tanksCount },
            { nameof(team), team },
            { nameof(DOTS), DOTS },
        };
    }
   

    
}

