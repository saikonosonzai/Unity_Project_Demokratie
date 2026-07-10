using System.Net;
using AdventurePuzzleKit.Scenes.Sacha.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;



public class InteractHint2 : MonoBehaviour
{
    public static DonationBoxRoomHints hintState;
    [SerializeField] private Transform _player;

    private float interactRange = 1f;
    [SerializeField] private Image _hintImage;
    private bool _isHintShowed;
    private bool _playerInRange;
    
    // Keypad 
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.Find("Main Camera").transform;
        _hintImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!hintState.hint1shown)
        {
            return;
        }
        _playerInRange = Vector3.Distance(transform.position, _player.position) <= interactRange;

        if (_playerInRange && Input.GetKeyDown(KeyCode.E) && !_isHintShowed)
        {
            _isHintShowed = true;
            _hintImage.gameObject.SetActive(true);
            hintState.hint2shown = true;
            
        } else if (_playerInRange && Input.GetKeyDown(KeyCode.E) && _isHintShowed)
        {
            _isHintShowed = false;
            _hintImage.gameObject.SetActive(false);
        }
    }
}
