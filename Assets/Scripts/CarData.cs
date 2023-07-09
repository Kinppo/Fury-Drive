using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Car Details")]
public class CarData : ScriptableObject
{
    public int id;
    public string name;
    public float speed;
    public float acceleration;
    public float strength;
    public int price;
    public int unlockLvl;
    public GameObject model;
    public Player playablePrefab;
}