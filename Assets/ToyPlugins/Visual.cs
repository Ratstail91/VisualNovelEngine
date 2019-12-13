using System;
using CSString = System.String;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Toy {
	namespace Plugin {
		public class Visual : IPlugin {
			//singleton pattern
			public IPlugin Singleton {
				get {
					if (singleton == null) {
						return singleton = new Visual();
					}
					return singleton;
				}
			}
			static Visual singleton = null;

			//the persistent functors
			static LoadCharacter loadCharacter = new LoadCharacter();
			static LoadBackground loadBackground = new LoadBackground();

			public void Initialize(Environment env, string alias) {
				if (CSString.IsNullOrEmpty(alias)) {
					//no alias, put these in the global scope
					env.Define("LoadCharacter", loadCharacter, true);
					env.Define("LoadBackground", loadBackground, true);
				} else {
					env.Define(alias, new VisualBundle(), true);
				}
			}

			//member class - the library as a bundle (for the alias)
			public class VisualBundle : IBundle {
				public override string ToString() { return "<native library>"; }

				public object Property(Interpreter interpreter, Token token, object argument) {
					string propertyName = (string)argument;

					switch(propertyName) { //TODO: string constants (split(), format())
						case "LoadCharacter": return loadCharacter;
						case "LoadBackground": return loadBackground;
						default:
							throw new ErrorHandler.RuntimeError(token, "Unknown property '" + propertyName + "'");
					}
				}
			}

			//member classes representing functions
			public class LoadCharacter : ICallable {
				public override string ToString() { return "<native function>"; }

				public int Arity() {
					return 2;
				}

				public object Call(Interpreter interpreter, Token token, List<object> arguments) {
					if (!(arguments[0] is string)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected string)");
					}

					if (!(arguments[1] is string)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected string)");
					}

					return new PluginExtras.Character((string)arguments[0], (string)arguments[1]);
				}
			}

			public class LoadBackground : ICallable {
				public override string ToString() { return "<native function>"; }

				public int Arity() {
					return 1;
				}

				public object Call(Interpreter interpreter, Token token, List<object> arguments) {
					if (!(arguments[0] is string)) {
						throw new ErrorHandler.RuntimeError(token, "Unexpected type received (expected string)");
					}

					Timeline.PushEvent(Timeline.EventType.LOAD_BACKGROUND, arguments, false);

					return null;
				}
			}
		}
	}
}
