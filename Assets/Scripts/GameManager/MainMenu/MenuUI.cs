using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public GameObject optionsTab;
    
    
    public void enableOptionsTab()
    {
        optionsTab.SetActive(true);
        
    }

    public void disableOptionsTab()
    {
        optionsTab.SetActive(false);
    }
}