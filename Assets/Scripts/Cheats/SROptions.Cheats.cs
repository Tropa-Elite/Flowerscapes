using System.Collections.Generic;
using System.ComponentModel;
using Game.Commands;
using Game.Data;
using Game.Logic;
using Game.Messages;
using Game.Services;
using GameLovers.Services;
using UnityEngine;

public partial class SROptions
{
#if !UNITY_EDITOR
	[Category("Data")]
	public void ResetAllData()
	{
		PlayerPrefs.DeleteAll();
	}
#endif
	
	[Category("Cheats")]
	public void CheatInitBoard()
	{
		var services = MainInstaller.Resolve<IGameServicesLocator>();
		
		services.MessageBrokerService.PublishSafe(new OnGameOverMessage());
		services.CommandService.ExecuteCommand(new RestartGameCommand { SetupTestData = SetupGameBoardTestData });
	}

	private void SetupGameBoardTestData(IGameLogicLocator gameLogic)
	{
		gameLogic.PiecesLogic.Pieces.GetOriginValue(gameLogic.GameplayBoardLogic.PieceDeck[0]).Slices = new List<SliceColor> 
		{ 
			SliceColor.Blue, SliceColor.Blue, SliceColor.White, SliceColor.White
		};

		if (gameLogic.GameplayBoardLogic.TryGetPieceFromTile(0, 0, out var piece1))
		{
			gameLogic.PiecesLogic.Pieces.Remove(piece1.Id);
			gameLogic.GameplayBoardLogic.CleanUpTile(0, 0);
		}
		if (gameLogic.GameplayBoardLogic.TryGetPieceFromTile(0, 1, out var piece2))
		{
			gameLogic.PiecesLogic.Pieces.Remove(piece2.Id);
			gameLogic.GameplayBoardLogic.CleanUpTile(0, 1);
		}
		if (gameLogic.GameplayBoardLogic.TryGetPieceFromTile(0, 2, out var piece3))
		{
			gameLogic.PiecesLogic.Pieces.Remove(piece3.Id);
			gameLogic.GameplayBoardLogic.CleanUpTile(0, 2);
		}
		var piece = gameLogic.EntityFactoryLogic.CreatePiece();
		gameLogic.GameplayBoardLogic.SetPieceOnTile(piece.Id, 0, 0);
		piece.Slices = new List<SliceColor> { SliceColor.Blue, SliceColor.Blue };
		piece = gameLogic.EntityFactoryLogic.CreatePiece();
		gameLogic.GameplayBoardLogic.SetPieceOnTile(piece.Id, 0, 2);
		piece.Slices = new List<SliceColor> { SliceColor.Blue, SliceColor.Blue };
	}
}