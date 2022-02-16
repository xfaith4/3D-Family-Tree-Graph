using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool previous;
		public bool next;
		public bool menu;
		public bool start;
		public bool interact;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        public void OnMove(InputValue value) => MoveInput(value.Get<Vector2>());

        public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

        public void OnJump(InputValue value) => JumpInput(value.isPressed);

        public void OnSprint(InputValue value) => SprintInput(value.isPressed);

        public void OnPrevious(InputValue value) => PreviousInput(value.isPressed);

        public void OnNext(InputValue value) => NextInput(value.isPressed);

        public void OnMenu(InputValue value) => SelectMenu(value.isPressed);

        public void OnStart(InputValue value) => StartInput(value.isPressed);

        public void OnInteract(InputValue value) => InteractInput(value.isPressed);
#else
	// old input sys if we do decide to have it (most likely wont)...
#endif

        public void MoveInput(Vector2 newMoveDirection) => move = newMoveDirection;

        public void LookInput(Vector2 newLookDirection) => look = newLookDirection;

        public void JumpInput(bool newVal) => jump = newVal;

        public void SprintInput(bool newVal) => sprint = newVal;

        public void PreviousInput(bool newVal) => previous = newVal;

        public void NextInput(bool newVal) => next = newVal;

		public void SelectMenu(bool newVal) => menu = newVal;

		public void StartInput(bool newVal) => start = newVal;

		public void InteractInput(bool newVal) => interact = newVal;

#if !UNITY_IOS || !UNITY_ANDROID

        private void OnApplicationFocus(bool hasFocus) => SetCursorState(cursorLocked);

        private void SetCursorState(bool newState) => Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;

#endif

    }
	
}