using System.Collections;
using TMPro;
using UnityEditor.Build;
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
    [SerializeField] Animator PlayerAnim;
    [SerializeField] Animator player2Anim;


    [SerializeField] GameObject Character1;
    [SerializeField] GameObject Character2;
    [SerializeField] Transform PlayerArea1;
    [SerializeField] Transform PlayerArea2;

    [SerializeField] bool Player1Ready;
    [SerializeField] bool Player2Ready;
    [SerializeField] bool setPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player2Anim.speed = 0;
        PlayerAnim.speed = 0;
        Character1.SetActive(true);
        Character2.SetActive(true);
        UIAnim = GameObject.Find("Canvas").GetComponent<Animator>();
        WorldUIanim = GameObject.Find("SceneCanvas").GetComponent<Animator>();
        PlayerAnim.Play("exit1");
        player2Anim.Play("exit2");
        setPos = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (setPos)
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
        setPos = false;
        Character1.transform.parent = null;
        Character2.transform.parent = null;
        CameraAnim.Play("startScene");
        player2Anim.speed = 1;
        PlayerAnim.speed = 1;
        WorldUIanim.Play("StartGameSceneCanvas");
        UIAnim.Play("Ready Dissapear");
        yield return new WaitForSeconds(3.5f);
        UIAnim.Play("Start");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);

    }
    public void GameStart()
    {
        
        StartCoroutine(StartGame());
    }

    
   
}
