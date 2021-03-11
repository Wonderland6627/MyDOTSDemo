using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public static UIManager Instance { get => _instance; set => _instance = value; }

    public Slider playerHPSlider;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {
        playerHPSlider.gameObject.SetActive(false);
    }

    public void InitPlayerHPSlider(float maxValue)
    {
        playerHPSlider.gameObject.SetActive(true);
        playerHPSlider.maxValue = maxValue;
    }

    public void UpdatePlayerHPSlider(float value)
    {
        playerHPSlider.value = value;
    }
}
