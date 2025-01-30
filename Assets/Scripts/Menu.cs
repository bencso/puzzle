using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
   public void GoToGame()
   {
      SceneManager.LoadScene("level1");
   }

   public void GoToMenu()
   {
      SceneManager.LoadScene("MainMenu");

   }
   public void GoToLevels(){
      SceneManager.LoadScene("LevelMenu");
   }

   public void GoToRandomLevel()
   {
      SceneManager.LoadScene("randomLevel");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
