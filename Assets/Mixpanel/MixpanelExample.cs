
using System.Collections.Generic;
using UnityEngine;

public class MixpanelExample : MonoBehaviour
{
	public string Token;
	public string DistinctID;

	private string _eventName = "test";
	private string _property1 = "foo";
	private string _property2 = "bar";

	public void Start()
	{
		Mixpanel.Token = Token;
		Mixpanel.DistinctID = DistinctID;

		// Set some "super properties" to be sent with every event.
		Mixpanel.SuperProperties.Add("platform", Application.platform.ToString());
		Mixpanel.SuperProperties.Add("quality", QualitySettings.names[QualitySettings.GetQualityLevel()]);
		Mixpanel.SuperProperties.Add("fullscreen", Screen.fullScreen);
		Mixpanel.SuperProperties.Add("resolution", Screen.width + "x" + Screen.height);
	}

	public void OnGUI()
	{
		GUILayout.Label("This is an example demonstrating how to use the Mixpanel integration plugin for Unity3D.");
		GUILayout.Label("All source code for this example is located in \"Assets/Mixpanel Analytics/MixpanelExample.cs\".");

		if(string.IsNullOrEmpty(Mixpanel.Token))
		{
			GUI.color = Color.red;
			GUILayout.Label("Step 1: Set the Token property on the 'Mixpanel Example' object to your unique Mixpanel token string.");
		}
	
		if(string.IsNullOrEmpty(Mixpanel.Token))
			return;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Event Name:");
		_eventName = GUILayout.TextField(_eventName);
		GUILayout.EndHorizontal();
	
		GUILayout.BeginHorizontal();
		GUILayout.Label("Property 1:");
		_property1 = GUILayout.TextField(_property1);
		GUILayout.EndHorizontal();
	
		GUILayout.BeginHorizontal();
		GUILayout.Label("Property 2:");
		_property2 = GUILayout.TextField(_property2);
		GUILayout.EndHorizontal();
	
		if(GUILayout.Button("Send Event"))
		{
			Mixpanel.SendEvent(_eventName, new Dictionary<string, object>
			{
				{"property1", _property1},
				{"property2", _property2},
			});
		}
	}
}