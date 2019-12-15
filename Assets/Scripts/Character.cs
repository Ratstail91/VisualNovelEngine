using System;
using CSString = System.String;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Toy {
	namespace PluginExtras {
		public class Character : ICallable, IBundle {
			public override string ToString() { return "<Visual character>"; }

			//callables
			LoadImage loadImage;
			UnloadImage unloadImage;
			SetPosition setPosition;

			//members
			GameObject gameObject;
			public string characterName = "default";
			string characterClothes = "default";
			string characterExpression = "default";
			public string characterColor = "white";
			public int characterPosition = -1;
			public static int characterCount = 0;
			static List<Character> characterList = new List<Character>(); //for position rubbish

			public Character(string imageName, string characterColor) {
				this.characterColor = characterColor;

				loadImage = new LoadImage(this);
				unloadImage = new UnloadImage(this);
				setPosition = new SetPosition(this);

				gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("CharacterPrefab"));

				string[] words = imageName.Split('-');

				//backwards from SetImageName()
				if (words.Length >= 1) {
					characterName = words[0];
				}

				if (words.Length >= 2) {
					characterClothes = words[1];
				}

				if (words.Length >= 3) {
					characterExpression = words[2];
				}

				if (characterName.Length > 0) {
					loadImage.PushLoadImageEvent(true);
				}
			}

			//ICallable
			public int Arity() {
				return 1;
			}

			public object Call(Interpreter interpreter, Token token, List<object> arguments) {
				if (!(arguments[0] is string)) {
					throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected string)");
				}

				Timeline.PushEvent(Timeline.EventType.SET_TEXT, new List<object>() { this, arguments[0] }, true);

				return null;
			}

			//IBundle
			public object Property(Interpreter interpreter, Token token, object argument) {
				string propertyName = (string)argument;

				switch(propertyName) {
					case "LoadImage": return loadImage;
					case "UnloadImage": return unloadImage;
					case "SetPosition": return setPosition;

					default:
						throw new ErrorHandler.RuntimeError(token, "Unknown property '" + propertyName + "'");
				}
			}

			class LoadImage : ICallable {
				public override string ToString() { return "<native function>"; }

				Character self;

				public LoadImage(Character self) {
					this.self = self;
				}

				public int Arity() {
					return 2;
				}

				public object Call(Interpreter interpreter, Token token, List<object> arguments) {
					if (!(arguments[0] is string)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected string)");
					}

					if (!(arguments[1] is bool)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected boolean)");
					}

					SetImageName((string)arguments[0]);

					PushLoadImageEvent((bool)arguments[1]);

					return null;
				}

				public void SetImageName(string imageName) {
					string[] words = imageName.Split('-');

					if (words.Length == 1) {
						string[] newWords = new string[3];
						newWords[0] = self.characterName;
						newWords[1] = self.characterClothes;
						newWords[2] = words[0];
						words = newWords;
					}

					else if (words.Length == 2) {
						string[] newWords = new string[3];
						newWords[0] = self.characterName;
						newWords[1] = words[0];
						newWords[2] = words[1];
						words = newWords;
					}

					self.characterName = words[0];
					self.characterClothes = words[1];
					self.characterExpression = words[2];
				}

				public void PushLoadImageEvent(bool block) {
					Timeline.PushEvent(Timeline.EventType.LOAD_CHARACTER_IMAGE, new List<object>() { self, self.gameObject, self.characterName, self.characterClothes, self.characterExpression }, block);
				}
			}

			class UnloadImage : ICallable {
				public override string ToString() { return "<native function>"; }

				Character self;

				public UnloadImage(Character self) {
					this.self = self;
				}

				public int Arity() {
					return 1;
				}

				public object Call(Interpreter interpreter, Token token, List<object> arguments) {
					if (!(arguments[0] is bool)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected boolean)");
					}

					Timeline.PushEvent(Timeline.EventType.UNLOAD_CHARACTER_IMAGE, new List<object>() { self, self.gameObject }, (bool)arguments[0]);

					return null;
				}
			}

			class SetPosition : ICallable {
				public override string ToString() { return "<native function>"; }

				Character self;

				public SetPosition(Character self) {
					this.self = self;
				}

				public int Arity() {
					return 2;
				}

				public object Call(Interpreter interpreter, Token token, List<object> arguments) {
					if (!(arguments[0] is double)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected number)");
					}

					if (!(arguments[1] is bool)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected boolean)");
					}

					int pos = (int)(double)arguments[0];

					Timeline.PushEvent(Timeline.EventType.SET_CHARACTER_POSITION, new List<object>() { self, self.gameObject, pos }, (bool)arguments[1]);

					return null;
				}
			}

			//utility functions
			public void CorrectPosition() {
				//well this is dumb
				characterPosition = Mathf.Clamp(characterPosition, 0, characterList.Count - 1);

				if (characterList.Count >= 2) {
					gameObject.GetComponent<Rigidbody2D>().position = new Vector2(-Screen.width/100 + Screen.width/(characterList.Count * 100f) + (Screen.width/50f * characterPosition / characterList.Count), gameObject.GetComponent<Rigidbody2D>().position.y);
				} else {
					gameObject.GetComponent<Rigidbody2D>().position = new Vector2(0, 0);
				}
			}

			public void CorrectAllPositions() {
				for (int i = 0; i < characterList.Count; i++) {
					characterList[i].CorrectPosition();
				}
			}

			public void RemovePositionAt(int i) {
				foreach(Character c in characterList) {
					if (c.characterPosition > i) {
						c.characterPosition--;
					}
				}

				characterList.Remove(this);
				CorrectAllPositions();
			}

			public void SetPos(int i) {
				if (i < 0) {
					characterPosition = characterList.Count;
					characterList.Add(this);
				} else {
					int left = Math.Min(i, characterPosition);
					int right = Math.Max(i, characterPosition);
					int shift = i - characterPosition;

					foreach(Character c in characterList) {
						if (left <= c.characterPosition && right >= c.characterPosition) {
							if (shift > 0) {
								c.characterPosition--;
							} else {
								c.characterPosition++;
							}
						}
					}

					characterPosition = i;
				}

				CorrectAllPositions();
			}
		}
	}
}
