using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public List<Button> btns = new List<Button>();
    [SerializeField] private Sprite background;

    public Sprite[] cards;
    public List<Sprite> gameCards = new List<Sprite>();

    private bool firstGuess, secondGuess;
    private int countGuesses, correctGuesses, gameGuesses, firstGuessIndex, secondGuessIndex;
    private string firstGuessCard, secondGuessCard;

    private void Awake()
    {
        cards = Resources.LoadAll<Sprite>("Sprites/Icons");
    }

    private void Start()
    {
        GetButtons();
        ShuffleCards();
        AddGameCards();
        ShuffleGameCards();
        gameGuesses = gameCards.Count / 2;
        StartCoroutine(RevealAllCards());
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
        }
        else if (!secondGuess)
        {
            secondGuess = true;
            secondGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            secondGuessCard = gameCards[secondGuessIndex].name;

            StartCoroutine(FlipCard(btns[secondGuessIndex], gameCards[secondGuessIndex]));
            btns[secondGuessIndex].interactable = false;

            countGuesses++;
            StartCoroutine(CheckMatch());
        }
    }

    void CheckGameEnded()
    {
        correctGuesses++;
        if (correctGuesses == gameGuesses)
        {
            Debug.Log($"Game completed in {countGuesses} guesses.");
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstGuessCard == secondGuessCard)
        {
            yield return new WaitForSeconds(0.5f);
            btns[firstGuessIndex].image.color = btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
            CheckGameEnded();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(FlipCardBack(btns[firstGuessIndex]));
            StartCoroutine(FlipCardBack(btns[secondGuessIndex]));
            btns[firstGuessIndex].interactable = btns[secondGuessIndex].interactable = true;
        }

        firstGuess = secondGuess = false;
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
            btns[i].interactable = true;  // Enable interactions after cards are hidden
        }
    }
}
