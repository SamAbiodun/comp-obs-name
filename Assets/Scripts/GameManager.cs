using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    public List<Button> btns = new List<Button>();
    [SerializeField] private Sprite background;

    public Sprite[] cards;
    public List<Sprite> gameCards = new List<Sprite>();

    //check to see which guess has been made
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
    }

    void GetButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Card");
        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<Button>());
            btns[i].image.sprite = background;
            btns[i].onClick.AddListener(PickCard);
        }
    }

    void AddGameCards()
    {
        int index = 0;
        for (int i = 0; i < btns.Count; i++)
        {
            if (index == (btns.Count/2))
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
        //get index
        Debug.Log("clicked card with name " + name);

        if (!firstGuess)
        {
            firstGuess = true;
            firstGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            Debug.Log("clicked card with name " + EventSystem.current.currentSelectedGameObject.name);
            //get image name
            firstGuessCard = gameCards[firstGuessIndex].name;
            btns[firstGuessIndex].interactable = false;
            btns[firstGuessIndex].image.sprite = gameCards[firstGuessIndex];
        }
        
        else if (!secondGuess)
        {
            secondGuess = true;
            secondGuessIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name);
            Debug.Log("clicked card with name " + EventSystem.current.currentSelectedGameObject.name);
            //get image name
            secondGuessCard = gameCards[secondGuessIndex].name;
            btns[secondGuessIndex].interactable = false;
            btns[secondGuessIndex].image.sprite = gameCards[secondGuessIndex];
            countGuesses++;
            StartCoroutine(CheckMatch());
        }

        
    }

    void CheckGameEnded()
    {
        correctGuesses++;
        if (correctGuesses == gameGuesses)
        {
            Debug.Log($"it took you {countGuesses} to finish the game");
        }
    }
    
    
    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1f);
        if (firstGuessCard == secondGuessCard)
        {
            yield return new WaitForSeconds(0.5f);
            //make uninterractable
            //btns[firstGuessIndex].interactable = btns[secondGuessIndex].interactable = false;
            
            //make transparent
            btns[firstGuessIndex].image.color = btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
            CheckGameEnded();
        }

        else
        {
            yield return new WaitForSeconds(0.5f);
            //if wrong, revert
            btns[firstGuessIndex].image.sprite = btns[secondGuessIndex].image.sprite = background;
            btns[firstGuessIndex].interactable = btns[secondGuessIndex].interactable = true;
        }

        yield return new WaitForSeconds(0.5f);
        firstGuess = secondGuess = false;
    }
}
