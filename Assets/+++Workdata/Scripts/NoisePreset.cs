using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoisePreset : ScriptableObject
{
    public NoiseSettings noiseSettings;
    public MapDisplay mapDisplay;

    private void OnValidate()
    {
        
    }
}