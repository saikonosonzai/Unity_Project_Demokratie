/// This script has a custom editor called "AKItemEditor", found in the "Editor" folder. You will need to add new properties to this if you create new variables / fields in this script.
/// Contact me if you have any troubles at all!

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using AdventurePuzzleKit.ExamineSystem;
using AdventurePuzzleKit.FlashlightSystem;
using AdventurePuzzleKit.GasMaskSystem;
using AdventurePuzzleKit.GeneratorSystem;
using AdventurePuzzleKit.KeypadSystem;
using AdventurePuzzleKit.PadlockSystem;
using AdventurePuzzleKit.PhoneSystem;
using AdventurePuzzleKit.ThemedKey;
using AdventurePuzzleKit.ChessSystem;
using AdventurePuzzleKit.SafeSystem;
using AdventurePuzzleKit.FuseSystem;
using AdventurePuzzleKit.NoteSystem;
using AdventurePuzzleKit.LeverSystem;
using AdventurePuzzleKit.ValveSystem;
using AdventurePuzzleKit.KeycardSystem;

namespace AdventurePuzzleKit
{
    public class AKItem : MonoBehaviour
    {
        public enum SystemType { None, GeneratorSys, ValveSys, FlashlightSys, GasMaskSys, KeypadSys, PhoneSys, SafeSys, FuseBoxSys, PadlockSys, LeverSys, ThemedKeySys,
        ChessSys, DoorSys, NoteSys, KeycardSys, ClockSys, HatchSys, StatueSys }
        [SerializeField] private SystemType _systemType = SystemType.None;

        [Tooltip("This is to add a highlight name when looking at objects, ONLY if you're not using Examine Sys as Primary Type")]
        [SerializeField] private string highlightName = "Change Name Here";  // Name to display
        [SerializeField] private bool showNameHighlight = false; // Enable/disable highlighting
        [SerializeField] private bool showInteractPrompt = false; // Show interact prompt (e.g: "Press E to interact")
        [SerializeField] private bool showPickupPrompt = false; // Show pickup prompt (e.g: "Press E to pickup")
        [SerializeField] private bool showExaminePrompt = false; // Show examine prompt (e.g: "Press Q to pickup")

        //Emission highlight
        [SerializeField] private bool showEmissionHighlight = false;
        [SerializeField] private bool isEmptyParent = false; // For items with no MeshRenderer
        [SerializeField] private bool includeChildrenWithParentMesh = false; // NEW

        private GeneratorItem _generatorItem;
        private ValveItem _valveItem;
        private FlashlightItem _flashlightItem;
        private GasMaskItem _gasMaskItem;
        private KeypadItem _keyPadItem;
        private PhoneItem _phoneItem;
        private SafeItem _safeItem;
        private FuseItem _fuseItem;
        private PadlockItem _padlockItem;
        private LeverItem _leverItem;
        private TKItem _tkItem;
        private ChessItem _chessItem;
        private ExaminableItem _examinableItem;
        private DoorController _doorController;
        private NoteTypeSelector _noteItem;
        private KeycardItem _keycardItem;
        private ClockItem _clockItem;
        private HatchItem _hatchItem;
        private Statue_Item _statue_Item;

        private List<Renderer> childObjects = null; // Automatically populated in Awake()
        private Material parentMaterial;
        private const string emissiveKeyword = "_EMISSION";

        public string HighlightName => highlightName;
        public bool ShowEmissionHighlight => showEmissionHighlight;
        public bool ShowInteractPrompt => showInteractPrompt;
        public bool ShowExaminePrompt => showExaminePrompt;
        public bool IsEmptyParent => isEmptyParent;
        
        public bool ShowNameHighlight 
        { 
            get => showNameHighlight; 
            set => showNameHighlight = value; 
        }

        private void Awake()
        {
            // Initialize subsystems and log if missing
            CheckAndAssignComponent(ref _generatorItem, SystemType.GeneratorSys, "GeneratorItem");
            CheckAndAssignComponent(ref _valveItem, SystemType.ValveSys, "ValveItem");
            CheckAndAssignComponent(ref _flashlightItem, SystemType.FlashlightSys, "FlashlightItem");
            CheckAndAssignComponent(ref _gasMaskItem, SystemType.GasMaskSys, "GasMaskItem");
            CheckAndAssignComponent(ref _keyPadItem, SystemType.KeypadSys, "KeypadItem");
            CheckAndAssignComponent(ref _phoneItem, SystemType.PhoneSys, "PhoneItem");
            CheckAndAssignComponent(ref _safeItem, SystemType.SafeSys, "SafeItem");
            CheckAndAssignComponent(ref _fuseItem, SystemType.FuseBoxSys, "FuseItem");
            CheckAndAssignComponent(ref _padlockItem, SystemType.PadlockSys, "PadlockItem");
            CheckAndAssignComponent(ref _leverItem, SystemType.LeverSys, "LeverItem");
            CheckAndAssignComponent(ref _tkItem, SystemType.ThemedKeySys, "TKItem");
            CheckAndAssignComponent(ref _chessItem, SystemType.ChessSys, "ChessItem");
            CheckAndAssignComponent(ref _doorController, SystemType.DoorSys, "DoorController");
            CheckAndAssignComponent(ref _noteItem, SystemType.NoteSys, "NoteItem");
            CheckAndAssignComponent(ref _keycardItem, SystemType.KeycardSys, "KeycardItem");
            CheckAndAssignComponent(ref _clockItem, SystemType.ClockSys, "ClockItem");
            CheckAndAssignComponent(ref _hatchItem, SystemType.HatchSys, "HatchItem");
            CheckAndAssignComponent(ref _statue_Item, SystemType.StatueSys, "Statue_Item");


            // Check for parent renderer and assign material
            if (TryGetComponent(out Renderer renderer))
            {
                parentMaterial = renderer.material;
            }
            else if (DebugSettings.EnableDebugLogs)
            {
                Debug.LogWarning($"No Renderer found on {gameObject.name}. Material assignment skipped.");
            }

            // Dynamically find `ExaminableItem` script if it exists
            TryGetComponent(out _examinableItem);

            // Populate childObjects based on settings
            PopulateChildObjects();
        }

        //Checks for a component of a given type, assigns it to the field, and logs a warning if missing.
        private void CheckAndAssignComponent<T>(ref T componentField, SystemType expectedSystemType, string componentName) where T : Component
        {
            if (_systemType == expectedSystemType)
            {
                if (!TryGetComponent(out componentField))
                {
                    if(DebugSettings.EnableDebugLogs)
                    Debug.LogWarning($"GameObject '{gameObject.name}' is set to {expectedSystemType} but is missing a {componentName} component.");
                }
            }
        }

        private void PopulateChildObjects()
        {
            childObjects = new List<Renderer>();

            // If the parent has no MeshRenderer, find all children with MeshRenderers
            if (isEmptyParent)
            {
                childObjects.AddRange(GetComponentsInChildren<Renderer>(includeInactive: true)
                    .Where(r => r.gameObject != gameObject)); // Exclude self
            }
            // If the parent has a MeshRenderer and children should also be highlighted
            else if (includeChildrenWithParentMesh)
            {
                childObjects.AddRange(GetComponentsInChildren<Renderer>(includeInactive: true));
            }
        }

        public void ToggleEmission(bool enable)
        {
            if (!showEmissionHighlight) return;

            // Highlight parent material if applicable
            if (parentMaterial != null)
            {
                SetEmission(parentMaterial, enable);
            }

            // Only iterate through child objects if necessary
            if (isEmptyParent || includeChildrenWithParentMesh)
            {
                foreach (var child in childObjects)
                {
                    SetEmission(child.material, enable);
                }
            }
            else if (DebugSettings.EnableDebugLogs)
            {
                Debug.Log($"Skipping child emission highlighting for {gameObject.name} because 'isEmptyParent' and 'includeChildrenWithParentMesh' are both false.");
            }
        }

        private void SetEmission(Material material, bool enable)
        {
            if (material == null) return;

            if (enable)
            {
                material.EnableKeyword(emissiveKeyword);
            }
            else
            {
                material.DisableKeyword(emissiveKeyword);
            }
        }

        public void ToggleHighlight(bool enable)
        {
            if (enable)
                AKUIManager.instance.SetHighlightName(HighlightName, true, ShowNameHighlight, showInteractPrompt, showPickupPrompt, showExaminePrompt);
            else
                AKUIManager.instance.SetHighlightName(null, false, false, false, false, false);
        }

        public void StartLooking()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log($"Started looking at {gameObject.name}");


            // Show item's name if highlighting is enabled
            if (ShowNameHighlight) ToggleHighlight(true);

            if (showEmissionHighlight) ToggleEmission(true);

            GetActiveInteractable()?.StartLooking();
        }

        public void StopInteraction()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log($"Stopped interacting with {gameObject.name}");

            GetActiveInteractable()?.StopInteraction();

            // Disable item's name if highlighting is enabled
            if (ShowNameHighlight) ToggleHighlight(false);
            if (showEmissionHighlight) ToggleEmission(false);
        }

        public void HandleInputClick()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log($"Interacting with the {gameObject.name}");

            GetActiveInteractable()?.HandleInputClick();
        }

        public void HandleExamine()
        {
            if (_examinableItem != null)
            {
                _examinableItem.HandleInputClick();
            }
            else
            {
                if (DebugSettings.EnableDebugLogs)
                    Debug.LogWarning($"No ExaminableItem script found on {gameObject.name}");
            }
        }

        public void HandleInputHold()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log($"Holding interaction with the {gameObject.name}");

            GetActiveInteractable()?.HandleInputHold();
        }

        public void HandleInputStop()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log($"Stopped interaction with the {gameObject.name}");

            GetActiveInteractable()?.HandleInputStop();
        }

        private IInteractable GetActiveInteractable()
        {
            // Return the active subsystem based on the current system type
            return _systemType switch
            {
                SystemType.GeneratorSys => _generatorItem,
                SystemType.ValveSys => _valveItem,
                SystemType.FlashlightSys => _flashlightItem,
                SystemType.GasMaskSys => _gasMaskItem,
                SystemType.KeypadSys => _keyPadItem,
                SystemType.PhoneSys => _phoneItem,
                SystemType.SafeSys => _safeItem,
                SystemType.FuseBoxSys => _fuseItem,
                SystemType.PadlockSys => _padlockItem,
                SystemType.LeverSys => _leverItem,
                SystemType.ThemedKeySys => _tkItem,
                SystemType.ChessSys => _chessItem,
                SystemType.DoorSys => _doorController,
                SystemType.NoteSys => _noteItem,
                SystemType.KeycardSys => _keycardItem,
                SystemType.ClockSys => _clockItem,
                SystemType.HatchSys => _hatchItem,
                SystemType.StatueSys => _statue_Item,
                _ => null
            };
        }
    }
}
