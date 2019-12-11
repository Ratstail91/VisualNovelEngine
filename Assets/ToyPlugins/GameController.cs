using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	void Start() {
		Timeline.FirstEvent();
	}

	void Update() {
		if (Input.GetMouseButtonUp(0)) {
			Timeline.NextEvent();
		} else if (Input.GetMouseButtonUp(1)) {
			Timeline.PrevEvent();
		}
	}
}