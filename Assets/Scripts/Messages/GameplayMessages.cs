using Game.Ids;
using GameLovers.Services;

namespace Game.Messages
{
	public struct OnGameInitMessage : IMessage { }
	public struct OnPieceDroppedMessage : IMessage
	{
		public UniqueId PieceId;
		public int Row;
		public int Column;
	}
}
