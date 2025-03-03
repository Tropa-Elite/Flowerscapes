using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Commands;
using Game.Controllers;
using Game.Data;
using Game.Ids;
using Game.Logic;
using Game.Messages;
using Game.Services;
using Game.Utils;
using Game.ViewControllers;
using GameLovers.Services;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Game.MlAgents
{
	public class SortAgent : Agent
	{
		[SerializeField] private TileViewController[] _tiles;

		public float TimePassReward = -0.02f;
		public float SliceTransferReward = 0.01f;
		public float PieceCompleteReward = 0.1f;
		public float DropFailedReward = -0.1f;
		public float WinReward = 1f;
		public float LoseReward = -1f;
		
		private IGameServicesLocator _services;
		private IPiecesController _piecesController;
		private IGameDataProviderLocator _gameDataProvider;

		protected override void Awake()
		{
			base.Awake();
			
			_tiles = FindObjectsByType<TileViewController>(FindObjectsSortMode.None);
				
			Array.Sort(_tiles, (a, b) => TileData.ToTileId(a.Row, a.Column).CompareTo(TileData.ToTileId(b.Row, b.Column)));
		}

		private void OnDestroy()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void FixedUpdate()
		{
			RequestDecision();
			AddReward(TimePassReward);
		}

		/// <summary>
		/// Initializes the agent to allow to follow the <see cref="Game.StateMachines.GameplayState"/> order of execution flow
		/// </summary>
		public void Init()
		{
			_services = MainInstaller.Resolve<IGameServicesLocator>();
			_gameDataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
			_piecesController = MainInstaller.Resolve<IPiecesController>();
			
			_services.MessageBrokerService.Subscribe<OnPieceDroppedMessage>(OnPieceDroppedMessage);
			_services.MessageBrokerService.Subscribe<OnGameInitMessage>(OnGameInitMessage);
			_services.MessageBrokerService.Subscribe<OnGameCompleteMessage>(OnGameCompleteMessage);
			_services.MessageBrokerService.Subscribe<OnGameOverMessage>(OnGameOverMessage);
		}

		/* This code has no effect and is defined manually on the agent Prefab. Find a fix online
		public override void Initialize()
		{
			GetComponent<BehaviorParameters>().BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(
				Constants.Gameplay.Max_Deck_Pieces,
				Constants.Gameplay.Board_Rows * Constants.Gameplay.Board_Columns
			);
		}*/

		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			var deckPieceIndex = actionBuffers.DiscreteActions[0];

			if (deckPieceIndex < 0)
			{
				AddReward(DropFailedReward);
				return;
			}
			
			var pieceId = _gameDataProvider.DeckSpawnerDataProvider.Deck[deckPieceIndex];
			var (row, column) = TileData.IdToRowColumn(actionBuffers.DiscreteActions[1]);
			var tileIndex = row * Constants.Gameplay.Board_Columns + column; 
			
			if (tileIndex < 0 || tileIndex >= _tiles.Length)
			{
				AddReward(DropFailedReward);
				return;
			}
			
			var screenPos = _tiles[tileIndex].transform.position;

			_piecesController.TryGetPiece(pieceId, out var piece);
			
			var dropResult = _piecesController.OnPieceDrop(piece, screenPos, out var command);

			while (DOTween.TotalActiveTweens() > 0)
			{
				DOTween.CompleteAll(true);
			}

			if (dropResult)
			{
				_services.CommandService.ExecuteCommand(command);
			}
			else
			{
				AddReward(DropFailedReward);
			}
		}

		public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
		{
			var branch = 0; // Branch for deck piece selection
			var deckSize = _gameDataProvider.DeckSpawnerDataProvider.Deck.Count;
			var maskLimit = Mathf.Min(Constants.Gameplay.Max_Deck_Pieces, deckSize);
			
			for (var i = 0; i < maskLimit; i++)
			{
				var id = _gameDataProvider.DeckSpawnerDataProvider.Deck[i];
				var state = id.IsValid && _piecesController.TryGetPiece(id, out _);
				
				actionMask.SetActionEnabled(branch, i, state);
			}
		}

		public override void Heuristic(in ActionBuffers actionBuffersOut)
		{
			var discreteActions = actionBuffersOut.DiscreteActions;

			discreteActions[0] = DeckPieceHeuristic();
			discreteActions[1] = TilePositionHeuristic(discreteActions[0]);
		}

		private int DeckPieceHeuristic()
		{
			var bestScore = float.MaxValue;
			var index = -1;
			
			for (var i = 0; i < _gameDataProvider.DeckSpawnerDataProvider.Deck.Count; i++)
			{
				var pieceId = _gameDataProvider.DeckSpawnerDataProvider.Deck[i];
				
				if (!pieceId.IsValid) continue;
				
				var score = EvaluateDeckPieceScore(pieceId);
				
				if (score > bestScore)
				{
					index = i;
					bestScore = score;
				}
			}

			return index;
		}

		private int TilePositionHeuristic(int deckPieceIndexPicked)
		{
			if (deckPieceIndexPicked < 0) return -1;
			
			var pickedPieceId = _gameDataProvider.DeckSpawnerDataProvider.Deck[deckPieceIndexPicked];
			var piece = _gameDataProvider.PieceDataProvider.Pieces[pickedPieceId];
			var bestScore = 0f;
			var tileId = 0;
				
			for (int row = 0; row < Constants.Gameplay.Board_Rows; row++)
			{
				for (int col = 0; col < Constants.Gameplay.Board_Columns; col++)
				{
					if (_gameDataProvider.TileBoardDataProvider.TryGetPieceFromTile(row, col, out _)) continue;
					
					var adjacentTiles = _gameDataProvider.TileBoardDataProvider.GetAdjacentTileList(row, col);
					var score = EvaluateTileScore(adjacentTiles, piece);
						
					if (score > bestScore)
					{
						bestScore = score;
						tileId = TileData.ToTileId(row, col);
					}
				}
			}

			return tileId;
		}

		private float EvaluateDeckPieceScore(UniqueId pieceId)
		{
			var slices = _gameDataProvider.PieceDataProvider.Pieces[pieceId].Slices;
			var score = 1;

			for (var i = 1; i < slices.Count; i++)
			{
				if (slices[i] != slices[i - 1])
				{
					score /= 2;
				}
				
				score++;
			}

			return score;
		}
		
		private float EvaluateAdjacentPieceScore(IPieceData adjacentPiece, Dictionary<SliceColor, int> selectedPieceSlices)
		{
			var sliceColors = adjacentPiece.GetSlicesColors();
			var score = 0f;
			var canComplete = false;

			foreach (var colorCount in sliceColors)
			{
				score += selectedPieceSlices.TryGetValue(colorCount.Key, out var count) ? count : -0.1f;
				canComplete = canComplete || count + colorCount.Value >= Constants.Gameplay.Max_Piece_Slices;
			}
			
			// Bonus if adjacent completes selected piece
			if (selectedPieceSlices.Count == 1 && canComplete)
			{
				score *= 100f;
			}
			
			// Bonus if adjacent is close to clean up
			if(sliceColors.Count == 1)
			{
				score *= 1.5f;
			}

			return score;
		}

		private float EvaluateTileScore(List<ITileData> adjacentTiles, IPieceData selectedPiece)
		{
			var score = 0f;
			var selectedColors = selectedPiece.GetSlicesColors();
			
			foreach (var tile in adjacentTiles)
			{
				if (!_gameDataProvider.TileBoardDataProvider.TryGetPieceFromTile(tile.Row, tile.Column, out var adjacentPiece)) continue;

				score += EvaluateAdjacentPieceScore(adjacentPiece, selectedColors);
			}
			
			return score;
		}

		private void OnPieceDroppedMessage(OnPieceDroppedMessage message)
		{
			foreach (var transferData in message.TransferHistory)
			{
				AddReward(transferData.SlicesAmount * SliceTransferReward);

				if (transferData.PieceCompleted)
				{
					AddReward(PieceCompleteReward);
				}
			}
		}

		private void OnGameInitMessage(OnGameInitMessage message)
		{
			gameObject.SetActive(true);
			
			while (DOTween.TotalActiveTweens() > 0)
			{
				DOTween.CompleteAll(true);
			}
		}

		private void OnGameOverMessage(OnGameOverMessage message)
		{
			AddReward(LoseReward);
			EndEpisode();
			_services.CommandService.ExecuteCommand(new RestartGameCommand());
		}

		private void OnGameCompleteMessage(OnGameCompleteMessage message)
		{
			AddReward(WinReward);
			EndEpisode();
			_services.CommandService.ExecuteCommand(new RestartGameCommand());
		}
	}
}