using System.Collections;
using UnityEngine;

public enum CarType
{
    Player,
    Enemy
}

public enum CarState
{
    Ready,
    OnRoad,
    Finished
}

public class Car : MonoBehaviour
{
    public CarState carState;
    public CarType carType;

    private void Update()
    {
        if (carState == CarState.Ready && GameManager.gameState == GameState.Play)
            carState = CarState.OnRoad;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 16) return;

        StartCoroutine("SetUpCarFinished");
        GameManager.Instance.cars.Add(this);
        if (carType == CarType.Player)
            GameManager.Instance.Win();
    }

    private IEnumerator SetUpCarFinished()
    {
        yield return new WaitForSeconds(0.5f);
        carState = CarState.Finished;
    }
}