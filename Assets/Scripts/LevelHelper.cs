using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelHelper : MonoBehaviour
{
    [SerializeField]
    public Font font;

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
        GameObject buttonGrid = GameObject.Find("ButtonGrid");
        for (int i = 0; i < levelCount; i++)
        {
            int currentLevel = i + 1;
            GameObject button = new GameObject("Level" + currentLevel);
            button.transform.SetParent(buttonGrid.transform, false);
            button.AddComponent<RectTransform>();
            button.AddComponent<Button>();
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100);

            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => LoadLevel(currentLevel));

            buttonComponent.interactable = currentLevel <= level;

            Text buttonText = button.AddComponent<Text>();
            buttonText.text = $"{currentLevel}";
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontSize = 100;
            buttonText.color = currentLevel <= level ? Color.white : Color.gray;
            buttonText.resizeTextForBestFit = true;
            buttonText.font = font;
            buttonText.resizeTextMaxSize = 100;
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
}