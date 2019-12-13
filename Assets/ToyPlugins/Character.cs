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

			//members
			GameObject gameObject;
			string characterName = "default";
			string characterClothes = "default";
			string characterExpression = "default";
			string characterColor;

			public Character(string imageName, string characterColor) {
				this.characterColor = characterColor;

				loadImage = new LoadImage(this);

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

				loadImage.PushLoadImageEvent(true);
			}

			//ICallable
			public int Arity() {
				return 1;
			}

			public object Call(Interpreter interpreter, Token token, List<object> arguments) {
				//TODO: call character(arg)
				return null;
			}

			//IBundle
			public object Property(Interpreter interpreter, Token token, object argument) {
				string propertyName = (string)argument;

				switch(propertyName) {
					case "LoadImage": return loadImage;

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
					Timeline.PushEvent(Timeline.EventType.LOAD_CHARACTER_IMAGE, new List<object>() { this.self.gameObject, this.self.characterName, this.self.characterClothes, this.self.characterExpression }, block);
				}
			}
		}
	}
}
