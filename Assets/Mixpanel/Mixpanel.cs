using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LitJson;
using UnityEngine;

public static class Mixpanel
{
	// Set this to your Mixpanel token.
	public static string Token;

	// Set this to the distinct ID of the current user.
	public static string DistinctID;

	// Set to true to enable debug logging.
	public static bool EnableLogging;

	// Add any custom "super properties" to this dictionary. These are properties sent with every event.
	public static Dictionary<string, object> SuperProperties = new Dictionary<string, object>();

	private const string API_URL_FORMAT = "http://api.mixpanel.com";

	private static MonoBehaviour _coroutineObject;
	
	public static void SendUser(IDictionary<string, object> userProperties)
	{
		if (!ValidateToken ()) return;
		SetupDistinctID ();

		Dictionary<string, object> jsonDict = new Dictionary<string, object>();
		jsonDict.Add("$set", ProcessProperties (userProperties));
		jsonDict.Add("$distinct_id", DistinctID);
		jsonDict.Add("$token", Token);

		SendDataToURL (data: jsonDict, 
		               url: API_URL_FORMAT+"/engage/?data={0}");
	}

	// Call this to send an event to Mixpanel.
	// eventName: The name of the event. (Can be anything you'd like.)
	public static void SendEvent(string eventName)
	{
		SendEvent(eventName, null);
	}

	// Call this to send an event to Mixpanel.
	// eventName: The name of the event. (Can be anything you'd like.)
	// properties: A dictionary containing any properties in addition to those in the Mixpanel.SuperProperties dictionary.
	public static void SendEvent(string eventName, IDictionary<string, object> properties)
	{
		if (!ValidateToken ()) return;

		SetupDistinctID ();

		Dictionary<string, object> propsDict = ProcessProperties (properties);
		propsDict.Add("distinct_id", DistinctID);
		propsDict.Add("token", Token);

		foreach(var kvp in SuperProperties)
		{
			if(kvp.Value is float) // LitJSON doesn't support floats.
			{
				float f = (float)kvp.Value;
				double d = f;
				propsDict.Add(kvp.Key, d);
			}
			else
			{
				propsDict.Add(kvp.Key, kvp.Value);
			}
		}

		Dictionary<string, object> jsonDict = new Dictionary<string, object>();
		jsonDict.Add("event", eventName);
		jsonDict.Add("properties", propsDict);
		SendDataToURL (data: jsonDict,
		               url: API_URL_FORMAT+"/track/?data={0}");
	}

	private static Dictionary<string, object> ProcessProperties(IDictionary<string, object> properties){
		Dictionary<string, object> propsDict = new Dictionary<string, object>();
		
		if(properties != null)
		{
			foreach(var kvp in properties)
			{
				if(kvp.Value is float) // LitJSON doesn't support floats.
				{
					float f = (float)kvp.Value;
					double d = f;
					propsDict.Add(kvp.Key, d);
				}
				else
				{
					propsDict.Add(kvp.Key, kvp.Value);
				}
			}
		}
		return propsDict;
	}
	
	private static bool ValidateToken(){
		if (string.IsNullOrEmpty (Token)) {
			Debug.LogError ("Attempted to send an event without setting the Mixpanel.Token variable.");
			return false;
		}
		return true;
	}
	
	private static void SetupDistinctID() {
		if (string.IsNullOrEmpty (DistinctID)) {
			if (!PlayerPrefs.HasKey ("mixpanel_distinct_id"))
				PlayerPrefs.SetString ("mixpanel_distinct_id", Guid.NewGuid ().ToString ());
			DistinctID = PlayerPrefs.GetString ("mixpanel_distinct_id");
		}
		
	}
	
	private static void SendDataToURL(Dictionary<string, object> data, string url){
		string jsonStr = JsonMapper.ToJson(data);
		if (EnableLogging) {
			Debug.Log ("Sending mixpanel person: " + jsonStr);
		}
		string jsonStr64 = EncodeTo64(jsonStr);
		string urlWithData = string.Format(url, jsonStr64);
		StartCoroutine(SendEventCoroutine(urlWithData));
	}


	private static string EncodeTo64(string toEncode)
	{
		var toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
		var returnValue = Convert.ToBase64String(toEncodeAsBytes);
		return returnValue;
	}

	private static void StartCoroutine(IEnumerator coroutine)
	{
		if(_coroutineObject == null)
		{
			var go = new GameObject("Mixpanel Coroutines");
			UnityEngine.Object.DontDestroyOnLoad(go);
			_coroutineObject = go.AddComponent<MonoBehaviour>();
		}

		_coroutineObject.StartCoroutine(coroutine);
	}

	private static IEnumerator SendEventCoroutine(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		if(www.error != null)
			Debug.LogWarning("Error sending mixpanel event: " + www.error);
		else if(www.text.Trim() == "0")
			Debug.LogWarning("Error on mixpanel processing event: " + www.text);
		else if(EnableLogging)
			Debug.Log("Mixpanel processed event: " + www.text);
	}
}
