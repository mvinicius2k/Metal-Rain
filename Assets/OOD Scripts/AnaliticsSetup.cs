using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public enum PcKind
{
    Desktop6600, Desktop750, NotebookHD, NotebookRyzen, Notebook1650
}

public class AnaliticsSetup : MonoBehaviour
{

    public static AnaliticsSetup Instance { get; private set; }
    public PcKind PcKind;
    public bool IsDOTS;

    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);

        var options = new InitializationOptions();
        options.SetEnvironmentName("dev");
        var username = System.Enum.GetName(typeof(PcKind), PcKind);
        UnityServices.ExternalUserId = username; ;
        await UnityServices.InitializeAsync(options);
    }
}
