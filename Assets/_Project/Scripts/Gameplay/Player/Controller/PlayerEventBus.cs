using UnityEngine;
using System;

public class PlayerEventBus
{
    private PlayerEventBus() { }

    private static PlayerEventBus _instance;
    public static PlayerEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PlayerEventBus();
            }
            return _instance;
        }
    }

    public Action OnJumpAbilityRequested;
    public Action OnSpellCastRequested;
    public Action<Vector3> OnAirWaveCast;
    public Action<Vector3> OnIcePlatformCreated;
}