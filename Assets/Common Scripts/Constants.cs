using UnityEngine;

public static class Constants
{
    public static string DeviceName = $"{SystemInfo.processorType} -- {SystemInfo.graphicsDeviceName}";

    //Constroles
    public const string InputStart = "Start";
    public const string InputPreset1 = "Preset 1";
    public const string InputPreset2 = "Preset 2";
    public const string InputPreset3 = "Preset 3";
    public const string InputPreset4 = "Preset 4";
    public const string InputSpawnGreen = "Spawn Green";
    public const string InputSpawnRed = "Spawn Red";
    public const string InputRotateCam = "Rotate Cam";
    public const string InputHorizontal = "Horizontal";
    public const string InputFoward = "Foward";

    //Layers
    public const string LayerTank = "Tank";

    //Tags
    public const string TagRedTeam = "Red Team";
    public const string TagGreenTeam = "Green Team";

    //public static uint TankLayer = (uint) LayerMask.GetMask(Constants.LayerTank);

}

