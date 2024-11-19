using Game.Ids;
using GameLovers.Services;

namespace Game.Messages
{
	public struct OnGameInitMessage : IMessage { }
	public struct OnGameOverMessage : IMessage { }
	public struct OnGameRestartClickedMessage : IMessage { }
	public struct OnReturnMenuClickedMessage : IMessage { }
	public struct OnPlayClickedMessage : IMessage { }
	public struct OnPieceDroppedMessage : IMessage
	{
		public UniqueId PieceId;
		public int Row;
		public int Column;
	}
}
