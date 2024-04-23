using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu]
public class NoisePreset : ScriptableObject
{
    public NoiseSettings noiseSettings;
    
    public void OnValidate()
    {
        if (MapDisplay.Instance != null)
        {
            MapDisplay.Instance.OnValidate();
        }
        else
        {
            Debug.LogError("MapDisplay instance is null!");
        }
    }
}