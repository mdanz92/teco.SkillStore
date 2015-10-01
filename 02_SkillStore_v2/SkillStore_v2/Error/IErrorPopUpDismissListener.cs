using System;

namespace SkillStore.Error
{
	interface IErrorPopUpDismissListener
	{
		void OnErrorPopUpDismissEvent(object s, EventArgs e);
	}
}