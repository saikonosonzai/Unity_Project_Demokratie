using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeypadManager : MonoBehaviour
{
    [SerializeField] private GameObject keyPadUI;
    [SerializeField] private TextMeshProUGUI _keypadText;

    private readonly string _correctPasscode = "1949";

    private string _input = "";

    void Start()
    {
        _keypadText = GetComponentInChildren<TextMeshProUGUI>();
        _input = "";
        _keypadText.text = _input;
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void PressClear()
    {
        _input = "";
        _keypadText.text = _input;
    }

    public void PressKey(string key)
    {
        _input += key;
        _keypadText.text = _input;
    }

    public void PressEnter()
    {
        if (_input == _correctPasscode)
        {
            // temp success
            OnSuccess();
        }
        else
        {
            OnFail();
        }
    }

    private void OnSuccess()
    {
        Debug.Log("Correct");
        Close();
    }

    private void OnFail()
    {
        Debug.Log("False");
    }
}
