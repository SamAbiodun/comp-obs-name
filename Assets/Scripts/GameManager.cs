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

    private int guesses, correctGuesses, gameGuesses, firstGuessIndex, secondGuessIndex;
    private string firstGuessName, secondGuessName;

    private void Awake()
    {
        cards = Resources.LoadAll<Sprite>("Sprites/Icons");
    }

    private void Start()
    {
        GetButtons();
        ShuffleCards();
        AddGameCards();
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

    
    public void PickCard()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        Debug.Log("clicked card with name " + name);
    }
}
