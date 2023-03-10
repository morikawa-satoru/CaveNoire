using UnityEditor;

namespace KoganeEditorUtils
{
	public static class LockInspector
	{
		private const string ITEM_NAME = "Edit/Plus/Lock Inspector &l";

		[MenuItem( ITEM_NAME )]
		private static void Lock()
		{
			var tracker = ActiveEditorTracker.sharedTracker;
			tracker.isLocked = !tracker.isLocked;
			tracker.ForceRebuild();
		}

		[MenuItem( ITEM_NAME, true )]
		private static bool CanLock()
		{
			return ActiveEditorTracker.sharedTracker != null;
		}
	}
}