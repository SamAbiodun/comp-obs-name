using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCards : MonoBehaviour
{
    //Grid Layout
    public int rows = 2;
    public int columns = 2;

    
    [SerializeField] private Transform board;
    [SerializeField] private GameObject crd; //card
    
    private void Awake()
    {
        for (int i = 0; i < (rows * columns); i++)
        {
            GameObject card = Instantiate(crd);
            card.name = $"{i}";
            card.transform.SetParent(board, false);
        }
    }
}
