using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using GameLovers.Services;
using UnityEngine;

namespace Game.MonoComponent
{
	public class TileMonoComponent : MonoBehaviour
	{
		[SerializeField] private int _row = -1;
		[SerializeField] private int _column = -1;

		private IGameServices _services;
		private IGameDataProvider _dataProvider;

		public int Row => _row;
		public int Column => _column;

		// Need to be done in start to avoid a race condition with the Initilization done at Main.Awake()
		private void Start()
		{
			_services = MainInstaller.Resolve<IGameServices>();
			_dataProvider = MainInstaller.Resolve<IGameDataProvider>();

			_services.MessageBrokerService.Subscribe<OnGameInitMessage>(OnGameInit);
		}

		private void OnValidate()
		{
			if(_row >= 0)
			{
				return;
			}

			var name = gameObject.name.Split('_');

			_row = int.Parse(name[1]);
			_column = int.Parse(name[2]);
		}

		private void OnGameInit(OnGameInitMessage message)
		{
			if(!_dataProvider.GameplayBoardDataProvider.TryGetPieceFromTile(_row, _column, out var pieceData))
			{
				return;
			}

			var piece = _services.PoolService.Spawn<PieceMonoComponent, UniqueId>(pieceData.Id);

			piece.RectTransform.SetParent(transform);
			piece.RectTransform.SetAsLastSibling();

			piece.RectTransform.anchoredPosition = Vector3.zero;
		}
	}
}
