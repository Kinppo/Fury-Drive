using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum GameState
{
    Start,
    Ready,
    Play,
    Pause,
    Lose,
    Win
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; protected set; }
    public static GameState gameState = GameState.Play;
    public Transform camPoint;
    public Transform startPosition;
    public float distance = 1.6f;
    public int killedEnemies;
    public List<GameObject> objects = new List<GameObject>();
    public List<CarData> carsData = new List<CarData>();

    [HideInInspector] public int level;
    [HideInInspector] public bool messageHasShown;

    [HideInInspector] public List<Car> cars = new List<Car>();
    [HideInInspector] public Player player;
    private GameState prevState;
    private float deltaTime;

    void Awake()
    {
        Instance = this;
        level = PlayerPrefs.GetInt("level", 1);
        messageHasShown = PlayerPrefs.GetInt("message", 0) == 1;
        var carId = PlayerPrefs.GetInt("carId", 0);
        player = Instantiate(carsData[carId].playablePrefab, startPosition.position, startPosition.rotation);
        TinySauce.OnGameStarted(level.ToString());
    }

    void Update()
    {
        if (gameState == GameState.Play)
        {
            CameraMovement();
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
    }

    private IEnumerator InstantiateObjects()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            yield return new WaitForSeconds(i * 0.01f);
            Instantiate(objects[i]);
        }

        yield return null;
    }

    // private void OnGUI()
    // {
    //     int fps = Mathf.RoundToInt(1.0f / deltaTime);
    //     string text = string.Format("FPS: {0}", fps);
    //
    //     GUIStyle style = new GUIStyle(GUI.skin.label);
    //     style.normal.textColor = Color.black;
    //     style.fontSize = 42;
    //     style.fontStyle = FontStyle.Bold;
    //     style.alignment = TextAnchor.UpperLeft;
    //
    //     GUI.Label(new Rect(6, 6, 256, 256), text, style);
    // }

    private void CameraMovement()
    {
        camPoint.position =
            new Vector3(player.transform.position.x, camPoint.position.y, player.transform.position.z);
        Quaternion targetRotation = Quaternion.Euler(0f, player.transform.rotation.eulerAngles.y, 0f);
        camPoint.rotation = Quaternion.Slerp(camPoint.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void OpenGarage()
    {
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.click);
        SceneManager.LoadScene(1);
    }

    public void OpenSettings()
    {
        prevState = gameState;
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.click);
        UIManager.Instance.SetPanel(GameState.Pause);
    }

    public void CloseSettings()
    {
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.click);
        UIManager.Instance.SetPanel(prevState);
    }

    public void Win()
    {
        UIManager.Instance.FillWinData();
        UIManager.Instance.SetPanel(GameState.Win);
        AudioManager.Instance.ChangeEngineVolume(true);
        AudioManager.Instance.engineSoundSource.mute = true;
        AudioManager.Instance.soundEffectSource.mute = true;
        TinySauce.OnGameFinished(true, 1, level.ToString());
    }

    public void Lose()
    {
        AudioManager.Instance.ChangeEngineVolume(true);
        UIManager.Instance.FillLoseData();
        UIManager.Instance.SetPanel(GameState.Lose);
        TinySauce.OnGameFinished(false, 1, level.ToString());
    }

    public void Play()
    {
        StartCoroutine(InstantiateObjects());
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.click);
        UIManager.Instance.SetPanel(GameState.Ready);
    }

    public void Restart()
    {
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.click);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Next()
    {
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.click);
        level++;
        PlayerPrefs.SetInt("level", level);
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (level > 10)
        {
            level = Random.Range(3, 11);
        }

        SceneManager.LoadScene(level + 1);
    }

    // [ContextMenu("AutoFill")]
    // void AutoFillList()
    // {
    //     items = GetComponentsInChildren<GameObject>().Where(i => i.name.ToLower().Contains("Wheel")).ToList();
    // }
}