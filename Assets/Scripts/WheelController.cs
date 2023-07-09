using System;
using System.Collections;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public GameObject[] wheelsToRotate;
    public GameObject trails;

    public float rotationSpeed;
    public Animator anim;

    public void RotateWheels(float verticalAxis, float horizontalAxis)
    {
        foreach (var wheel in wheelsToRotate)
        {
            wheel.transform.Rotate(Time.deltaTime * verticalAxis * rotationSpeed, 0, 0, Space.Self);
        }

        if (horizontalAxis > 0.25 && !anim.GetBool("toRight"))
        {
            anim.SetBool("toLeft", false);
            anim.SetBool("toRight", true);
        }
        else if (horizontalAxis < -0.25 && !anim.GetBool("toLeft"))
        {
            anim.SetBool("toRight", false);
            anim.SetBool("toLeft", true);
        }
        else if (horizontalAxis == 0)
        {
            anim.SetBool("toRight", false);
            anim.SetBool("toLeft", false);
        }

        ShowTrails(horizontalAxis);
    }

    private void ShowTrails(float horizontalAxis)
    {
        if (horizontalAxis >= 0.5 || horizontalAxis <= -0.5)
            trails.SetActive(true);
        else
            StartCoroutine("HideTrails");
    }

    private IEnumerator HideTrails()
    {
        yield return new WaitForSeconds(0.25f);
        trails.SetActive(false);
    }
}