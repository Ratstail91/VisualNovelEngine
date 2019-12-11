using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timeline {
	//structures
	public enum EventType {
		LOAD_BACKGROUND,
		LOAD_CHARACTER,
	};

	public class Event {
		public EventType type;
		public List<object> arguments;

		public Event(EventType type, List<object> arguments) {
			this.type = type;
			this.arguments = arguments;
		}
	}

	//members
	public static List<Event> eventList = new List<Event>();
	static int counter = 0;

	//accessors/mutators
	public static void PushEvent(EventType type, List<object> arguments) {
		eventList.Add(new Event(type, arguments));
	}

	public static void FirstEvent() {
		counter = 0;
		ExecuteEvent(eventList[counter]);
	}

	public static void NextEvent() {
		if (counter < eventList.Count-1) {
			counter++;
			ExecuteEvent(eventList[counter]);
		}
	}

	public static void PrevEvent() {
		if (counter > 0) {
			counter--;
			ExecuteEvent(eventList[counter]);
		}
	}

	static void ExecuteEvent(Event e) {
		switch(e.type) {
			case EventType.LOAD_BACKGROUND:
				GameObject bg = GameObject.Find("BackgroundImage");

				if (bg == null) {
					throw new NullReferenceException("Failed to find BackgroundImage");
				}

				bg.GetComponent<Image>().sprite = SpriteLoader((string)e.arguments[0]);

				if (bg.GetComponent<Image>().sprite == null) {
					throw new System.IO.IOException("Failed to load sprite");
				}

				return;

			//TODO: character manipulation

			default:
				throw new Exception("Unknown EventType");
		}
	}

	//utility functions
	static Sprite SpriteLoader(string fname) {
		string filePath = Application.streamingAssetsPath + "/" + fname;

		byte[] pngBytes = System.IO.File.ReadAllBytes(filePath);

		Texture2D tex = new Texture2D(2, 2);
		tex.LoadImage(pngBytes);

		return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
	}
}