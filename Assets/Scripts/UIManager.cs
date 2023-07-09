using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LevelIcon
{
    public Image background;
    public TextMeshProUGUI number;
    public GameObject stars;
    public GameObject lockIcon;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; protected set; }

    [Header("Panels")] [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Texts")] public TextMeshProUGUI startCoins;
    public TextMeshProUGUI startStars;
    public TextMeshProUGUI playCoins;
    public TextMeshProUGUI winScore;
    public TextMeshProUGUI winCoins;
    public TextMeshProUGUI winKm;
    public TextMeshProUGUI winKilled;
    public TextMeshProUGUI lostCoins;
    public TextMeshProUGUI lostKm;
    public TextMeshProUGUI lostKilled;
    public TextMeshProUGUI counter;

    [Header("Lists")] public List<LevelIcon> levelIcons = new List<LevelIcon>();
    public List<GameObject> pagination = new List<GameObject>();
    public List<GameObject> hearts = new List<GameObject>();

    [Header("Sprites")] public Sprite levelSelectedBg;
    [Header("Toggles")] public SwitchToggle musicToggle;
    public SwitchToggle soundToggle;
    public SwitchToggle vibroToggle;

    [Header("Others")] public FloatingJoystick joystick;
    public CanvasGroup bloodOnScreenEffect;
    public GameObject ready;
    public GameObject go;
    public DOTweenAnimation fadeAnim;
    public CanvasGroup startCanvasGroup;

    private int level;
    private int coins;
    private int page;
    private bool animateBloodScreen;
    private float targetAlpha = 1;
    private int step;
    private Tween fadeTween;

    private void Awake()
    {
        Instance = this;
        level = PlayerPrefs.GetInt("level", 1);
        coins = PlayerPrefs.GetInt("coins", 0);
        page = level / 18;
        if (level % 18 == 0) page -= 1;

        SetPanel();
        SetGainedStars();
        SetGainedCoins();
        SetUpLevelsMap();
        SetUpSettings();
    }

    private void Update()
    {
        if (animateBloodScreen) UpdateBloodOnScreenEffect();
    }

    private void SetGainedStars()
    {
        var l = level - 1;
        startStars.text = l * 3 + " <#60594f>/54";
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

        startCoins.text = str;
        playCoins.text = str;
    }

    private void SetUpLevelsMap()
    {
        for (int i = 0; i < levelIcons.Count; i++)
        {
            if ((i + (page * 18)) == (level - 1))
            {
                levelIcons[i].background.sprite = levelSelectedBg;
                levelIcons[i].lockIcon.SetActive(false);
                levelIcons[i].number.gameObject.SetActive(true);
                levelIcons[i].number.text = ((page * 18) + i + 1).ToString();
            }
            else if ((i + (page * 18)) < (level - 1))
            {
                levelIcons[i].lockIcon.SetActive(false);
                levelIcons[i].number.gameObject.SetActive(true);
                levelIcons[i].stars.gameObject.SetActive(true);
                levelIcons[i].number.transform.position += new Vector3(0, 23, 0);
                levelIcons[i].number.text = ((page * 18) + i + 1).ToString();
            }
        }

        pagination[page].SetActive(true);
    }

    private void UpdateBloodOnScreenEffect()
    {
        bloodOnScreenEffect.alpha = Mathf.Lerp(bloodOnScreenEffect.alpha, targetAlpha, Time.deltaTime * 3);
    }

    private IEnumerator ResetBloodOnScreenEffect()
    {
        yield return new WaitForSeconds(0.7f);
        targetAlpha = 0;
    }

    public void UpdateHealthSlider()
    {
        if (Player.Instance.isDead) return;
        Player.Instance.health--;
        animateBloodScreen = true;
        targetAlpha = 1;
        hearts[Player.Instance.health].SetActive(false);
        StartCoroutine("ResetBloodOnScreenEffect");

        if (Player.Instance.health == 0)
        {
            Player.Instance.InitializeCrash();
            Player.Instance.isDead = true;
            GameManager.Instance.Lose();
        }
    }

    public void FillWinData()
    {
        var d = GameManager.Instance.distance;
        var k = GameManager.Instance.killedEnemies;
        var c = d * 10 + k * 25;
        var s = d * 275 + k * 450;
        winKilled.text = k.ToString();
        winKm.text = d + "km";
        winCoins.text = c.ToString();
        winScore.text = s.ToString();
    }

    public void FillLoseData()
    {
        var d = GameManager.Instance.distance;
        var k = GameManager.Instance.killedEnemies;
        lostKilled.text = k.ToString();
        lostKm.text = d + "km";
        lostCoins.text = "0";
    }

    private IEnumerator GetReady(float time)
    {
        yield return new WaitForSeconds(time);
        switch (step)
        {
            case 0:
                ready.SetActive(true);
                StartCoroutine("GetReady", 0.7f);
                break;
            case 1:
                ready.SetActive(false);
                counter.gameObject.SetActive(true);
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.count1);
                StartCoroutine("GetReady", 1f);
                break;
            case 2:
                counter.text = "2";
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.count2);
                StartCoroutine("GetReady", 1f);
                break;
            case 3:
                counter.text = "3";
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.count3);
                StartCoroutine("GetReady", 1f);
                break;
            case 4:
                counter.gameObject.SetActive(false);
                go.SetActive(true);
                fadeAnim.gameObject.SetActive(true);
                fadeAnim.DOPlay();
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.count4);
                StartCoroutine("GetReady", 0.4f);
                break;
            case 5:
                go.SetActive(false);
                GameManager.gameState = GameState.Play;
                joystick.gameObject.SetActive(true);
                GameManager.Instance.player.StartCoroutine("EndReloadTime");
                AudioManager.Instance.engineSoundSource.enabled = true;
                break;
        }

        step++;
    }

    private void SetUpSettings()
    {
        var music = PlayerPrefs.GetInt("music", 1) == 1;
        var sound = PlayerPrefs.GetInt("sound", 1) == 1;
        var vibro = PlayerPrefs.GetInt("vibro", 1) == 1;

        musicToggle.SetState(music);
        soundToggle.SetState(sound);
        vibroToggle.SetState(vibro);
        AudioManager.Instance.InitializeStates(music, sound, vibro);
    }

    private void FadeIn(float duration)
    {
        Fade(1f, duration, () =>
        {
            startCanvasGroup.interactable = true;
            startCanvasGroup.blocksRaycasts = true;
        });
    }

    private void FadeOut(float duration)
    {
        Fade(0f, duration, () =>
        {
            startCanvasGroup.interactable = false;
            startCanvasGroup.blocksRaycasts = false;
            playPanel.SetActive(true);
            StartCoroutine("GetReady", 0);
        });
    }

    private void Fade(float endValue, float duration, TweenCallback onEnd)
    {
        if (fadeTween != null)
            fadeTween.Kill();

        fadeTween = startCanvasGroup.DOFade(endValue, duration);
        fadeTween.onComplete += onEnd;
    }

    public void SetPanel(GameState state = GameState.Start)
    {
        playPanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        if (GameManager.gameState == GameState.Start && state == GameState.Ready)
            FadeOut(0.5f);
        else
            startPanel.SetActive(false);

        switch (state)
        {
            case GameState.Start:
                startPanel.SetActive(true);
                break;
            // case GameState.Ready:
            //     playPanel.SetActive(true);
            //     StartCoroutine("GetReady", 0);
            //     break;
            case GameState.Play:
                playPanel.SetActive(true);
                break;
            case GameState.Pause:
                pausePanel.SetActive(true);
                break;
            case GameState.Win:
                winPanel.SetActive(true);
                break;
            case GameState.Lose:
                losePanel.SetActive(true);
                break;
        }

        GameManager.gameState = state;
    }
}