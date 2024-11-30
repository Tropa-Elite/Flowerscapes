using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Game.Ids
{
	/// <summary>
	/// Used to reference any entity by an unique Id value
	/// </summary>
	[Serializable]
	[JsonConverter(typeof(UniqueIdConverter))]
	public struct UniqueId : IEquatable<UniqueId>, IComparable<UniqueId>, IComparable<ulong>
	{
		public static readonly UniqueId Invalid = new UniqueId(0);

		public readonly ulong Id;

		public bool IsValid => this != Invalid;

		public UniqueId(ulong id)
		{
			Id = id;
		}
 
		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		/// <inheritdoc />
		public int CompareTo(UniqueId value)
		{
			if (Id < value.Id)
			{
				return -1;
			}
			
			return Id > value.Id ? 1 : 0;
		}

		/// <inheritdoc />
		public int CompareTo(ulong value)
		{
			if (Id < value)
			{
				return -1;
			}
			
			return Id > value ? 1 : 0;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
 
			return obj is UniqueId && Equals((UniqueId)obj);
		}
 
		public bool Equals(UniqueId other)
		{
			return Id == other.Id;
		}
 
		public static bool operator ==(UniqueId p1, UniqueId p2)
		{
			return p1.Id == p2.Id;
		}
 
		public static bool operator !=(UniqueId p1, UniqueId p2)
		{
			return p1.Id != p2.Id;
		}
		
		public static implicit operator ulong(UniqueId id)
		{
			return id.Id;
		}
		
		public static implicit operator UniqueId(ulong id)
		{
			return new UniqueId(id);
		}
 
		/// <inheritdoc />
		public override string ToString()
		{
			return $"UniqueId: {Id.ToString()}";
		}
	}

	/// <summary>
	/// Avoids boxing for Dictionary
	/// </summary>
	public class UniqueIdKeyComparer : IEqualityComparer<UniqueId>
	{
		/// <inheritdoc />
		public bool Equals(UniqueId x, UniqueId y)
		{
			return x.Id == y.Id;
		}

		/// <inheritdoc />
		public int GetHashCode(UniqueId obj)
		{
			return obj.Id.GetHashCode();
		}
	}

	/// <summary>
	/// Configuration of the Serialization and Desialization of <see cref="UniqueId"/>
	/// </summary>
	public class UniqueIdConverter : JsonConverter<UniqueId>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, UniqueId value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Id);
		}

		/// <inheritdoc />
		public override UniqueId ReadJson(JsonReader reader, Type objectType, UniqueId existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Integer)
			{
				return new UniqueId(JToken.Load(reader).ToObject<ulong>());
			}
			throw new JsonSerializationException($"Unexpected token type. Unable to convert {existingValue} to UniqueId");
		}
	}
}