using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelHelper : MonoBehaviour
{
    [SerializeField]
    public Font font;

    public GameObject levelButtonPrefab;

    int level;

    void Start()
    {
        if (PlayerPrefs.GetInt("level") > 0)
        {
            level = PlayerPrefs.GetInt("level");
        }
        else
        {
            level = 1;
            PlayerPrefs.SetInt("level", level);
            PlayerPrefs.Save();
        }
        int levelCount = GetLevelCount();
        GameObject buttonGrid = GameObject.Find("layout");
        for (int i = 0; i < levelCount; i++)
        {
            int currentLevel = i + 1;

            GameObject newButton = Instantiate(levelButtonPrefab);
            newButton.SetActive(true);

            newButton.transform.SetParent(buttonGrid.transform);
            newButton.transform.localScale = new Vector3(1, 1, 1);

            Button buttonComponent = newButton.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => LoadLevel(currentLevel));

            buttonComponent.interactable = currentLevel <= level;

            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = $"{currentLevel}. PÁLYA";
            buttonText.color = currentLevel <= level ? Color.white : Color.gray;
        }
    }


    int GetLevelCount()
    {
        int count = 0;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string lowerPath = scenePath.ToLower();
            if (lowerPath.Contains("level") &&
                lowerPath.IndexOf("level") + 5 < lowerPath.Length &&
                char.IsDigit(lowerPath[lowerPath.IndexOf("level") + 5]))
            {
                count++;
            }
        }
        return count;
    }

    void LoadLevel(int level)
    {
        SceneManager.LoadScene("level" + level);
    }

    public static void setMaxLevel()
    {
        int currentLevel = getCurrentPlayedLevel()+1;
        if (currentLevel > PlayerPrefs.GetInt("level"))
        {
            PlayerPrefs.SetInt("level", currentLevel);
            PlayerPrefs.Save();
        }
    }

    public static int getCurrentPlayedLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name.ToLower();
        if (currentSceneName.Contains("level") && currentSceneName != "randomlevel")
        {
            string levelNumber = currentSceneName.Replace("level", "");
            if (int.TryParse(levelNumber, out int result))
            {
                return result;
            }
        }
        if(currentSceneName == "randomlevel"){
            return 0;
        }
        return -1;
    }
}