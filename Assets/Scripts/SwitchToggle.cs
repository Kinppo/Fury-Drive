using UnityEngine;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] private GameObject on;
    [SerializeField] private GameObject off;
    public new string name = "";
    public bool state = true;

    public void SetState(bool newState)
    {
        PlayerPrefs.SetInt(name, newState ? 1 : 0);

        if (state != newState)
        {
            on.SetActive(newState);
            off.SetActive(!newState);
        }

        state = newState;
    }
}