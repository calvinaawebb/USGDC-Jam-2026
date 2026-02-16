using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class ExitButton : MonoBehaviour
    {
        private void Awake()
        {
            if (TryGetComponent(out Button button))
            {
                button.onClick.AddListener(ExitGame);
            }
        }

        private void ExitGame()
        {
            Application.Quit();
        }
    }
}
