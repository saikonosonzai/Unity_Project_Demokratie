using UnityEngine;

namespace AdventurePuzzleKit
{
    // Handles enabling/disabling light GameObjects and toggling emissive materials
    public class ActivateLights : MonoBehaviour
    {
        [Header("Array of Mesh Renderers for emissive materials")]
        [SerializeField] private Renderer[] thisMaterial = null; // Materials we want to enable/disable emission on

        private string emissionName = "_EMISSION"; // Name of the emission keyword used in shaders

        [SerializeField] private GameObject[] lights = null; // Lights to toggle on/off

        // Turns on all lights and enables emission on specified materials
        public void PowerLights()
        {
            foreach (GameObject lightObjects in lights)
            {
                lightObjects.SetActive(true); // Enable each light GameObject
            }

            foreach (Renderer emissiveMaterial in thisMaterial)
            {
                emissiveMaterial.material.EnableKeyword(emissionName); // Enable emission on each material
            }
        }

        // Turns off all lights and disables emission on specified materials
        public void DeactivateLights()
        {
            foreach (GameObject lightObjects in lights)
            {
                lightObjects.SetActive(false); // Disable each light GameObject
            }

            foreach (Renderer emissiveMaterial in thisMaterial)
            {
                emissiveMaterial.material.DisableKeyword(emissionName); // Disable emission on each material
            }
        }
    }
}
