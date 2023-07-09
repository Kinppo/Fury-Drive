using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Slider loadingBarFill;

    private void Awake()
    {
        SetLevel();
    }

    private void SetLevel()
    {
        var level = PlayerPrefs.GetInt("level", 1);
        if (level > 10)
        {
            level = Random.Range(1, 11);
        }

        //SceneManager.LoadScene(level + 1);
        LoadScene(level + 1);
    }

    private async void LoadScene(int sceneId)
    {
        await Task.Delay(5);
        var operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false;

        do
        {
            loadingBarFill.value = operation.progress * 100;
        } while (operation.progress < 0.9f);

        operation.allowSceneActivation = true;
    }
}