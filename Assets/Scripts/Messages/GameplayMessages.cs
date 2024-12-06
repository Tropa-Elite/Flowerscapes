using System.Collections.Generic;
using Game.Data;
using Game.Ids;
using GameLovers.Services;

namespace Game.Messages
{
	public struct OnGameInitMessage : IMessage { }
	public struct OnGameOverMessage : IMessage { }
	public struct OnGameRestartMessage : IMessage { }
	public struct OnReturnMenuClickedMessage : IMessage { }
	public struct OnPlayClickedMessage : IMessage { }
	public struct OnPieceDroppedMessage : IMessage
	{
		public UniqueId PieceId;
		public ITileData Tile;
		public List<PieceTransferData> TransferHistory;
	}
}
