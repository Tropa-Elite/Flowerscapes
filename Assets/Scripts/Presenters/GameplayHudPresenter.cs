using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Presenters
{
	// TODO: Add to Canvas GameObject
	public class GameplayHudPresenter : MonoBehaviour
	{
		[SerializeField] private Button _gameOverButton;

		private void Awake()
		{
			_gameOverButton.onClick.AddListener(OnGameOverClicked);
		}

		private void OnGameOverClicked()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
