using AdventurePuzzleKit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AdventurePuzzleKit
{
    public class Statue_Controller : MonoBehaviour
    {
        [Header("Rätsel-Einstellungen")]
        public bool isActive = false;
        public GameObject player;
        public Renderer statueRenderer;
        
        [Header("UI-Elemente")]
        public GameObject puzzleUI;           // Das gesamte UI Panel
        public SpriteRenderer[] slots;
        public Sprite[] symbols;
        
        [Header("Lösungs-Kombination (Indizes der Symbole)")]
        [SerializeField] private int[] correctCombination = { 2, 0, 3 };
        
        // Privates Variablen
        private int[] currentSelection;
        private int activeSlot = 0;
        private bool isSolved = false;
        private bool isWrong = false;
        private AKFPSController playerController;
        
        void Start()
        {
            // Prüfe ob alle Referenzen gesetzt sind
            if (!ValidateReferences())
                return;
            
            // Initialisiere currentSelection
            currentSelection = new int[slots.Length];
            for (int i = 0; i < currentSelection.Length; i++)
            {
                currentSelection[i] = 0;
            }
            
            // Player Controller einmal cachen
            if (player != null)
                playerController = player.GetComponent<AKFPSController>();
            
            // UI zu Beginn ausblenden
            if (puzzleUI != null)
                puzzleUI.SetActive(false);
            
            UpdateDisplay();
        }
        
        void Update()
        {
            if (isActive && !isSolved)
            {
                // Player Movement deaktivieren
                if (playerController != null)
                {
                    playerController.canMove = false;
                    playerController.canRotate = false;
                }
                
                // UI anzeigen wenn aktiv
                if (puzzleUI != null && !puzzleUI.activeSelf)
                {
                    puzzleUI.SetActive(true);
                    // Maus für UI-Interaktion freigeben
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                
                HandleInput();
            }
            
            // Mit Q kann jederzeit beendet werden
            if (Input.GetKeyDown(KeyCode.Q) && isActive)
            {
                ExitPuzzle();
            }
        }
        
        void HandleInput()
        {
            // Navigation zwischen Slots (Links/Rechts)
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                activeSlot = (activeSlot - 1 + slots.Length) % slots.Length;
                UpdateDisplay();
            }
            
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                activeSlot = (activeSlot + 1) % slots.Length;
                UpdateDisplay();
            }
            
            // Symbol ändern (Hoch/Runter)
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSelection[activeSlot] = (currentSelection[activeSlot] + 1) % symbols.Length;
                UpdateDisplay();
            }
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSelection[activeSlot] = (currentSelection[activeSlot] - 1 + symbols.Length) % symbols.Length;
                UpdateDisplay();
            }
            
            // Lösung prüfen (Enter)
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CheckSolution();
            }
        }
        
        void CheckSolution()
        {
            if (IsCorrect())
            {
                // Rätsel gelöst!
                Debug.Log("Rätsel gelöst!");
                statueRenderer.material.color = Color.green;
                isSolved = true;
                
                // Optional: Erfolgs-Effekt
                OnPuzzleSolved();
                
                // Rätsel nach 2 Sekunden automatisch beenden
                Invoke(nameof(ExitPuzzle), 2f);
            }
            else
            {
                // Falsche Lösung
                Debug.Log("Falsche Kombination! Versuche es erneut.");
                statueRenderer.material.color = Color.red;
                isWrong = true;
                
                // Nach kurzer Zeit zurücksetzen
                Invoke(nameof(ResetWrongAnswer), 0.8f);
            }
        }
        
        bool IsCorrect()
        {
            if (currentSelection.Length != correctCombination.Length)
                return false;
            
            for (int i = 0; i < correctCombination.Length; i++)
            {
                if (currentSelection[i] != correctCombination[i])
                    return false;
            }
            
            return true;
        }
        
        void ResetWrongAnswer()
        {
            // Farbe zurücksetzen
            if (statueRenderer != null)
                statueRenderer.material.color = Color.white;
            
            // Alle Slots auf Standardwert 0 setzen
            for (int i = 0; i < currentSelection.Length; i++)
            {
                currentSelection[i] = 0;
            }
            
            // Ersten Slot auswählen
            activeSlot = 0;
            isWrong = false;
            
            UpdateDisplay();
            Debug.Log("Rätsel wurde zurückgesetzt.");
        }
        
        void UpdateDisplay()
        {
            if (slots == null || symbols == null || currentSelection == null)
                return;
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (currentSelection[i] < symbols.Length && slots[i] != null)
                {
                    slots[i].sprite = symbols[currentSelection[i]];
                }
            }
        }
        
        void ExitPuzzle()
        {
            // Player Movement wieder aktivieren
            if (playerController != null)
            {
                playerController.canMove = true;
                playerController.canRotate = true;
            }
            
            isActive = false;
            
            // Maus wieder sperren für FPS-Spiel
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // UI ausblenden
            if (puzzleUI != null)
                puzzleUI.SetActive(false);
            
            // Wenn das Rätsel nicht gelöst wurde, Farbe zurücksetzen
            if (!isSolved)
            {
                if (statueRenderer != null)
                    statueRenderer.material.color = Color.white;
            }
            
            Debug.Log("Rätsel wurde verlassen");
        }
        
        // Wird von Statue_Item aufgerufen
        public void ActivatePuzzle()
        {
            print("test");
            if (!isSolved)
            {
                isActive = true;
                Debug.Log("Rätsel wurde aktiviert");
            }
            else
            {
                Debug.Log("Dieses Rätsel wurde bereits gelöst!");
            }
        }
        
        void OnPuzzleSolved()
        {
            // Hier kannst du zusätzliche Effekte einfügen:
            // - Sound abspielen
            // - Partikel-Effekt
            // - Tür öffnen
            // - Gegenstand geben etc.
            Debug.Log("Puzzle wurde gelöst! Belohnung wird aktiviert.");
        }
        
        private bool ValidateReferences()
        {
            bool isValid = true;
            
            if (player == null)
            {
                Debug.LogError("Spieler-Referenz fehlt in " + gameObject.name);
                isValid = false;
            }
            
            if (statueRenderer == null)
            {
                Debug.LogError("Statue Renderer fehlt in " + gameObject.name);
                isValid = false;
            }
            
            if (slots == null || slots.Length == 0)
            {
                Debug.LogError("Slots wurden nicht zugewiesen in " + gameObject.name);
                isValid = false;
            }
            
            if (symbols == null || symbols.Length == 0)
            {
                Debug.LogError("Symbole wurden nicht zugewiesen in " + gameObject.name);
                isValid = false;
            }
            
            if (puzzleUI == null)
            {
                Debug.LogWarning("PuzzleUI wurde nicht zugewiesen in " + gameObject.name);
            }
            
            return isValid;
        }
    }
}