using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuScripts : MonoBehaviour
{
    [SerializeField] Animator UIAnim;
    [SerializeField] Animator WorldUIanim;
    [SerializeField] Animator CameraAnim;



    [SerializeField] GameObject Character1;
    [SerializeField] GameObject Character2;
    [SerializeField] Transform PlayerArea1;
    [SerializeField] Transform PlayerArea2;

    [SerializeField] bool Player1Ready;
    [SerializeField] bool Player2Ready;
    [SerializeField] bool snapPlayerPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Character1.SetActive(true);
        Character2.SetActive(true);
        UIAnim = GameObject.Find("Canvas").GetComponent<Animator>();
        WorldUIanim = GameObject.Find("SceneCanvas").GetComponent<Animator>();
        snapPlayerPos = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ( snapPlayerPos)
        {
            Character1.transform.position = PlayerArea1.position;
            Character2.transform.position = PlayerArea2.position;
        }
        
    }
    public void Char1Active1()
    {

        Character1.SetActive(true);
        Character1.transform.position = PlayerArea1.position;

    }
    public void Char2Active1()
    {
        Character2.SetActive(true);
        Character2.transform.position = PlayerArea1.position;

    }
    public void DisableChar1()
    {
        Character1.SetActive(false);
    }
    public void DisableChar2()
    {
        Character2.SetActive(false);
    }
    public void CharacterSelect()
    {
        StartCoroutine(StartChar());
    }
    public void mainmenu()
    {
        StartCoroutine(back2Menu());
    }

    public IEnumerator StartChar()
    {
        UIAnim.Play("ToChar");
        yield return new WaitForSeconds(0.5f);
        WorldUIanim.Play("CharSelect");
        yield return new WaitForSeconds(0.5f);
        UIAnim.Play("ReadyAppear");

    }
    IEnumerator back2Menu()
    {
        WorldUIanim.Play("BackToMain");
        yield return new WaitForSeconds(0.5f);
        UIAnim.Play("MenuStart");
    }
    private IEnumerator StartGame()
    {
        CameraAnim.Play("StartGame");
        WorldUIanim.Play("StartGameSceneCanvas");
        UIAnim.Play("Ready Dissapear");
        yield return new WaitForSeconds(1f);
        UIAnim.Play("Start");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("CombatScene");
    }
    public void GameStart()
    {
        
        StartCoroutine(StartGame());
    }

    
   
}
