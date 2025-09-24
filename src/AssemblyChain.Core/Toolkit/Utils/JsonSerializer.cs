using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Utils
{
    /// <summary>
    /// Enhanced JSON serialization utilities with Rhino geometry support.
    /// </summary>
    public static class JsonSerializer
    {
        private static readonly JsonSerializerSettings _settings;

        static JsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new Point3dConverter(),
                    new Vector3dConverter(),
                    new PlaneConverter(),
                    new BoundingBoxConverter(),
                    new GuidConverter()
                }
            };
        }

        public class SerializationOptions
        {
            public bool IncludeMetadata { get; set; } = true;
            public bool CompressOutput { get; set; } = false;
            public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
            public int MaxDepth { get; set; } = 10;
        }

        public static string Serialize(object obj, SerializationOptions options = null)
        {
            options ??= new SerializationOptions();
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = _settings.Formatting,
                    NullValueHandling = _settings.NullValueHandling,
                    ReferenceLoopHandling = _settings.ReferenceLoopHandling,
                    Converters = _settings.Converters,
                    MaxDepth = options.MaxDepth
                };

                if (options.IncludeMetadata)
                {
                    var wrapper = new
                    {
                        Data = obj,
                        Metadata = new
                        {
                            Timestamp = DateTime.UtcNow.ToString(options.DateTimeFormat),
                            Version = "1.0.0",
                            Type = obj?.GetType().FullName
                        }
                    };
                    return Newtonsoft.Json.JsonConvert.SerializeObject(wrapper, settings);
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Serialization failed: {ex.Message}", ex);
            }
        }

        public static T Deserialize<T>(string json)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, _settings);
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Deserialization failed: {ex.Message}", ex);
            }
        }

        public static void SaveToFile(object obj, string filePath, SerializationOptions options = null)
        {
            var json = Serialize(obj, options);
            File.WriteAllText(filePath, json);
        }

        public static T LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
            var json = File.ReadAllText(filePath);
            return Deserialize<T>(json);
        }

        public class SerializationException : Exception
        {
            public SerializationException(string message, Exception innerException = null)
                : base(message, innerException) { }
        }

        #region Custom JSON Converters

        private class Point3dConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var pt = (Point3d)value;
                writer.WriteStartObject();
                writer.WritePropertyName("X"); serializer.Serialize(writer, pt.X);
                writer.WritePropertyName("Y"); serializer.Serialize(writer, pt.Y);
                writer.WritePropertyName("Z"); serializer.Serialize(writer, pt.Z);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                double x = 0, y = 0, z = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = reader.Value?.ToString();
                        reader.Read();
                        switch (propertyName)
                        {
                            case "X": x = Convert.ToDouble(reader.Value); break;
                            case "Y": y = Convert.ToDouble(reader.Value); break;
                            case "Z": z = Convert.ToDouble(reader.Value); break;
                        }
                    }
                    else if (reader.TokenType == JsonToken.EndObject) break;
                }
                return new Point3d(x, y, z);
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(Point3d);
        }

        private class Vector3dConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var v = (Vector3d)value;
                writer.WriteStartObject();
                writer.WritePropertyName("X"); serializer.Serialize(writer, v.X);
                writer.WritePropertyName("Y"); serializer.Serialize(writer, v.Y);
                writer.WritePropertyName("Z"); serializer.Serialize(writer, v.Z);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                double x = 0, y = 0, z = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = reader.Value?.ToString();
                        reader.Read();
                        switch (propertyName)
                        {
                            case "X": x = Convert.ToDouble(reader.Value); break;
                            case "Y": y = Convert.ToDouble(reader.Value); break;
                            case "Z": z = Convert.ToDouble(reader.Value); break;
                        }
                    }
                    else if (reader.TokenType == JsonToken.EndObject) break;
                }
                return new Vector3d(x, y, z);
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(Vector3d);
        }

        private class PlaneConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var pl = (Plane)value;
                writer.WriteStartObject();
                writer.WritePropertyName("Origin"); serializer.Serialize(writer, pl.Origin);
                writer.WritePropertyName("Normal"); serializer.Serialize(writer, pl.Normal);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                Point3d origin = Point3d.Origin; Vector3d normal = Vector3d.ZAxis;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = reader.Value?.ToString();
                        reader.Read();
                        switch (propertyName)
                        {
                            case "Origin": origin = serializer.Deserialize<Point3d>(reader); break;
                            case "Normal": normal = serializer.Deserialize<Vector3d>(reader); break;
                        }
                    }
                    else if (reader.TokenType == JsonToken.EndObject) break;
                }
                return new Plane(origin, normal);
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(Plane);
        }

        private class BoundingBoxConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var bb = (BoundingBox)value;
                writer.WriteStartObject();
                writer.WritePropertyName("Min"); serializer.Serialize(writer, bb.Min);
                writer.WritePropertyName("Max"); serializer.Serialize(writer, bb.Max);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                Point3d min = Point3d.Origin, max = Point3d.Origin;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = reader.Value?.ToString();
                        reader.Read();
                        switch (propertyName)
                        {
                            case "Min": min = serializer.Deserialize<Point3d>(reader); break;
                            case "Max": max = serializer.Deserialize<Point3d>(reader); break;
                        }
                    }
                    else if (reader.TokenType == JsonToken.EndObject) break;
                }
                return new BoundingBox(min, max);
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(BoundingBox);
        }

        private class GuidConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                writer.WriteValue(((Guid)value).ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return Guid.Parse(reader.Value?.ToString() ?? Guid.Empty.ToString());
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(Guid);
        }

        #endregion
    }
}




