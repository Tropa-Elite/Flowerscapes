using Game.Data;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using System;
using System.Collections.Generic;

namespace Game.Logic.Client
{
	public interface IPiecesDataProvider
	{
		IObservableDictionaryReader<UniqueId, IPieceData> Pieces { get; }
	}

	/// <inheritdoc />
	public interface IPiecesLogic : IPiecesDataProvider
	{
		new IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> Pieces { get; }

		int TransferSlices(UniqueId sourceId, UniqueId targetId, SliceColor color, int maxSlices = -1);
	}

	public class PiecesLogic : AbstractBaseLogic<PlayerData>, IPiecesLogic, IGameLogicInitializer
	{
		private IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> _pieces;

		/// <inheritdoc />
		public IObservableDictionaryReader<UniqueId, IPieceData> Pieces => _pieces;
		/// <inheritdoc />
		IObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData> IPiecesLogic.Pieces => _pieces;

		public PiecesLogic(
			IGameDataProviderLocator gameDataProvider,
			IConfigsProvider configsProvider,
			IDataProvider dataProvider,
			ITimeService timeService) :
			base(gameDataProvider, configsProvider, dataProvider, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			_pieces = new ObservableResolverDictionary<UniqueId, IPieceData, ulong, PieceData>(Data.Pieces,
				originPair => new KeyValuePair<UniqueId, IPieceData>(originPair.Key, originPair.Value),
				(key, value) => new KeyValuePair<ulong, PieceData>(key, value as PieceData));
		}

		public int TransferSlices(UniqueId sourceId, UniqueId targetId, SliceColor color, int maxSlices = -1)
		{
			var sourcePiece = _pieces[sourceId];
			var targetPiece = _pieces[targetId];
			var slicesColors = sourcePiece.GetSlicesColors();

			if (!slicesColors.TryGetValue(color, out var slicesCount))
			{
				return 0;
			}

			slicesCount = maxSlices == -1 ? slicesCount : Math.Min(maxSlices, slicesCount);

			var targetfreeSpace = Constants.Gameplay.MAX_PIECE_SLICES - targetPiece.Slices.Count;
			var transferAmount = Math.Min(targetfreeSpace, slicesCount);
			var collectedSlices = CollectSlicesFromPiece(sourceId, color, transferAmount);

			FillPiece(targetId, color, collectedSlices);

			return collectedSlices;
		}

		private void FillPiece(UniqueId id, SliceColor color, int amount)
		{
			var piece = _pieces.GetOriginValue(id);
			var index = piece.Slices.IndexOf(color);

			index = index == -1 ? piece.Slices.Count - 1 : index;

			for (int i = 0; i < amount; i++)
			{
				piece.Slices.Insert(index, color);
			}

			if(piece.Slices.Count > Constants.Gameplay.MAX_PIECE_SLICES)
			{
				throw new LogicException($"Piece {id} was filled too much, a total of {piece.Slices.Count} slices");
			}
		}

		private int CollectSlicesFromPiece(UniqueId pieceId, SliceColor color, int slices)
		{
			var count = 0;
			var piece = _pieces.GetOriginValue(pieceId);

			for (int i = piece.Slices.Count - 1; i > -1 && count < slices; i--)
			{
				if (piece.Slices[i] != color) continue;

				count++;

				piece.Slices.RemoveAt(i);
			}

			return count;
		}
	}
}
