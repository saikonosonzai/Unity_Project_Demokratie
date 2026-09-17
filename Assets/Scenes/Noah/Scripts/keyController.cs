using AdventurePuzzleKit;
using UnityEngine;
using System.Collections;

public class keyController : MonoBehaviour
{
    public BremenDialogueData[] BremenDialogueData;

    [Header("Key")]
    [SerializeField] private int keyNumber;

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

            BremenDialogueManager.Instance.StartDialogue(bd);

            yield return new WaitUntil(() =>
                !BremenDialogueManager.Instance.IsDialogueActive
            );
        }

        GameStateManager.Instance.CollectKey(keyNumber);

        gameObject.SetActive(false);
    }
}