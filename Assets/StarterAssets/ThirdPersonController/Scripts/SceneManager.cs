using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance {get; private set;}
    
    [SerializeField] private GameObject _loadingScreen;
    
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of SceneManager found!");
        }
        instance = this;
    }


    public IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while(!operation.isDone)
        {
            _loadingScreen.SetActive(true);
            //GameObjectLoading + animatino
            // loadingScreen.SetTrigger("LoadingOn");
            // progress
            yield return null;
        }
        //play animation for transition
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

        // if(loadingScreenAnimator != null)
            // loadingScreenAnimator.SetTrigger("LoadingOff");
    }
}