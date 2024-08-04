using UnityEngine;
using UnityEngine.Rendering;

public class LightProbeAdjuster : MonoBehaviour
{
    public float intensityMultiplier = 0.5f;

    void Start()
    {
        LightProbes lightProbes = LightmapSettings.lightProbes;
        SphericalHarmonicsL2[] probes = lightProbes.bakedProbes;

        for (int i = 0; i < probes.Length; i++)
        {
            for (int j = 0; j < 3; j++) // RGB channels
            {
                for (int k = 0; k < 9; k++) // Coefficients
                {
                    probes[i][j, k] *= intensityMultiplier;
                }
            }
        }

        lightProbes.bakedProbes = probes;
    }
}
