namespace _3rd.HighlightPlus.Demo.Scripts {
	using HighlightPlus.Scripts;
	using UnityEngine;

	public class SphereSelectionEventsExample : MonoBehaviour {

		void Start() {
			HighlightManager.instance.OnObjectSelected += OnObjectSelected;
			HighlightManager.instance.OnObjectUnSelected += OnObjectUnSelected;
		}

		bool OnObjectSelected(GameObject go) {
			Debug.Log(go.name + " selected!");
			return true;
        }

		bool OnObjectUnSelected(GameObject go) {
			Debug.Log(go.name + " un-selected!");
			return true;
		}


	}

}