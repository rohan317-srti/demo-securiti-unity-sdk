using System;
using System.Collections.Generic;
using UnityEngine;

namespace Securiti.Consent
{
    /// <summary>
    /// Helper utilities for JSON serialization/deserialization
    /// Unity's JsonUtility doesn't handle top-level arrays, so we need a wrapper
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Wrapper class for JSON array deserialization
        /// </summary>
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        /// <summary>
        /// Deserialize a JSON array to a strongly-typed array
        /// </summary>
        /// <typeparam name="T">The type of objects in the array</typeparam>
        /// <param name="json">JSON array string</param>
        /// <returns>Array of deserialized objects</returns>
        public static T[] FromJsonArray<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new T[0];
            }

            // Unity's JsonUtility requires a wrapper for top-level arrays
            // Wrap the array in an object: {"items": [...]}
            string wrappedJson = $"{{\"items\":{json}}}";

            try
            {
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
                return wrapper.items ?? new T[0];
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecuritiConsent] Failed to deserialize JSON array: {e.Message}");
                Debug.LogError($"[SecuritiConsent] JSON: {json}");
                return new T[0];
            }
        }

        /// <summary>
        /// Serialize an array to JSON
        /// </summary>
        /// <typeparam name="T">The type of objects in the array</typeparam>
        /// <param name="array">Array to serialize</param>
        /// <returns>JSON array string</returns>
        public static string ToJsonArray<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return "[]";
            }

            try
            {
                Wrapper<T> wrapper = new Wrapper<T> { items = array };
                string wrappedJson = JsonUtility.ToJson(wrapper);

                // Extract just the array part: {"items":[...]} -> [...]
                int startIndex = wrappedJson.IndexOf('[');
                int endIndex = wrappedJson.LastIndexOf(']');

                if (startIndex != -1 && endIndex != -1)
                {
                    return wrappedJson.Substring(startIndex, endIndex - startIndex + 1);
                }

                return "[]";
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecuritiConsent] Failed to serialize array to JSON: {e.Message}");
                return "[]";
            }
        }

        /// <summary>
        /// Parse a simple JSON object to dictionary
        /// Useful for parsing callback messages
        /// </summary>
        public static Dictionary<string, string> ParseJsonObject(string json)
        {
            var dict = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(json))
            {
                return dict;
            }

            try
            {
                // Simple JSON parser for callback messages
                // Handles: {"key":"value", "key2":"value2"}
                json = json.Trim('{', '}', ' ');
                string[] pairs = json.Split(',');

                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim('"', ' ');
                        string value = keyValue[1].Trim('"', ' ');
                        dict[key] = value;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecuritiConsent] Failed to parse JSON object: {e.Message}");
            }

            return dict;
        }
    }
}
