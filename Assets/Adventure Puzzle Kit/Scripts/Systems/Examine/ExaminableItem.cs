// Handles object examination logic: rotation, zoom, UI display, item collection, and inspect points
// Requires ExaminableItemEditor (in the Editor folder) to expose new fields in inspector

using UnityEngine;
using System.Collections;
using AdventurePuzzleKit.FlashlightSystem;
using AdventurePuzzleKit.GeneratorSystem;
using AdventurePuzzleKit.GasMaskSystem;
using AdventurePuzzleKit.ThemedKey;
using AdventurePuzzleKit.ChessSystem;
using AdventurePuzzleKit.FuseSystem;
using AdventurePuzzleKit.ValveSystem;
using AdventurePuzzleKit.KeycardSystem;
using TMPro;

namespace AdventurePuzzleKit.ExamineSystem
{
    public class ExaminableItem : MonoBehaviour, IInteractable
    {
        #region Offset Fields
        // Offset the position and rotation of the item when examined
        [SerializeField] private Vector3 initialRotationOffset = new Vector3(0, 0, 0);
        [Range(-1, 1)][SerializeField] private float horizontalOffset = 0;
        [Range(-1, 1)][SerializeField] private float verticalOffset = 0;
        #endregion

        #region Zoom Fields
        // Zoom settings and responsiveness while examining
        [SerializeField] private float smoothExamineSpeed = 0.2f;
        [SerializeField] private float initialZoom = 1f;
        [SerializeField] private Vector2 zoomRange = new Vector2(0.5f, 2f);
        [SerializeField] private float zoomSensitivity = 0.1f;
        #endregion

        #region Rotation Fields
        // Rotation speed and direction while rotating item
        [SerializeField] private float rotationSpeed = 5.0f;
        [SerializeField] private bool invertRotation = false;
        #endregion

        #region InspectPoint Fields
        // Optional inspectable hotspots on the object
        [SerializeField] bool _hasInspectPoints = false;
        [SerializeField] private GameObject[] inspectPoints = null;
        private LayerMask inspectPointLayer;
        private float viewDistance = 50;
        private bool disableInspectInput = false;
        #endregion

        #region Sound Fields
        // Pickup and drop sound when examining or collecting
        [SerializeField] private Sound pickupSound = null;
        [SerializeField] private Sound dropSound = null;
        #endregion

        #region Text Customisation Fields
        // Display name, description, font and color settings for examine UI
        private enum UIType { None, BasicLowerUI, RightSideUI}

        [SerializeField] private UIType _UIType = UIType.None;

        [SerializeField] private string itemName = null;
        [SerializeField] private int textSize = 32;
        [SerializeField] private TMP_FontAsset fontType = null;
        [SerializeField] private Color fontColor = Color.white;

        [SerializeField, TextArea] private string itemDescription = null;
        [SerializeField] private int textSizeDesc = 30;
        [SerializeField] private TMP_FontAsset fontTypeDesc = null;
        [SerializeField] private Color fontColorDesc = Color.white;
        #endregion

        #region Initialisation Fields
        // Cached references and starting state
        Vector3 originalPosition;
        Quaternion originalRotation;
        private bool allowExamineInput;
        private float currentZoom = 1;
        private Camera mainCamera;
        private Transform examinePoint = null;
        private AKUIManager akUIManager;
        private BoxCollider boxCollider;
        #endregion

        #region String Field References
        // Constant values used throughout script
        private const string mouseX = "Mouse X";
        private const string mouseY = "Mouse Y";
        private const string examineLayer = "ExamineLayer";
        private const string defaultLayer = "Default";
        private const string inspectPointTag = "InspectPoint";
        private const string inspectLayer = "InspectPointLayer";
        #endregion

        #region Collectable Fields
        // Optional system integrations for collectible item types
        [SerializeField] private bool hasChildObjects = false;
        [SerializeField] private bool _isCollectable = false;
        [SerializeField] private bool removeInteractionCollider = false;
        [SerializeField] private SystemType _systemType = SystemType.None;
        private enum SystemType { None, FlashlightSys, GeneratorSys, GasMaskSys, ThemedKeySys, ChessSys, FuseBoxSys, ValveSys, KeycardSys }

        // References to specific system types
        private FlashlightItem _flashlightItemController;
        private GeneratorItem _generatorItem;
        private GasMaskItem _gasMaskItem;
        private TKItem _themedKeyItem;
        private ChessItem _chessItem;
        private FuseItem _fuseboxItem;
        private ValveItem _valveItem;
        private KeycardItem _keycardItem;
        #endregion

        #region Public Properties
        // Get/set properties for inspect and collect states
        public bool hasInspectPoints { get => _hasInspectPoints; set => _hasInspectPoints = value; }
        public bool isCollectable { get => _isCollectable; set => _isCollectable = value; }
        #endregion

        void Start()
        {
            // Setup defaults, references and collectable detection
            inspectPointLayer = LayerMask.GetMask(inspectLayer);
            initialZoom = Mathf.Clamp(initialZoom, zoomRange.x, zoomRange.y);
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            examinePoint = GameObject.FindGameObjectWithTag("ExaminePoint")?.transform;
            boxCollider = GetComponent<BoxCollider>();
            mainCamera = Camera.main;
            if (isCollectable) SetType();
            CheckSoundDebug();
        }

        public void StartLooking() { } // Unused: item highlighted

        public void StopInteraction() { } // Unused: item un-highlighted

        public void HandleInputClick() { ExamineObject(); } // Trigger examination

        public void HandleInputHold() { } // Unused: holding logic

        public void HandleInputStop() { } // Unused: input released

        void SetExamineLayer(string layerName)
        {
            // Apply layer to this item or all children (if compound object)
            int layer = LayerMask.NameToLayer(layerName);
            if (!hasChildObjects) { gameObject.layer = layer; return; }
            foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = layer;
        }

        void Update()
        {
            // Handle input while examining
            if (allowExamineInput)
            {
                ExamineInput();
                ExamineZooming();
            }
        }

        void ExamineInput()
        {
            if (!GameState.IsExamining) return;

            // Mouse-based rotation and zoom
            float rotMul = invertRotation ? 1 : -1;
            float h = rotationSpeed * rotMul * Input.GetAxis(mouseX);
            float v = rotationSpeed * rotMul * Input.GetAxis(mouseY);

            if (hasInspectPoints) FindInspectPoints();

            // Rotate item
            if (Input.GetKey(AKInputManager.instance.rotateKey))
                transform.Rotate(v, h, 0);
            // Drop item
            else if (Input.GetKeyDown(AKInputManager.instance.dropKey))
                DropObject(true);
            // Collect item
            else if (isCollectable && Input.GetKeyDown(AKInputManager.instance.pickupItemKey))
                CollectItem();
        }

        public void ExamineObject()
        {
            // Start the examination state
            GameState.IsExamining = true;
            AKUIManager.instance._examinableItem = this;
            akUIManager = AKUIManager.instance;
            AKDisableManager.instance.DisablePlayerDefault(true, true, true);
            AKPromptManager.Instance.RegisterPromptsForSubsystem(isCollectable ? "ExamineWithCollect" : "Examine");

            ToggleEmission(false);
            var akItem = GetComponent<AKItem>();
            if (akItem != null && akItem.ShowEmissionHighlight) akItem.ToggleEmission(false);

            if (removeInteractionCollider) boxCollider.enabled = false;

            SetExamineLayer(examineLayer);
            EnableInspectPoints(inspectLayer);
            PlayPickupSound();
            ResetZoomAndPosition(initialZoom);
            allowExamineInput = true;
            StartCoroutine(MoveToPosition(transform, examinePoint.position, smoothExamineSpeed));
            HandleUI(true);
        }

        public void DropObject(bool shouldLerp)
        {
            // End examination and return object
            GameState.IsExamining = false;
            AKDisableManager.instance.DisablePlayerDefault(false, false, true);
            if (shouldLerp) StartCoroutine(MoveToPosition(transform, originalPosition, smoothExamineSpeed));

            transform.rotation = originalRotation;
            if (removeInteractionCollider) boxCollider.enabled = true;

            SetExamineLayer(defaultLayer);
            PlayDropSound();
            InspectPointUI(null, null, false);
            DisableInspectPoints();
            currentZoom = initialZoom;
            ItemZoom(currentZoom, false);
            allowExamineInput = false;
            ToggleEmission(true);
            AKPromptManager.Instance.ClearPrompts();
            HandleUI(false);
        }

        private void ToggleEmission(bool enable)
        {
            // Toggles emission on AKItem
            var akItem = GetComponent<AKItem>();
            if (akItem != null && akItem.ShowEmissionHighlight)
                akItem.ToggleEmission(enable);
        }

        private void ResetZoomAndPosition(float zoomLevel)
        {
            // Reset zoom and rotation
            currentZoom = zoomLevel;
            ItemZoom(currentZoom, false);
            transform.LookAt(transform.position + mainCamera.transform.forward, mainCamera.transform.up);
            transform.Rotate(initialRotationOffset);
        }

        private void HandleUI(bool enable)
        {
            // Show or hide appropriate UI layout
            switch (_UIType)
            {
                case UIType.None: akUIManager.ShowCloseButton(enable); break;
                case UIType.BasicLowerUI:
                    akUIManager.SetBasicUIText(enable ? itemName : null, enable ? itemDescription : null, enable);
                    if (enable) TextCustomisation(); break;
                case UIType.RightSideUI:
                    akUIManager.SetRightSideUIText(enable ? itemName : null, enable ? itemDescription : null, enable);
                    if (enable) TextCustomisation(); break;
            }
        }

        void ExamineZooming()
        {
            // Zoom in/out using scroll wheel
            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                currentZoom = Mathf.Clamp(currentZoom + scrollDelta * zoomSensitivity, zoomRange.x, zoomRange.y);
                ItemZoom(currentZoom);
            }
        }

        private void ItemZoom(float value, bool moveSelf = true)
        {
            // Adjust examine point distance
            examinePoint.localPosition = new Vector3(horizontalOffset, verticalOffset, value);
            if (moveSelf) transform.position = examinePoint.position;
        }

        IEnumerator MoveToPosition(Transform target, Vector3 destination, float duration)
        {
            // Smoothly move item into/out of view
            float t = 0;
            Vector3 start = target.position;
            while (t < duration)
            {
                target.position = Vector3.Lerp(start, destination, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            target.position = destination;
        }

        private void TextCustomisation()
        {
            // Applies font settings based on UI type
            switch (_UIType)
            {
                case UIType.BasicLowerUI:
                    akUIManager.SetBasicUITextSettings(textSize, fontType, fontColor, textSizeDesc, fontTypeDesc, fontColorDesc);
                    break;
                case UIType.RightSideUI:
                    akUIManager.SetRightUITextSettings(textSize, fontType, fontColor, textSizeDesc, fontTypeDesc, fontColorDesc);
                    break;
            }
        }

        void FindInspectPoints()
        {
            // Raycast to detect inspectable points
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
            {
                if (hit.transform.CompareTag(inspectPointTag) && hit.transform.gameObject.layer == LayerMask.NameToLayer(inspectLayer))
                {
                    InspectPointUI(hit.transform.gameObject, mainCamera, true);
                    if (Input.GetKeyDown(AKInputManager.instance.interactKey) && !disableInspectInput)
                    {
                        StartCoroutine(NextIspectPointTimer(1f));
                        hit.transform.GetComponent<ExamineInspectPoint>().InspectPointInteract();
                    }
                }
                else InspectPointUI(null, null, false);
            }
            else InspectPointUI(null, null, false);
        }

        void InspectPointUI(GameObject item, Camera cam, bool show)
        {
            // Shows or hides inspect point UI
            if (show)
            {
                Vector3 pos = cam.WorldToScreenPoint(item.transform.position);
                akUIManager.SetInspectPointParent(true, pos);
                akUIManager.SetInspectPointText(item.GetComponent<ExamineInspectPoint>().InspectInformation());
            }
            else akUIManager.SetInspectPointParent(false, Vector3.zero);
        }

        IEnumerator NextIspectPointTimer(float waitTime)
        {
            // Prevent spam interaction with inspect points
            disableInspectInput = true;
            yield return new WaitForSeconds(waitTime);
            disableInspectInput = false;
        }

        void EnableInspectPoints(string layerName)
        {
            // Delayed enabling of inspect points
            StartCoroutine(WaitBeforeEnable(0.1f, layerName));
        }

        IEnumerator WaitBeforeEnable(float waitTime, string layerName)
        {
            yield return new WaitForSeconds(waitTime);
            int layer = LayerMask.NameToLayer(layerName);

            if (inspectPoints.Length >= 1)
            {
                hasInspectPoints = true;
                foreach (var inspectpoint in inspectPoints)
                {
                    inspectpoint.SetActive(true);
                    inspectpoint.layer = layer;
                }
            }
        }

        void DisableInspectPoints()
        {
            // Hide all inspect points
            if (hasInspectPoints)
                foreach (var p in inspectPoints) p.SetActive(false);
        }

        void SetType()
        {
            // Assign appropriate system component based on enum type
            switch (_systemType)
            {
                case SystemType.FlashlightSys: _flashlightItemController = GetComponent<FlashlightItem>(); break;
                case SystemType.GeneratorSys: _generatorItem = GetComponent<GeneratorItem>(); break;
                case SystemType.GasMaskSys: _gasMaskItem = GetComponent<GasMaskItem>(); break;
                case SystemType.ThemedKeySys: _themedKeyItem = GetComponent<TKItem>(); break;
                case SystemType.ChessSys: _chessItem = GetComponent<ChessItem>(); break;
                case SystemType.FuseBoxSys: _fuseboxItem = GetComponent<FuseItem>(); break;
                case SystemType.ValveSys: _valveItem = GetComponent<ValveItem>(); break;
                case SystemType.KeycardSys: _keycardItem = GetComponent<KeycardItem>(); break;
            }
        }

        void CollectItem()
        {
            // Trigger item collection based on type
            switch (_systemType)
            {
                case SystemType.FlashlightSys: _flashlightItemController.HandleInputClick(); break;
                case SystemType.GeneratorSys: _generatorItem.HandleInputClick(); break;
                case SystemType.GasMaskSys: _gasMaskItem.HandleInputClick(); break;
                case SystemType.ThemedKeySys: _themedKeyItem.HandleInputClick(); break;
                case SystemType.ChessSys: _chessItem.HandleInputClick(); break;
                case SystemType.FuseBoxSys: _fuseboxItem.HandleInputClick(); break;
                case SystemType.ValveSys: _valveItem.HandleInputClick(); break;
                case SystemType.KeycardSys: _keycardItem.HandleInputClick(); break;
            }
            DropObject(false);
        }

        void PlayPickupSound()
        {
            if (pickupSound != null)
                AKAudioManager.instance.Play(pickupSound);
        }

        void PlayDropSound()
        {
            if (dropSound != null)
                AKAudioManager.instance.Play(dropSound);
        }

        private void CheckSoundDebug()
        {
            // Debug for missing sounds
            if (pickupSound == null)
                print("Did you forget to add a sound Scriptable to item " + gameObject);
            else if (dropSound == null)
                print("Did you forget to add a sound Scriptable to item " + gameObject);
        }
    }
}
