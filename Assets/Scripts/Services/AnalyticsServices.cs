using AptabaseSDK;
using Game.Messages;
using Game.Services.Analytics;
using GameLovers.Services;
using mixpanel;
using UnityEngine.Device;

namespace Game.Services
{
	/// <summary>
	/// Static class that defines all the event types names
	/// </summary>
	public static class AnalyticsEvents
	{
		public static readonly string SessionStart = "game_session_start";
		public static readonly string SessionEnd = "game_session_end";
		public static readonly string SessionHeartbeat = "session_heartbeat";
		public static readonly string SessionAdsData = "session_ads_data";
		public static readonly string LoadingStarted = "loading_started";
		public static readonly string LoadingCompleted = "loading_completed";
		public static readonly string PlayerLogin = "player_login";
		public static readonly string PlayerAge = "player_age";
		public static readonly string MainMenuEnter = "main_menu_enter";
		public static readonly string Error = "error_log";
		public static readonly string Purchase = "purchase";
		public static readonly string LevelStart = "level_start";
		public static readonly string LevelComplete = "level_complete";
	}

	/// <summary>
	/// The analytics service is an endpoint in the game to log custom events to Game's analytics console
	/// </summary>
	public interface IAnalyticsService
	{
		/// <inheritdoc cref="AnalyticsSession"/>
		AnalyticsSession SessionCalls { get; }
		/// <inheritdoc cref="AnalyticsEconomy"/>
		AnalyticsEconomy EconomyCalls { get; }
		/// <inheritdoc cref="AnalyticsErrors"/>
		AnalyticsErrors ErrorsCalls { get; }
		/// <inheritdoc cref="AnalyticsMainMenu"/>
		AnalyticsMainMenu MainMenuCalls { get; }
		/// <inheritdoc cref="AnalyticsGameplay"/>
		AnalyticsGameplay GameplayCalls { get; }

		/// <summary>
		/// Flushes all the queued analytics events
		/// </summary>
		void FlushEvents();
	}

	/// <inheritdoc cref="IAnalyticsService" />
	public class AnalyticsService : IAnalyticsService, IGameServicesInitializer
	{
		/// <inheritdoc />
		public AnalyticsSession SessionCalls { get; }
		/// <inheritdoc />
		public AnalyticsEconomy EconomyCalls { get; }
		/// <inheritdoc />
		public AnalyticsErrors ErrorsCalls { get; }
		/// <inheritdoc />
		public AnalyticsMainMenu MainMenuCalls { get; }
		/// <inheritdoc />
		public AnalyticsGameplay GameplayCalls { get; }

		public AnalyticsService(IMessageBrokerService messageBrokerService, IDataProvider dataProvider)
		{
			SessionCalls = new AnalyticsSession(this, dataProvider);
			GameplayCalls = new AnalyticsGameplay(this);
			EconomyCalls = new AnalyticsEconomy(this);
			ErrorsCalls = new AnalyticsErrors(this);
			MainMenuCalls = new AnalyticsMainMenu(this);
			
			messageBrokerService.Subscribe<ApplicationComplianceAcceptedMessage>(OnApplicationComplianceAcceptedMessage);
		}

		/// <inheritdoc />
		public void Init()
		{
			Unity.Services.Analytics.AnalyticsService.Instance.StartDataCollection();
			SessionCalls.SessionStart();
			//SessionCalls.PlayerLogin(SystemInfo.deviceUniqueIdentifier);
		}
		
		/// <inheritdoc />
		public void FlushEvents()
		{
			Unity.Services.Analytics.AnalyticsService.Instance.Flush();
			Aptabase.Flush();
			Mixpanel.Flush();
		}

		private void OnApplicationComplianceAcceptedMessage(ApplicationComplianceAcceptedMessage message)
		{
			SessionCalls.PlayerAge(message.Age);
		}
	}
}
