using TriInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        [SerializeField, Scene] private string scene;

        private void Awake()
        {
            if (TryGetComponent(out Button button))
            {
                button.onClick.AddListener(LoadLevel);
            }
        }

        private void LoadLevel()
        {
            if (string.IsNullOrEmpty(scene))
            {
                return;
            }

            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }
    }
}
