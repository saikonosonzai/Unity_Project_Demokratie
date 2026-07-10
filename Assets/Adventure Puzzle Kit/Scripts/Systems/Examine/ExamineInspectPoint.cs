using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.ExamineSystem
{
    public class ExamineInspectPoint : MonoBehaviour
    {
        [Header("Inspect Point Details")]
        [TextArea]
        [SerializeField] private string inspectDescription = ""; // Text shown when the player hovers over or inspects this point

        [Header("Click Event")]
        [SerializeField] private UnityEvent specialInteraction = null; // Optional UnityEvent to invoke when this point is clicked

        public void InspectPointInteract()
        {
            // Triggers the UnityEvent assigned in the Inspector — 
            // this can be used to perform custom actions like unlocking secrets, playing sounds, triggering animations, etc.
            specialInteraction.Invoke();
        }

        public string InspectInformation()
        {
            // Returns the inspect point description text
            return inspectDescription;
        }
    }
}
