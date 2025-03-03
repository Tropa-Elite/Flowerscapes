using System.Collections.Generic;
using Game.Data;
using Game.Logic;
using Game.Utils;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Game.MlAgents
{
	 public class SortSensor : ISensor
	 {
		private readonly IGameDataProviderLocator _gameDataProvider;
		private readonly ObservationSpec _observationSpec;
		private readonly string _name;

		public int ObservationSize => Constants.Gameplay.Max_Deck_Pieces + Constants.Gameplay.Board_Columns *
			Constants.Gameplay.Board_Rows * (int)SliceColor.ColorCount;
		
		public SortSensor(string name, IGameDataProviderLocator dataProvider)
		{
			_name = name;
			_gameDataProvider = dataProvider;
			_observationSpec = ObservationSpec.Vector(ObservationSize);
		}
		
		public ObservationSpec GetObservationSpec()
		{
			return _observationSpec;
		}

		public int Write(ObservationWriter writer)
		{
			var parameters = new List<float>((int) SliceColor.ColorCount);
			
			for (var i = 0; i < Constants.Gameplay.Board_Rows; i++)
			{
				for(var j = 0; j < Constants.Gameplay.Board_Columns; j++)
				{
					_gameDataProvider.TileBoardDataProvider.TryGetPieceFromTile(i, j, out var piece);
					ProcessPieceColors(piece, parameters);
					//writer.AddList(parameters, TileData.ToTileId(i, j));
					WriteParameters(i, j, writer, parameters);
				}
			}

			for (var i = 0; i < _gameDataProvider.DeckSpawnerDataProvider.Deck.Count; i++)
			{
				var id = _gameDataProvider.DeckSpawnerDataProvider.Deck[i];
				//var parameters = new int[(int) SliceColor.ColorCount];
				var row = Constants.Gameplay.Board_Rows - 1;
				var column = Constants.Gameplay.Board_Columns - 1 + i;

				ProcessPieceColors(_gameDataProvider.PieceDataProvider.Pieces[id], parameters);
				//writer.AddList(parameters, TileData.ToTileId(i, column));
				WriteParameters(row, column, writer, parameters);
			}
			
			return ObservationSize;
		}

		public byte[] GetCompressedObservation()
		{
			return null;
		}

		public void Update()
		{
		}

		public void Reset()
		{
		}

		public CompressionSpec GetCompressionSpec()
		{
			return CompressionSpec.Default();
		}

		public string GetName()
		{
			return _name;
		}

		private void ProcessPieceColors(IPieceData piece, IList<float> parameters)
		{
			for (var i = 0; i < (int) SliceColor.ColorCount; i++)
			{
				if (i >= parameters.Count)
				{
					parameters.Add(0);
				}
				else
				{
					parameters[i] = 0;
				}
			}
			
			if (piece == null)
			{
				return;
			}
			
			foreach (var t in piece.Slices)
			{
				parameters[(int)t]++;
			}
		}

		private void WriteParameters(int row, int column, ObservationWriter writer, IList<float> parameters)
		{
			for (var i = 0; i < parameters.Count; i++)
			{
				writer[i, TileData.ToTileId(row, column)] = parameters[i];
			}
		}
	 }
}