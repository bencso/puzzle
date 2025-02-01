using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuHelper : MonoBehaviour
{
   [SerializeField]
   private TMP_Text buttonTextHelper;

   public static string buttonText;
   public static string successText;

   private void Awake()
   {
      buttonText = successText == "randomlevel" ? "RANDOM SZINT" : "VISSZA A VÁLASZTÓ MENÜBE";
      buttonTextHelper.text = buttonText;
   }

   public static void buttonFunction()
   {
      SceneManager.LoadScene(successText == "randomlevel" ? "randomLevel" : "LevelMenu");
   }
}