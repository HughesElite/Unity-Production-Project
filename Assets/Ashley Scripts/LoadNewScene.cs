using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNewScene : MonoBehaviour
{
    public Button loadSceneButton;
    public string sceneToLoad;

    void Start()
    {
        loadSceneButton.onClick.AddListener(LoadScene);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}