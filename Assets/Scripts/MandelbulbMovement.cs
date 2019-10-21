using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandelbulbMovement : MonoBehaviour
{
    public float timeScaleMovement = 0.2f;
    public float moveDistance = 3.0f;

    public float timeScalePower = 0.5f;
    public float baserPower = 9.0f;
    public float powerScale = 4.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var currentTime = Time.time;

        // Move up and down
        var yMovement = Mathf.Sin(currentTime * timeScaleMovement) * moveDistance;
        var movement = new Vector3(0.0f, yMovement, 0.0f);

        transform.Translate(movement * Time.deltaTime);

        // Change power
        var power = baserPower + Mathf.Sin(currentTime * timeScalePower) * powerScale;

        transform.localScale = new Vector3(1.0f, power, 1.0f);
    }
}
