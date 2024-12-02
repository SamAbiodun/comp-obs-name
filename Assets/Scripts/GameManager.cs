using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;  // Add this for TextMeshPro

[System.Serializable]
public class SaveData
{
    public int score;
    public int comboMultiplier;
    public int currentCombo;
    public int correctGuesses;
    public bool gameCompleted;
    public List<int> matchedIndices = new List<int>(); // Indices of matched cards
    public List<Sprite> gameCards = new List<Sprite>();
}

public class GameManager : MonoBehaviour
{
    public List<Button> btns = new List<Button>();
    [SerializeField] private Sprite background;

    public Sprite[] cards;
    public List<Sprite> gameCards = new List<Sprite>();

    private bool firstGuess, secondGuess;
    public static bool gameCompleted;
    private int countGuesses, correctGuesses, gameGuesses, firstGuessIndex, secondGuessIndex;
    private string firstGuessCard, secondGuessCard;

    // AudioSource to play sound effects
    private AudioSource audioSource;

    // Sound effects
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;

    // Score and combo system
    private int score;
    private int comboMultiplier;
    private int currentCombo;

    // Persistence
    private SaveData saveData = new SaveData();
    private string saveFilePath;

    // UI elements for score and combo (using TextMeshProUGUI)
    [SerializeField] private TextMeshProUGUI scoreText;  // Changed to TextMeshProUGUI
    [SerializeField] private TextMeshProUGUI comboText;  // Changed to TextMeshProUGUI

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/saveData.json";
        cards = Resources.LoadAll<Sprite>("Sprites/Icons");

        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Debug.Log(gameCompleted);
        ResetSave();
        GetButtons();
        ShuffleCards();
        AddGameCards();
        ShuffleGameCards();
        gameGuesses = gameCards.Count / 2;
        if (gameCompleted || !File.Exists(saveFilePath))
        {
            ResetSave();
        }
        // if (File.Exists(saveFilePath) || !gameCompleted)
        // {
        //     //LoadGame(); // Load saved game state if it exists
        // }
        else
        {
            //LoadGame();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Save when 'S' is pressed
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.L)) // Load when 'L' is pressed
        {
            LoadGame();
        }
    }

    void GetButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Card");
        for (int i = 0; i < objects.Length; i++)
        {
            var button = objects[i].GetComponent<Button>();
            btns.Add(button);
            btns[i].image.sprite = background;
            btns[i].onClick.AddListener(PickCard);
        }
    }

    void AddGameCards()
    {
        int index = 0;
        for (int i = 0; i < btns.Count; i++)
        {
            if (index == (btns.Count / 2))
            {
                index = 0;
            }
            gameCards.Add(cards[index]);
            index++;
        }
    }

    void ShuffleCards()
    {
        for (int i = cards.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    void ShuffleGameCards()
    {
        for (int i = gameCards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = gameCards[i];
            gameCards[i] = gameCards[randomIndex];
            gameCards[randomIndex] = temp;
        }
    }

    public void PickCard()
    {
        if (!firstGuess)
        {
            firstGuess = true;
            firstGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            firstGuessCard = gameCards[firstGuessIndex].name;

            StartCoroutine(FlipCard(btns[firstGuessIndex], gameCards[firstGuessIndex]));
            btns[firstGuessIndex].interactable = false;

            PlaySound(flipSound); // Play flip sound
        }
        else if (!secondGuess)
        {
            secondGuess = true;
            secondGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            secondGuessCard = gameCards[secondGuessIndex].name;

            StartCoroutine(FlipCard(btns[secondGuessIndex], gameCards[secondGuessIndex]));
            btns[secondGuessIndex].interactable = false;

            countGuesses++;
            PlaySound(flipSound); // Play flip sound
            StartCoroutine(CheckMatch());
        }
    }

    void CheckGameEnded()
    {
        correctGuesses++;
        if (correctGuesses == gameGuesses)
        {
            PlaySound(gameOverSound);
            gameCompleted = true;
            Debug.Log($"Game completed in {countGuesses} guesses.");
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstGuessCard == secondGuessCard)
        {
            PlaySound(matchSound); // Play match sound
            UpdateScore(10);  // Increase score for correct match
            IncreaseCombo();
            yield return new WaitForSeconds(0.5f);
            btns[firstGuessIndex].image.color = btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0); // Hide matched cards
            CheckGameEnded();
        }
        else
        {
            PlaySound(mismatchSound); // Play mismatch sound
            ResetCombo();  // Reset combo if mismatch
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(FlipCardBack(btns[firstGuessIndex]));
            StartCoroutine(FlipCardBack(btns[secondGuessIndex]));
            btns[firstGuessIndex].interactable = btns[secondGuessIndex].interactable = true;
        }

        firstGuess = secondGuess = false;
    }


    void UpdateScore(int points)
    {
        score += points * comboMultiplier;  // Apply combo multiplier to score
        scoreText.text = "Score: " + score;  // Update the score display
    }

    void IncreaseCombo()
    {
        currentCombo++;
        if (currentCombo >= 2)
        {
            comboMultiplier = currentCombo;  // Increase combo multiplier after 2 consecutive correct matches
        }
        comboText.text = "Combo: " + comboMultiplier;  // Update the combo display
    }

    void ResetCombo()
    {
        currentCombo = 0;
        comboMultiplier = 1;
        comboText.text = "Combo: " + comboMultiplier;  // Reset combo display
    }

    IEnumerator FlipCard(Button button, Sprite newSprite)
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / (duration / 2));
            rt.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        button.image.sprite = newSprite;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / (duration / 2));
            rt.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }
    }

    IEnumerator FlipCardBack(Button button)
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / (duration / 2));
            rt.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        button.image.sprite = background;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / (duration / 2));
            rt.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }
    }

    IEnumerator RevealAllCards()
    {
        // Show all cards
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].image.sprite = gameCards[i];
            btns[i].interactable = false;  // Disable interactions while cards are shown
        }

        // Wait for 1 second
        yield return new WaitForSeconds(1f);

        // Hide all cards again by setting them to the background
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].image.sprite = background;
            btns[i].interactable = true;  // Enable interaction
        }
    }

    void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    // Save the game state
    public void SaveGame()
    {
        saveData.score = score;
        saveData.comboMultiplier = comboMultiplier;
        saveData.currentCombo = currentCombo;
        saveData.correctGuesses = correctGuesses;
        saveData.gameCompleted = gameCompleted;
        saveData.gameCards = gameCards;

        // Save matched card indices
        saveData.matchedIndices.Clear();  // Clear previous indices
        for (int i = 0; i < btns.Count; i++)
        {
            if (btns[i].image.color.a == 0)  // Transparent cards are matched
            {
                saveData.matchedIndices.Add(i);
            }
        }

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game saved!");
    }


    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Save file not found. Starting a new game.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        saveData = JsonUtility.FromJson<SaveData>(json);

        // Restore game state
        score = saveData.score;
        comboMultiplier = saveData.comboMultiplier;
        currentCombo = saveData.currentCombo;
        correctGuesses = saveData.correctGuesses;
        gameCards = saveData.gameCards;
        //gameCompleted = saveData.gameCompleted;

        scoreText.text = "Score: " + score;  // Update score display
        comboText.text = "Combo: " + comboMultiplier;  // Update combo display

        // Restore matched cards and update the buttons' images
        for (int i = 0; i < btns.Count; i++)
        {
            if (saveData.matchedIndices.Contains(i))  // If the card is already matched
            {
                btns[i].image.color = new Color(0, 0, 0, 0);  // Hide matched card (transparent)
                btns[i].interactable = false;  // Disable interaction for matched card
            }
            else  // If the card is not matched
            {
                btns[i].image.sprite = saveData.gameCards[i];  // Restore the card image
                btns[i].interactable = true;  // Enable interaction for unmatched card
            }
        }

        Debug.Log("Game loaded successfully!");
    }


    // Reset the game save
    public void ResetSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save file deleted.");
        }

        // Optionally, reset the game state here
        score = 0;
        comboMultiplier = 1;
        currentCombo = 0;
        correctGuesses = 0;
        gameCompleted = false;

        scoreText.text = "Score: " + score;  // Reset score display
        comboText.text = "Combo: " + comboMultiplier;  // Reset combo display

        // Reset all cards to their default state
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].image.sprite = background;
            btns[i].image.color = Color.white;
            btns[i].interactable = true;
        }
        StartCoroutine(RevealAllCards());  // Show all cards for a short period
        Debug.Log("Game reset!");
    }


    // Save the game when the application quits
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
