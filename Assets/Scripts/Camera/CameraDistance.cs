using System.Net.NetworkInformation;
using UnityEngine;

public class CameraDistance : MonoBehaviour
{

    [SerializeField] GameObject Player1;
    [SerializeField] GameObject Player2;
    private Camera cam;
    [SerializeField] Vector3 playerView1;
    [SerializeField] GameObject wall;
    [SerializeField] GameObject wall2;
   
    
    [SerializeField] Vector3 playerView2;
    [SerializeField] Transform P1wallR;
    [SerializeField] Transform P1wallL;


    [SerializeField] Transform WallStartPos;
    [SerializeField] GameObject Tracker1;
    [SerializeField] GameObject Tracker2;


    public float distance; 
    void Start()
    {
        cam = GetComponent<Camera>();
    }

        void Update()
        {
            Tracker1.transform.position = Player1.transform.position + new Vector3(0,2,0);
            Tracker2.transform.position = Player2.transform.position + new Vector3(0,2,0);
            
            /*
            playerView1 = cam.WorldToViewportPoint(Player1.transform.position);
            playerView2 = cam.WorldToViewportPoint(Player2.transform.position);
            distance = Vector3.Distance(Player1.transform.position, Player2.transform.position);
            if (playerView1.x <= 0.08 && distance > 20)
            {
                wall.transform.position = P1wallL.transform.position;
            }
            else if (playerView1.x >= 0.1 && distance < 20) 
            {
                wall.transform.position = WallStartPos.transform.position;
            }
            if (playerView1.x >0.9f && distance > 20)
            {
                wall2.transform.position = P1wallR.transform.position;
            }
            else if (playerView1.x <= 0.9 && distance < 20)
            {
                wall2.transform.position = WallStartPos.transform.position;
            }   
            */

    }
        
    
}
