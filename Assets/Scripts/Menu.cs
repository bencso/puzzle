using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
   public void GoToGame()
   {
      SceneManager.LoadScene("level1");
   }

   public void GoToMenu()
   {
      if (LevelHelper.getCurrentPlayedLevel() >= 0)
      {
         PipeHelper.reset();
      }
      SceneManager.LoadScene("MainMenu");

   }
   public void GoToLevels()
   {
      MenuHelper.buttonFunction();
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
