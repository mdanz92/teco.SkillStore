package md554f820690157a93aa62a7080599b5060;


public class ServerCommService_CommThread
	extends java.lang.Thread
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_run:()V:GetRunHandler\n" +
			"";
		mono.android.Runtime.register ("SkillStore.Service.Communication.ServerCommService/CommThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ServerCommService_CommThread.class, __md_methods);
	}


	public ServerCommService_CommThread () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ServerCommService_CommThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Communication.ServerCommService/CommThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void run ()
	{
		n_run ();
	}

	private native void n_run ();

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
