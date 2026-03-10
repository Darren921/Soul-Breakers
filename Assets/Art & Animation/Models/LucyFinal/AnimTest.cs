using UnityEngine;

public class AnimTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponent<Animator>().Play("LucyIdle");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
