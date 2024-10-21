using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Ids;
using Game.Logic.Shared;
using Game.Utils;
using System;
using System.Collections.Generic;

namespace Game.Logic
{
	/// <summary>
	/// This logic provides the necessary behaviour to manage the player's board during a gameplay session
	/// </summary>
	public interface IGameplayBoardDataProvider
	{
		IObservableDictionaryReader<UniqueId, IPieceData> Pieces { get; }
		IObservableListReader<UniqueId> InputPieces { get; }

		bool TryGetPieceFromTile(int row, int column, out IPieceData pieceCopy);
	}

	/// <inheritdoc />
	public interface IGameplayBoardLogic : IGameplayBoardDataProvider
	{
		new IObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData> Pieces { get; }
		new IObservableList<UniqueId> InputPieces { get; }

		bool TryGetPieceDataFromTile(int row, int column, out PieceData piece);

		void SetPieceOnTile(int row, int column, UniqueId id);

		void CleanUpTile(int row, int column);

		void RefillInputPieces(Func<PieceData> createPieceFunc);

		void RefillBoard(Func<PieceData> createPieceFunc, IRngLogic rngLogic);
	}

	/// <inheritdoc cref="IGameplayBoardLogic"/>
	public class GameplayBoardLogic : AbstractBaseLogic<PlayerData>, IGameplayBoardLogic, IGameLogicInitializer
	{
		private IObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData> _pieces;
		private IObservableList<UniqueId> _inputPieces;

		/// <inheritdoc />
		public IObservableDictionaryReader<UniqueId, IPieceData> Pieces => _pieces;
		/// <inheritdoc />
		IObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData> IGameplayBoardLogic.Pieces => _pieces;
		/// <inheritdoc />
		public IObservableList<UniqueId> InputPieces => _inputPieces;
		/// <inheritdoc />
		IObservableListReader<UniqueId> IGameplayBoardDataProvider.InputPieces => _inputPieces;

		public GameplayBoardLogic(IConfigsProvider configsProvider, IDataProvider dataProvider, ITimeService timeService) :
			base(configsProvider, dataProvider, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			_inputPieces = new ObservableList<UniqueId>(Data.InputPieces);
			_pieces = new ObservableResolverDictionary<UniqueId, IPieceData, UniqueId, PieceData>(Data.Pieces,
				originPair => new KeyValuePair<UniqueId, IPieceData>(originPair.Key, originPair.Value),
				(key, value) => new KeyValuePair<UniqueId, PieceData>(key, value as PieceData));
		}

		/// <inheritdoc />
		public bool TryGetPieceFromTile(int row, int column, out IPieceData piece)
		{
			var ret = TryGetPieceDataFromTile(row, column, out var pieceData);

			piece = pieceData;

			return ret;
		}

		/// <inheritdoc />
		public bool TryGetPieceDataFromTile(int row, int column, out PieceData piece)
		{
			if (row < 0 || column < 0 || row >= Constants.Gameplay.BOARD_ROWS || column >= Constants.Gameplay.BOARD_COLUMNS)
			{
				piece = null;

				return false;
			}

			if (!Data.Pieces.TryGetValue(Data.Board[row, column].Piece, out var pieceData))
			{
				piece = null;

				return false;
			}

			piece = pieceData;

			return true;
		}

		/// <inheritdoc />
		public void SetPieceOnTile(int row, int column, UniqueId piece)
		{
			if (Data.Board[row, column] != null)
			{
				Data.Board[row, column].Piece = piece;

				return;
			}

			Data.Board[row, column] = new TileData
			{
				Row = row,
				Column = column,
				Piece = piece
			};
		}

		/// <inheritdoc />
		public void CleanUpTile(int row, int column)
		{
			_pieces.Remove(Data.Board[row, column].Piece);

			Data.Board[row, column].Piece = UniqueId.Invalid;
		}

		/// <inheritdoc />
		public void RefillInputPieces(Func<PieceData> createPieceFunc)
		{
			InputPieces.Clear();

			for (var i = 0; i < Constants.Gameplay.MAX_INPUT_PIECES; i++)
			{
				InputPieces.Add(createPieceFunc().Id);
			}
		}

		/// <inheritdoc />
		public void RefillBoard(Func<PieceData> createPieceFunc, IRngLogic rngLogic)
		{
			var totalSpace = Constants.Gameplay.BOARD_ROWS * Constants.Gameplay.BOARD_COLUMNS;
			var totalPieces = rngLogic.Range(totalSpace / 4, totalSpace / 2);

			for(int i = 0, pos = 0; i < totalPieces; i++)
			{
				pos = rngLogic.Range(pos, totalSpace - totalPieces + i);

				SetPieceOnTile(
					pos / Constants.Gameplay.BOARD_COLUMNS,
					pos % Constants.Gameplay.BOARD_COLUMNS,
					createPieceFunc().Id);
			}
		}
	}
}
