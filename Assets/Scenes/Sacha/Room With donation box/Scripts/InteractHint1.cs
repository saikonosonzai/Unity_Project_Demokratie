    using AdventurePuzzleKit.Scenes.Sacha.Scripts;
    using UnityEngine;
    using TMPro;

    public class InteractHint1 : MonoBehaviour
    {
        public static DonationBoxRoomHints hintState;
        [SerializeField] private Transform _player;
        private float interactRange = 2f;
        private bool _playerInRange;
        [SerializeField] private Canvas _hint1;

        private bool _isHintShowed = false;
        
        // Keypad
        [SerializeField] private KeypadManager _keypad;
        private bool _ispadshown = false;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _player =  GameObject.FindGameObjectWithTag("MainCamera").transform;
            _hint1 = GameObject.Find("ShowHint1").GetComponent<Canvas>();
            _hint1.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            _playerInRange = Vector3.Distance(transform.position, _player.position) <= interactRange;

            if (_playerInRange && Input.GetKeyDown(KeyCode.E) && !_isHintShowed)
            {
                _isHintShowed = true;
                _hint1.gameObject.SetActive(true);
                ShowHint();
            } else if (_playerInRange && Input.GetKeyDown(KeyCode.E) && _isHintShowed)
            {
                _isHintShowed = false;
                _hint1.gameObject.SetActive(false);
            } else if (_playerInRange && Input.GetKeyDown(KeyCode.F) && !_ispadshown)
            {
                _ispadshown = true;
                ShowKeypad();
            } else if (_playerInRange && Input.GetKeyDown(KeyCode.F) && _ispadshown)
            {
                _ispadshown = false;
                CloseKeyPad();
            }
        }

        private void ShowHint()
        {
            hintState.hint1shown = true;
        }

        private void ShowKeypad()
        {
                _keypad.Open();
        }

        private void CloseKeyPad()
        {
            _keypad.Close();
        }
        
    }
