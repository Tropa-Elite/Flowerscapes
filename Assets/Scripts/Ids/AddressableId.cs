/* AUTO GENERATED CODE */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameLovers.AssetsImporter;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace Game.Ids
{
	public enum AddressableId
	{
		Prefabs_Piece,
		Prefabs_Configs_UiConfigs,
		Prefabs_UI_GameOver
	}

	public enum AddressableLabel
	{
		Label_GenerateIds
	}

	public static class AddressablePathLookup
	{
		public static readonly string PrefabsUI = "Prefabs/UI";
		public static readonly string PrefabsConfigs = "Prefabs/Configs";
		public static readonly string Prefabs = "Prefabs";
	}

	public static class AddressableConfigLookup
	{
		public static IList<AddressableConfig> Configs => _addressableConfigs;
		public static IList<string> Labels => _addressableLabels;

		public static AddressableConfig GetConfig(this AddressableId addressable)
		{
			return _addressableConfigs[(int) addressable];
		}

		public static IList<AddressableConfig> GetConfigs(this AddressableLabel label)
		{
			return _addressableLabelMap[_addressableLabels[(int) label]];
		}

		public static IList<AddressableConfig> GetConfigs(string label)
		{
			return _addressableLabelMap[label];
		}

		public static string ToLabelString(this AddressableLabel label)
		{
			return _addressableLabels[(int) label];
		}

		private static readonly IList<string> _addressableLabels = new List<string>
		{
			"GenerateIds"
		}.AsReadOnly();

		private static readonly IReadOnlyDictionary<string, IList<AddressableConfig>> _addressableLabelMap = new ReadOnlyDictionary<string, IList<AddressableConfig>>(new Dictionary<string, IList<AddressableConfig>>
		{
			{"GenerateIds", new List<AddressableConfig>
				{
					new AddressableConfig(0, "Prefabs/UI/GameOver.prefab", "Assets/Prefabs/UI/GameOver.prefab", typeof(UnityEngine.GameObject), new [] {"GenerateIds"}),
					new AddressableConfig(1, "Prefabs/Configs/UiConfigs.asset", "Assets/Prefabs/Configs/UiConfigs.asset", typeof(GameLovers.UiService.UiConfigs), new [] {"GenerateIds"}),
					new AddressableConfig(2, "Prefabs/Piece.prefab", "Assets/Prefabs/Piece.prefab", typeof(UnityEngine.GameObject), new [] {"GenerateIds"}),
				}.AsReadOnly()}
		});

		private static readonly IList<AddressableConfig> _addressableConfigs = new List<AddressableConfig>
		{
			new AddressableConfig(0, "Prefabs/Piece.prefab", "Assets/Prefabs/Piece.prefab", typeof(UnityEngine.GameObject), new [] {"GenerateIds"}),
			new AddressableConfig(1, "Prefabs/Configs/UiConfigs.asset", "Assets/Prefabs/Configs/UiConfigs.asset", typeof(GameLovers.UiService.UiConfigs), new [] {"GenerateIds"}),
			new AddressableConfig(2, "Prefabs/UI/GameOver.prefab", "Assets/Prefabs/UI/GameOver.prefab", typeof(UnityEngine.GameObject), new [] {"GenerateIds"})
		}.AsReadOnly();
	}
}
