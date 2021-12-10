using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void GoToScene(int sceneNum) 
    {
        SceneManager.LoadSceneAsync(sceneNum);
    }
}
