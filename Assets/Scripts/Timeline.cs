using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timeline {
	//structures
	public enum EventType {
		LOAD_BACKGROUND,
		LOAD_CHARACTER_IMAGE,
		UNLOAD_CHARACTER_IMAGE,
		SET_CHARACTER_POSITION,
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

	static GameObject dialogCanvas = null;

	//accessors/mutators
	public static void PushEvent(EventType type, List<object> arguments, bool block) {
		eventList.Add(new Event(type, arguments, block));
	}

	public static void Restart() {
		if (dialogCanvas == null) {
			dialogCanvas = GameObject.Find("DialogCanvas");
			dialogCanvas.active = false;
		}

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
			case EventType.LOAD_BACKGROUND: {
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
			}

			case EventType.LOAD_CHARACTER_IMAGE: {
				Toy.PluginExtras.Character character = (Toy.PluginExtras.Character)eventList[counter].arguments[0];

				GameObject go = (GameObject)eventList[counter].arguments[1];

				//handle undos
				if (undo) {
					go.GetComponent<SpriteRenderer>().sprite = (Sprite)eventList[counter].undo;
					if ((Sprite)eventList[counter].undo == null) {
						character.RemovePositionAt(character.characterPosition);
						character.characterPosition = -1;
					}
					return;
				}

				eventList[counter].undo = go.GetComponent<SpriteRenderer>().sprite;

				string name = (string)eventList[counter].arguments[2];
				string clothes = (string)eventList[counter].arguments[3];
				string expression = (string)eventList[counter].arguments[4];

				go.GetComponent<SpriteRenderer>().sprite = SpriteLoader($"{name}-{clothes}-{expression}.png");

				character.SetPos(character.characterPosition);

				return;
			}

			case EventType.UNLOAD_CHARACTER_IMAGE: {
				Toy.PluginExtras.Character character = (Toy.PluginExtras.Character)eventList[counter].arguments[0];

				GameObject go = (GameObject)eventList[counter].arguments[1];

				//handle undos
				if (undo) {
					go.GetComponent<SpriteRenderer>().sprite = (Sprite)eventList[counter].undo;
					return;
				}

				eventList[counter].undo = go.GetComponent<SpriteRenderer>().sprite;

				go.GetComponent<SpriteRenderer>().sprite = null;

				character.RemovePositionAt(character.characterPosition);

				return;
			}

			case EventType.SET_CHARACTER_POSITION: {
				Toy.PluginExtras.Character character = (Toy.PluginExtras.Character)eventList[counter].arguments[0];

				//handle undos
				if (undo) {
					character.SetPos((int)eventList[counter].undo);
					return;
				}

				eventList[counter].undo = character.characterPosition;

				character.SetPos((int)eventList[counter].arguments[2]);

				return;
			}

			case EventType.SET_TEXT: {
				Toy.PluginExtras.Character character = (Toy.PluginExtras.Character)eventList[counter].arguments[0];

				dialogCanvas.SetActive(true);
				GameObject title = GameObject.Find("TitleText");
				GameObject main = GameObject.Find("MainText");

				//handle undos
				if (undo) {
					var t = (Tuple<Color, string, string>)eventList[counter].undo;

					title.GetComponent<TextMeshProUGUI>().color = t.Item1;
					title.GetComponent<TextMeshProUGUI>().text = t.Item2;
					main.GetComponent<TextMeshProUGUI>().color = t.Item1;
					main.GetComponent<TextMeshProUGUI>().text = t.Item3;

					dialogCanvas.SetActive(main.GetComponent<TextMeshProUGUI>().text.Length > 0);

					return;
				}

				eventList[counter].undo = new Tuple<Color, string, string>(
					title.GetComponent<TextMeshProUGUI>().color,
					title.GetComponent<TextMeshProUGUI>().text,
					main.GetComponent<TextMeshProUGUI>().text
				);

				Color color = Color.clear;
				ColorUtility.TryParseHtmlString(character.characterColor, out color);

				title.GetComponent<TextMeshProUGUI>().color = color;
				title.GetComponent<TextMeshProUGUI>().text = character.characterName;
				main.GetComponent<TextMeshProUGUI>().color = color;
				main.GetComponent<TextMeshProUGUI>().text = (string)eventList[counter].arguments[1];

				dialogCanvas.SetActive(main.GetComponent<TextMeshProUGUI>().text.Length > 0);

				return;
			}

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