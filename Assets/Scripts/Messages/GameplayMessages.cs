using System.Collections.Generic;
using Game.Data;
using Game.Ids;
using GameLovers.Services;

namespace Game.Messages
{
	public struct OnGameInitMessage : IMessage { }
	public struct OnGameOverMessage : IMessage { }
	public struct OnGameCompleteMessage : IMessage { }
	public struct OnGameRestartMessage : IMessage { }
	public struct OnPieceDroppedMessage : IMessage
	{
		public UniqueId PieceId;
		public int TileId;
		public List<PieceTransferData> TransferHistory;
	}
}
