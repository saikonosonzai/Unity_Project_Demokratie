using AdventurePuzzleKit;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;


public class CandleController : MonoBehaviour
{
    public GameObject candle;


    void Start()
    {
    }

    void Update()
    {
    }

    public void OnTrigger()
    {
        candle.SetActive(true);
        gameObject.SetActive(false);
        
    }
}
