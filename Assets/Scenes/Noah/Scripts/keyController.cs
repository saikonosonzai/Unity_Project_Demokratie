using AdventurePuzzleKit;
using UnityEngine;
using System.Collections;

public class keyController : MonoBehaviour
{
    public BremenDialogueData[] BremenDialogueData;

    public void OnTrigger()
    {
       

        StartCoroutine(PlayDialogues());
    }

    private IEnumerator PlayDialogues()
    {
        foreach (BremenDialogueData bd in BremenDialogueData)
        {
            if (bd == null)
                continue;

            // Dialog starten
            BremenDialogueManager.Instance.StartDialogue(bd);

            // Warten, bis der Dialog beendet wurde
            yield return new WaitUntil(() =>
                !BremenDialogueManager.Instance.IsDialogueActive
            );
        }
        gameObject.SetActive(false);
    }
}