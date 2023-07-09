using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class CarsManager : MonoBehaviour
{
    [Header("Panels")] [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Toggles")] public SwitchToggle musicToggle;
    public SwitchToggle soundToggle;
    public SwitchToggle vibroToggle;

    [Header("Sliders")] public Slider speedSlider;
    public Slider accelerationSlider;
    public Slider strengthSlider;

    [Header("Texts")] public TextMeshProUGUI coinsTxt;
    public TextMeshProUGUI starsTxt;
    public TextMeshProUGUI nameTxt;
    public TextMeshProUGUI speedTxt;
    public TextMeshProUGUI accelerationTxt;
    public TextMeshProUGUI strengthTxt;
    public TextMeshProUGUI priceTxt;
    public TextMeshProUGUI unlockLvlTxt;
    public TextMeshProUGUI infoTxt;

    [Header("GameObjects")] public GameObject upgradeButtons;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public AudioManager audioManager;
    public List<CarData> carsData = new List<CarData>();

    private CarData selectedCarData;
    private int lastOwnedCar;
    private int level;
    private int coins;
    private int stars;
    private Transform car;
    private bool delayFinished;

    private void Awake()
    {
        level = PlayerPrefs.GetInt("level", 1);
        coins = PlayerPrefs.GetInt("coins", 0);
        var carId = PlayerPrefs.GetInt("carId", 0);
        selectedCarData = carsData[carId];
        lastOwnedCar = carId;
        SetUpSettings();
        SetGainedStars();
        SetGainedCoins();
        SetSlidersMaxValues();
        ShowSelectedCar();
        SetUpArrows();
    }

    private void Update()
    {
        if (car != null && delayFinished) car.transform.Rotate(Vector3.up * 10 * Time.deltaTime, Space.Self);
    }

    public void UpgradeCar()
    {
        if (selectedCarData.price <= coins && selectedCarData.unlockLvl <= stars)
        {
            print("upgrade");
            audioManager.Vibrate();
            audioManager.PlaySoundEffect(audioManager.click);
            coins -= selectedCarData.price;
            PlayerPrefs.SetInt("coins", coins);
            SetGainedCoins();
            upgradeButtons.SetActive(false);
            PlayerPrefs.SetInt("carId", selectedCarData.id);
            lastOwnedCar = selectedCarData.id;
        }
        else
        {
            print("Not Enough");
        }
    }

    public void GoToPlayScene()
    {
        audioManager.Vibrate();
        audioManager.PlaySoundEffect(audioManager.click);

        if (level > 10)
            level = Random.Range(1, 11);

        SceneManager.LoadScene(level + 1);
    }

    public void ScrollLeft()
    {
        audioManager.Vibrate();
        audioManager.PlaySoundEffect(audioManager.click);
        selectedCarData = carsData[selectedCarData.id - 1];
        ShowSelectedCar();
        SetUpArrows();
    }

    public void ScrollRight()
    {
        audioManager.Vibrate();
        audioManager.PlaySoundEffect(audioManager.click);
        selectedCarData = carsData[selectedCarData.id + 1];
        ShowSelectedCar();
        SetUpArrows();
    }

    private void ShowSelectedCar()
    {
        if (car != null)
            Destroy(car.gameObject);

        nameTxt.text = selectedCarData.name;
        speedTxt.text = selectedCarData.speed.ToString();
        accelerationTxt.text = selectedCarData.acceleration.ToString();
        strengthTxt.text = selectedCarData.strength.ToString();
        speedSlider.value = selectedCarData.speed;
        accelerationSlider.value = selectedCarData.acceleration;
        strengthSlider.value = selectedCarData.strength;
        priceTxt.text = selectedCarData.price.ToString();
        unlockLvlTxt.text = selectedCarData.unlockLvl.ToString();
        car = Instantiate(selectedCarData.model).transform;
        delayFinished = false;
        StartCoroutine("SetUpDelay");

        upgradeButtons.SetActive(lastOwnedCar < selectedCarData.id);

        if (selectedCarData.price > coins || selectedCarData.unlockLvl > stars)
            infoTxt.gameObject.SetActive(true);
        else
            infoTxt.gameObject.SetActive(false);
    }

    private IEnumerator SetUpDelay()
    {
        yield return new WaitForSeconds(1.2f);
        delayFinished = true;
    }

    private void SetUpArrows()
    {
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);

        if (selectedCarData.id == 0)
            leftArrow.SetActive(false);

        if (selectedCarData.id == carsData.Count - 1)
            rightArrow.SetActive(false);
    }

    private void SetSlidersMaxValues()
    {
        speedSlider.maxValue = 150;
        accelerationSlider.maxValue = 100;
        strengthSlider.maxValue = 100;
    }

    private void SetGainedStars()
    {
        var l = level - 1;
        //stars = l * 3;
        stars = 200;
        starsTxt.text = stars + " <#60594f>/300";
    }

    private void SetGainedCoins()
    {
        var l = coins.ToString().Length;

        var str = coins.ToString();

        if (l < 7)
            for (int i = l; i < 7; i++)
                str = 0 + str;

        str = str.Insert(1, ",");
        str = str.Insert(5, ",");

        coinsTxt.text = str;
    }

    public void OpenSettings()
    {
        audioManager.Vibrate();
        audioManager.PlaySoundEffect(audioManager.click);
        upgradePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        audioManager.Vibrate();
        audioManager.PlaySoundEffect(audioManager.click);
        settingsPanel.SetActive(false);
        upgradePanel.SetActive(true);
    }

    private void SetUpSettings()
    {
        var music = PlayerPrefs.GetInt("music", 1) == 1;
        var sound = PlayerPrefs.GetInt("sound", 1) == 1;
        var vibro = PlayerPrefs.GetInt("vibro", 1) == 1;

        musicToggle.SetState(music);
        soundToggle.SetState(sound);
        vibroToggle.SetState(vibro);
        audioManager.InitializeStates(music, sound, vibro);
    }
}