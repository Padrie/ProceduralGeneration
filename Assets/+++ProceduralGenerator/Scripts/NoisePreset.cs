using UnityEngine;

[CreateAssetMenu(menuName = "Noise Preset/Terrain Data")]
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