using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
   public void GoToGame()
   {
      Debug.Log("Go to game");
      SceneManager.LoadScene("level1");
   }

   public void GoToMenu()
   {
      SceneManager.LoadScene("menu");
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
