using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComicManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject comicPanel;
    public GameObject candyCrushPanel;

    [Header("Comic UI")]
    public Image comicImage;
    public TMP_Text comicText;
    public Button nextButton;

    [Header("Comic-Bilder")]
    public Sprite[] comicSprites;

    [Header("Entscheidung am Ende")]
    public GameObject choicePanel;
    public Button leftChoiceButton;
    public Button rightChoiceButton;
    public TMP_Text leftChoiceText;
    public TMP_Text rightChoiceText;

    [TextArea(1, 3)]
    public string finalQuestionText = "Welche Frau ist die richtige Wahl?";

    public string leftChoiceLabel = "Links";
    public string rightChoiceLabel = "Rechts";

    [Header("Ergebnis-Seite nach Auswahl")]
    public GameObject resultPanel;
    public Image resultImage;
    public TMP_Text resultText;
    public Button resultNextButton;

    [Header("Richtige Antwort")]
    public Sprite correctResultSprite;

    [TextArea(2, 6)]
    public string correctResultExplanation = "Richtig! Diese Wahl führt zum Ziel. Du hast die Geschichte erfolgreich beendet.";

    [Header("Falsche Antwort")]
    public Sprite wrongResultSprite;

    [TextArea(2, 6)]
    public string wrongResultExplanation = "Falsch! Diese Wahl führt nicht weiter. Das Schwert muss noch einmal seinen Weg finden.";

    [Header("Candy/Sword-Spiel")]
    public BremerCandyGameUI candyGame;

    [Header("Nach Abschluss nicht nochmal spielbar")]
    public bool disableAfterCompleted = true;
    public Collider paintingColliderToDisable;
    public GameObject objectToDisableAfterCompleted;

    [Header("Sicher beim Schließen deaktivieren")]
    public GameObject[] objectsToDisableWhenClosed;
    public Behaviour[] effectsToDisableWhenClosed;

    [Header("Player-Steuerung deaktivieren während UI")]
    public MonoBehaviour[] playerControlsToDisable;

    [Header("Kamera/Mausblick deaktivieren während UI")]
    public MonoBehaviour[] cameraControlsToDisable;

    [Header("Optional: Player Rigidbody stoppen")]
    public Rigidbody playerRigidbody;

    [Header("Comic-Seiten")]
    [TextArea(2, 6)]
    public string[] comicPages;

    [Header("Wann startet das Minispiel?")]
    public int candyStartsAfterPageIndex = 2;

    private int currentPageIndex = 0;
    private bool comicOpen = false;
    private bool waitingForFinalChoice = false;
    private bool resultScreenOpen = false;
    private bool lastChoiceWasCorrect = false;
    private bool completed = false;

    private void Start()
    {
        RemovePanelOverlayImages();
        HideAllUI();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextPage);
        }

        if (leftChoiceButton != null)
        {
            leftChoiceButton.onClick.RemoveAllListeners();
            leftChoiceButton.onClick.AddListener(ChooseLeft);
        }

        if (rightChoiceButton != null)
        {
            rightChoiceButton.onClick.RemoveAllListeners();
            rightChoiceButton.onClick.AddListener(ChooseRight);
        }

        if (resultNextButton != null)
        {
            resultNextButton.onClick.RemoveAllListeners();
            resultNextButton.onClick.AddListener(ContinueAfterResultScreen);
        }

        if (candyGame != null)
            candyGame.comicManager = this;

        if (comicPages == null || comicPages.Length == 0)
        {
            comicPages = new string[]
            {
                "Du betrachtest das alte Bremer Gemälde.",
                "Eine geheimnisvolle Geschichte beginnt.",
                "Plötzlich wird das Schwert wichtig. Es muss nach unten gebracht werden!",
                "Das Schwert ist angekommen. Das Gemälde verändert sich.",
                "Jetzt musst du dich entscheiden: Welche Frau führt zur Lösung?"
            };
        }

        UpdateChoiceTexts();
    }

    public void OpenComic()
    {
        if (completed && disableAfterCompleted)
        {
            Debug.Log("Dieses Gemälde wurde bereits abgeschlossen.");
            return;
        }

        if (comicOpen)
            return;

        RemovePanelOverlayImages();

        comicOpen = true;
        waitingForFinalChoice = false;
        resultScreenOpen = false;
        lastChoiceWasCorrect = false;
        currentPageIndex = 0;

        SetPlayerInput(false);

        if (comicPanel != null)
            comicPanel.SetActive(true);

        if (candyCrushPanel != null)
            candyCrushPanel.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        if (comicImage != null)
            comicImage.gameObject.SetActive(true);

        if (comicText != null)
            comicText.gameObject.SetActive(true);

        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        RemovePanelOverlayImages();

        waitingForFinalChoice = false;
        resultScreenOpen = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        if (comicImage != null)
            comicImage.gameObject.SetActive(true);

        if (comicText != null)
            comicText.gameObject.SetActive(true);

        if (comicText != null)
        {
            if (currentPageIndex >= 0 && currentPageIndex < comicPages.Length)
                comicText.text = comicPages[currentPageIndex];
            else
                comicText.text = "";
        }

        UpdateComicImage();
    }

    private void UpdateComicImage()
    {
        if (comicImage == null)
            return;

        if (comicSprites != null &&
            currentPageIndex >= 0 &&
            currentPageIndex < comicSprites.Length &&
            comicSprites[currentPageIndex] != null)
        {
            comicImage.sprite = comicSprites[currentPageIndex];
            comicImage.color = Color.white;
            comicImage.preserveAspect = true;
        }
        else
        {
            comicImage.sprite = null;
            comicImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public void NextPage()
    {
        if (!comicOpen)
            return;

        if (waitingForFinalChoice || resultScreenOpen)
            return;

        if (currentPageIndex == candyStartsAfterPageIndex)
        {
            StartCandyGame();
            return;
        }

        currentPageIndex++;

        if (currentPageIndex >= comicPages.Length)
        {
            ShowFinalChoice();
            return;
        }

        ShowCurrentPage();
    }

    private void StartCandyGame()
    {
        RemovePanelOverlayImages();

        waitingForFinalChoice = false;
        resultScreenOpen = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (comicPanel != null)
            comicPanel.SetActive(false);

        if (candyCrushPanel != null)
            candyCrushPanel.SetActive(true);

        if (candyGame != null)
            candyGame.StartGame();
    }

    public void OnCandyGameFinished()
    {
        RemovePanelOverlayImages();

        if (candyCrushPanel != null)
            candyCrushPanel.SetActive(false);

        if (comicPanel != null)
            comicPanel.SetActive(true);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        currentPageIndex++;

        if (currentPageIndex >= comicPages.Length)
        {
            ShowFinalChoice();
            return;
        }

        ShowCurrentPage();
    }

    private void ShowFinalChoice()
    {
        RemovePanelOverlayImages();

        waitingForFinalChoice = true;
        resultScreenOpen = false;

        if (comicPanel != null)
            comicPanel.SetActive(true);

        if (candyCrushPanel != null)
            candyCrushPanel.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(true);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (comicImage != null)
            comicImage.gameObject.SetActive(false);

        if (comicText != null)
        {
            comicText.gameObject.SetActive(true);
            comicText.text = finalQuestionText;
        }

        UpdateChoiceTexts();
    }

    private void ChooseLeft()
    {
        ShowResultScreen(false);
    }

    private void ChooseRight()
    {
        ShowResultScreen(true);
    }

    private void ShowResultScreen(bool correct)
    {
        RemovePanelOverlayImages();

        waitingForFinalChoice = false;
        resultScreenOpen = true;
        lastChoiceWasCorrect = correct;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (comicImage != null)
            comicImage.gameObject.SetActive(false);

        if (comicText != null)
            comicText.gameObject.SetActive(false);

        if (resultImage != null)
        {
            resultImage.sprite = correct ? correctResultSprite : wrongResultSprite;
            resultImage.color = Color.white;
            resultImage.preserveAspect = true;
        }

        if (resultText != null)
            resultText.text = correct ? correctResultExplanation : wrongResultExplanation;

        if (resultNextButton != null)
            resultNextButton.gameObject.SetActive(true);
    }

    private void ContinueAfterResultScreen()
    {
        if (!resultScreenOpen)
            return;

        if (lastChoiceWasCorrect)
        {
            StartCoroutine(FinishCompletely());
        }
        else
        {
            StartCoroutine(WrongChoiceRestartGame());
        }
    }

    private IEnumerator WrongChoiceRestartGame()
    {
        waitingForFinalChoice = false;
        resultScreenOpen = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (comicImage != null)
            comicImage.gameObject.SetActive(false);

        if (comicText != null)
        {
            comicText.gameObject.SetActive(true);
            comicText.text = "Das Schwert muss noch einmal seinen Weg finden.";
        }

        yield return new WaitForSeconds(1.2f);

        currentPageIndex = comicPages.Length - 1;

        StartCandyGame();
    }

    private IEnumerator FinishCompletely()
    {
        waitingForFinalChoice = false;
        resultScreenOpen = false;

        completed = true;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (comicImage != null)
            comicImage.gameObject.SetActive(false);

        if (comicText != null)
        {
            comicText.gameObject.SetActive(true);
            comicText.text = "Richtig! Du hast es geschafft.";
        }

        yield return new WaitForSeconds(1.2f);

        CloseComic();

        if (disableAfterCompleted)
            DisableThisGameForever();
    }

    public void CloseComic()
    {
        comicOpen = false;
        waitingForFinalChoice = false;
        resultScreenOpen = false;

        HideAllUI();
        DisableExtraObjects();
        DisableExtraEffects();

        SetPlayerInput(true);

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void HideAllUI()
    {
        if (comicPanel != null)
            comicPanel.SetActive(false);

        if (candyCrushPanel != null)
            candyCrushPanel.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(true);

        if (comicImage != null)
            comicImage.gameObject.SetActive(true);

        if (comicText != null)
            comicText.gameObject.SetActive(true);
    }

    private void DisableThisGameForever()
    {
        if (paintingColliderToDisable != null)
            paintingColliderToDisable.enabled = false;
    }

    private void DisableExtraObjects()
    {
        if (objectsToDisableWhenClosed == null)
            return;

        foreach (GameObject obj in objectsToDisableWhenClosed)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void DisableExtraEffects()
    {
        if (effectsToDisableWhenClosed == null)
            return;

        foreach (Behaviour effect in effectsToDisableWhenClosed)
        {
            if (effect != null)
                effect.enabled = false;
        }
    }

    private void RemovePanelOverlayImages()
    {
        MakePanelImageTransparent(comicPanel);
        MakePanelImageTransparent(candyCrushPanel);
        MakePanelImageTransparent(choicePanel);
        MakePanelImageTransparent(resultPanel);
    }

    private void MakePanelImageTransparent(GameObject panel)
    {
        if (panel == null)
            return;

        Image panelImage = panel.GetComponent<Image>();

        if (panelImage != null)
        {
            panelImage.color = new Color(1f, 1f, 1f, 0f);
            panelImage.raycastTarget = false;
        }
    }

    private void UpdateChoiceTexts()
    {
        if (leftChoiceText != null)
            leftChoiceText.text = leftChoiceLabel;

        if (rightChoiceText != null)
            rightChoiceText.text = rightChoiceLabel;
    }

    private void SetPlayerInput(bool enabled)
    {
        if (playerControlsToDisable != null)
        {
            foreach (MonoBehaviour control in playerControlsToDisable)
            {
                if (control != null)
                    control.enabled = enabled;
            }
        }

        if (cameraControlsToDisable != null)
        {
            foreach (MonoBehaviour control in cameraControlsToDisable)
            {
                if (control != null)
                    control.enabled = enabled;
            }
        }

        if (!enabled && playerRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            playerRigidbody.linearVelocity = Vector3.zero;
#else
            playerRigidbody.velocity = Vector3.zero;
#endif
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (enabled)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}