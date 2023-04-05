using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("aaaaa");
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("BattleScene");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("aaaaa");
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Entering Player");
        }
    }
}
