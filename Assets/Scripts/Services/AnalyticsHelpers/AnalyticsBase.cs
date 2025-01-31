using System.Collections.Generic;
using System;
using AptabaseSDK;
using mixpanel;
using UnityEngine;

namespace Game.Services.Analytics
{
	/// <summary>
	/// Analytics base class for all analytics endpoint calls
	/// </summary>
	public abstract class AnalyticsBase
	{
		protected IAnalyticsService AnalyticsService;
		
		protected AnalyticsBase(IAnalyticsService analyticsService)
		{
			AnalyticsService = analyticsService;
		}

		/// <summary>
		/// Logs an analytics event with the given <paramref name="eventName"/>.
		/// </summary>
		protected void LogEvent(string eventName, Dictionary<string, object> parameters = null)
		{
			try
			{
				/*
				//PlayFab Analytics
				if (PlayFabSettings.staticPlayer.IsClientLoggedIn())
				{
					var request = new WriteClientPlayerEventRequest { EventName = eventName, Body = parameters };
					PlayFabClientAPI.WritePlayerEvent(request, null, null);
				}
				*/
				//ByteBrew.NewCustomEvent(eventName, parameters);
				
				Aptabase.TrackEvent(eventName, parameters);
				MixpanelTrack(eventName, parameters);
				UnityAnalyticsTrack(eventName, parameters);
			}
			catch (Exception e)
			{
				Debug.LogError("Error while sending analytics: " + e.Message);
				Debug.LogException(e);
			}
		}

		private void UnityAnalyticsTrack(string eventName, Dictionary<string, object> parameters)
		{
			if (parameters == null || parameters.Count == 0)
			{
				UnityEngine.Analytics.Analytics.CustomEvent(eventName);
				return;
			}
			
			if (parameters.Count > 10)
			{
				Debug.LogError($"The event {eventName} has {parameters.Count} and the max parameters for unity is 10");
			}
			
			UnityEngine.Analytics.Analytics.CustomEvent(eventName, parameters);
		}

		private void MixpanelTrack(string eventName, Dictionary<string, object> parameters)
		{
			if (parameters == null || parameters.Count == 0)
			{
				Mixpanel.Track(eventName);
				return;
			}

			foreach (var pair in parameters)
			{
				if (pair.Value is int)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((int) pair.Value));
				}
				else if (pair.Value is float or double)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((double) pair.Value));
				}
				else if (pair.Value is bool)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((bool) pair.Value));
				}
				else if (pair.Value is string)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((string) pair.Value));
				}
				else if (pair.Value is DateTime)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((DateTime) pair.Value));
				}
				else if (pair.Value is Vector2)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((Vector2) pair.Value));
				}
				else if (pair.Value is Vector3)
				{
					Mixpanel.Track(eventName, pair.Key, new Value((Vector3) pair.Value));
				}
			}
		}
	}
}