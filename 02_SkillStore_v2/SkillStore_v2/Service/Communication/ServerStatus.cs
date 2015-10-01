

namespace SkillStore.Service.Communication
{
	public enum ServerStatus
	{
		NetworkConnected = 1001,
		NoNetworkConnection,
		Starting,
		NoServerConnection,
		WaitingForResponse,
		SendingSuccessful,
		SendingFailed,
		Stopped
	}
}