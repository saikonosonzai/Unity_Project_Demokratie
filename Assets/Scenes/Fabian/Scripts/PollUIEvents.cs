using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// --- DATEN-KLASSEN FÜR DIE JSON DATEI---
[System.Serializable]
public class StatementData
{
    public string text;
    public bool isTrue;
}

[System.Serializable]
public class StatementList
{
    public StatementData[] statements;
}
// ----------------------------------------

public class PollUIEvents : MonoBehaviour
{
    private UIDocument _document;
    public SeasonClockController clockController;
    public TextAsset jsonFile; 

    private Label _statementLabel;
    private Button _btnTrue;
    private Button _btnFalse;
    
    private float _yesVotes = 0f;
    
    private StatementList _loadedData;
    
    private List<int> _remainingQuestions = new List<int>(); // welche Fragen sind noch offen?
    private int _currentListIndex = 0; // Wo in der offenen Liste sind wir gerade?
    private float _stepSize; // Wie viele Punkte gibt eine Frage?

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        var root = _document.rootVisualElement;

        // UI Elemente laden
        _statementLabel = root.Q<Label>("StatementText"); 
        _btnTrue = root.Q<Button>("BtnTrue");
        _btnFalse = root.Q<Button>("BtnFalse");

        // Daten laden & Start
        LoadStatements();
        DisplayStatement();

        // Click Events registrieren
        if (_btnTrue != null) _btnTrue.clicked += OnClickTrue;
        if (_btnFalse != null) _btnFalse.clicked += OnClickFalse;
    }

    private void OnDisable()
    {
        if (_btnTrue != null) _btnTrue.clicked -= OnClickTrue;
        if (_btnFalse != null) _btnFalse.clicked -= OnClickFalse;
    }

    private void OnClickTrue() { EvaluateAnswer(true); }
    private void OnClickFalse() { EvaluateAnswer(false); }

    private void LoadStatements()
    {
        if (jsonFile != null)
        {
            _loadedData = JsonUtility.FromJson<StatementList>(jsonFile.text);
            
            _remainingQuestions.Clear();
            for (int i = 0; i < _loadedData.statements.Length; i++)
            {
                _remainingQuestions.Add(i);
            }

            _stepSize = 100f / _loadedData.statements.Length;

            if (clockController != null)
            {
                clockController.SetStepSizes(_loadedData.statements.Length);
            }
        }
        else
        {
            Debug.LogError("Keine JSON-Datei im Inspector zugewiesen!");
        }
    }

    private void DisplayStatement()
    {
        // Keine Daten? Abbruch
        if (_loadedData == null || _remainingQuestions.Count == 0) return;

        // Frage Nummer aus den Daten ermitteln
        int actualQuestionIndex = _remainingQuestions[_currentListIndex];

        if (_statementLabel != null) 
        {
            _statementLabel.text = _loadedData.statements[actualQuestionIndex].text;
        }
    }

    private void EvaluateAnswer(bool playerGuess)
    {
        if (_remainingQuestions.Count == 0) return;

        int actualQuestionIndex = _remainingQuestions[_currentListIndex];
        StatementData currentStatement = _loadedData.statements[actualQuestionIndex];
        
        bool isCorrect = (playerGuess == currentStatement.isTrue);

        if (isCorrect)
        {
            Debug.Log("Korrekt!");
            _yesVotes += _stepSize;
            if (clockController != null) clockController.movePointers(true);

            // Frage aus der Liste werfen, da sie richtig beantwortet wurde!
            _remainingQuestions.RemoveAt(_currentListIndex);

            // Wenn der Index nun zu hoch ist, Index = 0
            if (_remainingQuestions.Count > 0 && _currentListIndex >= _remainingQuestions.Count)
            {
                _currentListIndex = 0; // Wieder vorne anfangen
            }
        }
        else
        {
            Debug.Log("Falsch! Frage kommt später nochmal.");
                   

            // Frage bleibt in der Liste. Wir gehen einfach einen Schritt weiter.
            _currentListIndex++;
            if (_currentListIndex >= _remainingQuestions.Count)
            {
                _currentListIndex = 0; // Ende der Liste erreicht? Wieder bei den anderen Fehlern vorne anfangen!
            }
        }

        _yesVotes = Mathf.Clamp(_yesVotes, 0, 100);
        

        
        if (_remainingQuestions.Count == 0) 
        {
            Win();
        }
        else 
        {
            DisplayStatement();
        }
    }

    private void Win()
    {
        Debug.Log("You Win");
        if (clockController != null) clockController.puzzleSolved = true;
        ExitPuzzle();
    }


    private void ExitPuzzle()
    {
        if (clockController != null) clockController.ExitPuzzle();
    }

}