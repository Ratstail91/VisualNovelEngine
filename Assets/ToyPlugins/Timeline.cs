using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timeline {
	//structures
	public enum EventType {
		LOAD_BACKGROUND,
		LOAD_CHARACTER_IMAGE,
		UNLOAD_CHARACTER,
		SET_TEXT,
	};

	public class Event {
		public EventType type;
		public List<object> arguments;
		public bool block;

		public object undo; //how to undo the event

		public Event(EventType type, List<object> arguments, bool block) {
			this.type = type;
			this.arguments = arguments;
			this.block = block;
		}
	}

	//members
	public static List<Event> eventList = new List<Event>();
	static int counter = 0;

	//accessors/mutators
	public static void PushEvent(EventType type, List<object> arguments, bool block) {
		eventList.Add(new Event(type, arguments, block));
	}

	public static void Restart() {
		counter = 0;
		ExecuteEvent();
		if (!eventList[counter].block) {
			NextEvent();
		}
	}

	public static void NextEvent() {
		if (counter < eventList.Count-1) {
			counter++;
			ExecuteEvent();

			if (!eventList[counter].block) {
				NextEvent();
			}
		}
	}

	public static void PrevEvent() {
		if (counter > 0) {
			ExecuteEvent(true);
			counter--;

			if (counter > 0 && !eventList[counter].block) {
				PrevEvent();
			} else if (counter == 0 && !eventList[counter].block) {
				ExecuteEvent();
				NextEvent();
			}
		}
	}

	static void ExecuteEvent(bool undo = false) {
		switch(eventList[counter].type) {
			case EventType.LOAD_BACKGROUND:
				GameObject bg = GameObject.Find("BackgroundImage");

				if (bg == null) {
					throw new NullReferenceException("Failed to find BackgroundImage");
				}

				//handle undos
				if (undo) {
					bg.GetComponent<Image>().sprite = (Sprite)eventList[counter].undo;
					return;
				}

				eventList[counter].undo = bg.GetComponent<Image>().sprite;

				bg.GetComponent<Image>().sprite = SpriteLoader((string)eventList[counter].arguments[0]);

				if (bg.GetComponent<Image>().sprite == null) {
					throw new System.IO.IOException("Failed to load sprite");
				}

				return;

			case EventType.LOAD_CHARACTER_IMAGE:
				GameObject go = (GameObject)eventList[counter].arguments[0];

				//handle undos
				if (undo) {
					go.GetComponent<SpriteRenderer>().sprite = (Sprite)eventList[counter].undo;
					return;
				}

				eventList[counter].undo = go.GetComponent<SpriteRenderer>().sprite;

				string name = (string)eventList[counter].arguments[1];
				string clothes = (string)eventList[counter].arguments[2];
				string expression = (string)eventList[counter].arguments[3];

				go.GetComponent<SpriteRenderer>().sprite = SpriteLoader($"{name}-{clothes}-{expression}.png");

				return;

			//TODO: character manipulation

			//TODO: text manipulation

			default:
				throw new Exception("Unknown EventType");
		}
	}

	//utility functions
	static Dictionary<string, Sprite> memoization = new Dictionary<string, Sprite>();

	static Sprite SpriteLoader(string fname) {
		string filePath = Application.streamingAssetsPath + "/" + fname;

		if (memoization.ContainsKey(filePath)) {
			return memoization[filePath];
		}

		byte[] pngBytes = System.IO.File.ReadAllBytes(filePath);

		Texture2D tex = new Texture2D(2, 2);
		tex.LoadImage(pngBytes);

		return memoization[filePath] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
	}
}