using UnityEngine; using UnityEngine.SceneManagement;
public class StartNewGame : MonoBehaviour {
  [SerializeField] string firstLevel = "Level0_Meadow";
  public void LoadFirstLevel(){ Time.timeScale = 1f; SceneManager.LoadScene(firstLevel); }
}
