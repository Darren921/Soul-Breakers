using UnityEngine;

public class distanceTest : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float distance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
    }
}
