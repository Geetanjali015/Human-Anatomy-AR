
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MuscleQuizManager : MonoBehaviour
{
    [Header("Quiz UI")]
    public GameObject quizPanel;
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public Button[] optionButtons;
    public Button nextButton;
    public Button closeButton;

    [Header("Three Highlighted Muscles")]
    public GameObject object79;
    public GameObject object95;
    public GameObject object97;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(0.02f, 0.35f, 0.08f, 1f);

    [Header("UI Layout")]
    public Vector2 feedbackPosition = new Vector2(0, -220);
    public Vector2 nextPosition = new Vector2(0, -310);
    public Vector2 closePosition = new Vector2(0, -400);

    private readonly string[] colorProperties =
    {
        "diffuseFactor",
        "_BaseColor",
        "_Color",
        "_BaseColorFactor"
    };

    private string[][] options =
    {
        new string[]
        {
            "Fibularis longus",
            "Brachioradialis",
            "Temporalis",
            "Gastrocnemius"
        },
        new string[]
        {
            "Deltoid",
            "Brachioradialis",
            "Masseter",
            "Fibularis longus"
        },
        new string[]
        {
            "Masseter",
            "Biceps brachii",
            "Temporalis",
            "Brachioradialis"
        }
    };

    private string[] questions =
    {
        "Identify the highlighted muscle.",
        "Which muscle is highlighted on the forearm?",
        "Identify the highlighted muscle near the temple."
    };

    private int[] correctAnswers = { 0, 1, 2 };

    private GameObject[] muscles;

    private int currentQuestion = 0;
    private int score = 0;

    private bool answered = false;
    private bool quizCompleted = false;

    private List<Material> highlightedMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private List<string> usedColorProperties = new List<string>();

    void Start()
    {
        muscles = new GameObject[]
        {
            object79,
            object95,
            object97
        };

        ConfigureUILayout();

        if (quizPanel != null)
            quizPanel.SetActive(false);

        // Register Next button only once.
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextQuestion);
            nextButton.gameObject.SetActive(false);
        }

        // Register answer buttons only once.
        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null)
                    continue;

                int index = i;

                optionButtons[i].onClick.RemoveAllListeners();

                optionButtons[i].onClick.AddListener(
                    () => CheckAnswer(index)
                );
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseQuiz);
        }

        Debug.Log("Muscle Quiz Manager initialized.");
    }

    void ConfigureUILayout()
    {
        if (feedbackText != null)
        {
            RectTransform rect = feedbackText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = feedbackPosition;
            rect.sizeDelta = new Vector2(900, 80);
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.gameObject.SetActive(true);
        }

        if (nextButton != null)
        {
            RectTransform rect = nextButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = nextPosition;
            rect.sizeDelta = new Vector2(500, 75);
        }

        if (closeButton != null)
        {
            RectTransform rect = closeButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = closePosition;
            rect.sizeDelta = new Vector2(500, 75);
        }
    }

    public void OpenQuiz()
    {
        RestoreMuscle();

        if (quizPanel == null)
        {
            Debug.LogError("Quiz Panel is not assigned!");
            return;
        }

        currentQuestion = 0;
        score = 0;
        answered = false;
        quizCompleted = false;

        quizPanel.SetActive(true);

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        ConfigureUILayout();
        LoadQuestion();
    }

    void LoadQuestion()
    {
        answered = false;

        if (currentQuestion >= questions.Length)
            return;

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "";
            feedbackText.color = Color.white;
        }

        if (questionText != null)
        {
            questionText.text =
                "Question " + (currentQuestion + 1) +
                " / 3\n\n" + questions[currentQuestion];
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            Button button = optionButtons[i];

            if (button == null)
                continue;

            button.gameObject.SetActive(true);
            button.interactable = true;

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    (char)('A' + i) + ". " +
                    options[currentQuestion][i];
            }
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.interactable = false;
        }

        // Highlight the muscle belonging to this question.
        HighlightMuscle(muscles[currentQuestion]);

        Debug.Log(
            "Question " + (currentQuestion + 1) +
            " loaded. Muscle: " + muscles[currentQuestion].name
        );

        Canvas.ForceUpdateCanvases();
    }

    void CheckAnswer(int selected)
    {
        Debug.Log("ANSWER BUTTON CLICKED: " + selected);

        if (answered || quizCompleted)
            return;

        if (selected < 0 || selected >= options[currentQuestion].Length)
            return;

        answered = true;

        // Disable all answers after one selection.
        foreach (Button button in optionButtons)
        {
            if (button != null)
                button.interactable = false;
        }

        // Display feedback.
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);

            if (selected == correctAnswers[currentQuestion])
            {
                score++;
                feedbackText.text = "CORRECT! WELL DONE!";
                feedbackText.color = new Color(0.1f, 0.8f, 0.3f);
            }
            else
            {
                feedbackText.text =
                    "INCORRECT! Correct answer: " +
                    options[currentQuestion][correctAnswers[currentQuestion]];

                feedbackText.color = Color.red;
            }
        }

        // Show Next / Finish below the MCQs.
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;

            TMP_Text nextText =
                nextButton.GetComponentInChildren<TMP_Text>();

            if (nextText != null)
            {
                nextText.text =
                    currentQuestion == 2 ? "FINISH" : "NEXT";
            }
        }

        // Keep Close button separate and visible.
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        Debug.Log("Answer processed. Next button activated.");
    }

    public void NextQuestion()
    {
        Debug.Log("NEXT BUTTON CLICKED");

        if (quizCompleted)
        {
            CloseQuiz();
            return;
        }

        if (!answered)
        {
            Debug.LogWarning("Select an answer first!");
            return;
        }

        // Restore the previous muscle before moving on.
        RestoreMuscle();

        if (currentQuestion < 2)
        {
            currentQuestion++;
            LoadQuestion();
        }
        else
        {
            ShowResults();
        }
    }

    void ShowResults()
    {
        quizCompleted = true;

        RestoreMuscle();

        foreach (Button button in optionButtons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }

        if (questionText != null)
        {
            questionText.text =
                "QUIZ COMPLETED!\n\nYour Score: " +
                score + " / 3";
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);

            feedbackText.text =
                score == 3 ? "Excellent work!" :
                score >= 2 ? "Good job!" :
                "Keep practising!";

            feedbackText.color = Color.white;
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;

            TMP_Text nextText =
                nextButton.GetComponentInChildren<TMP_Text>();

            if (nextText != null)
                nextText.text = "BACK TO AR";
        }

        Debug.Log("Quiz completed. Final score: " + score + "/3");
    }

    void HighlightMuscle(GameObject muscle)
    {
        RestoreMuscle();

        if (muscle == null)
        {
            Debug.LogError("Muscle object is not assigned!");
            return;
        }

        Renderer[] renderers =
            muscle.GetComponentsInChildren<Renderer>(true);

        Debug.Log(
            "Highlighting " + muscle.name +
            ". Renderers found: " + renderers.Length
        );

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            foreach (Material mat in materials)
            {
                string selectedProperty = "";

                foreach (string property in colorProperties)
                {
                    if (mat.HasProperty(property))
                    {
                        selectedProperty = property;
                        break;
                    }
                }

                if (selectedProperty == "")
                {
                    Debug.LogWarning(
                        "No supported colour property on " + mat.name
                    );
                    continue;
                }

                // Save original material colour and property.
                originalColors.Add(mat.GetColor(selectedProperty));
                highlightedMaterials.Add(mat);
                usedColorProperties.Add(selectedProperty);

                // Apply dark green.
                mat.SetColor(selectedProperty, highlightColor);

                Debug.Log(
                    "DARK GREEN HIGHLIGHT APPLIED: " + mat.name +
                    " using " + selectedProperty
                );
            }
        }

        if (highlightedMaterials.Count == 0)
        {
            Debug.LogError(
                "No materials could be highlighted on " + muscle.name
            );
        }
    }

    void RestoreMuscle()
    {
        for (int i = 0; i < highlightedMaterials.Count; i++)
        {
            if (highlightedMaterials[i] != null)
            {
                highlightedMaterials[i].SetColor(
                    usedColorProperties[i],
                    originalColors[i]
                );
            }
        }

        highlightedMaterials.Clear();
        originalColors.Clear();
        usedColorProperties.Clear();
    }

    public void CloseQuiz()
    {
        RestoreMuscle();

        if (quizPanel != null)
            quizPanel.SetActive(false);

        quizCompleted = false;
        answered = false;

        Debug.Log("Quiz closed. Returned to AR.");
    }

    void OnDestroy()
    {
        RestoreMuscle();
    }
}
