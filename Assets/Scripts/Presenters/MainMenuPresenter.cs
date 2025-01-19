﻿using System;
using Game.Messages;
using Game.Services;
using GameLovers;
using GameLovers.Services;
using GameLovers.StatechartMachine;
using GameLovers.UiService;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Presenters
{
	/// <summary>
	/// This Presenter handles the Main Menu UI by:
	/// - Showing the Main Menu button to start the game
	/// - Showing game instructions and objectives about the game for the player to plat
	/// </summary>
	public class MainMenuPresenter : UiPresenterData<MainMenuPresenter.PresenterData>
	{
		public struct PresenterData
		{
			public UnityAction OnPlayClicked;
		}
		
		[SerializeField] private TextMeshProUGUI _version;
		[SerializeField] private Button _playButton;

		private void Awake()
		{
			_playButton.onClick.AddListener(() => Data.OnPlayClicked.Invoke());
		}

		private void Start()
		{
			_version.text =
				$"internal = v{VersionServices.VersionInternal}\n" +
				$"external = v{VersionServices.VersionExternal}\n" +
				$"build number = {VersionServices.BuildNumber}";
		}
	}
}
