using UnityEngine;

public class MoveInCircle : MonoBehaviour
{
    public float radius = 1.0f;
    public float timeScale = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var currentTime = Time.time;

        var xMovement = Mathf.Sin(currentTime * timeScale) * radius;
        var zMovement = Mathf.Cos(currentTime * timeScale) * radius;

        var movement = new Vector3(xMovement, 0.0f, zMovement);

        transform.Translate(movement * Time.deltaTime);
    }
}
