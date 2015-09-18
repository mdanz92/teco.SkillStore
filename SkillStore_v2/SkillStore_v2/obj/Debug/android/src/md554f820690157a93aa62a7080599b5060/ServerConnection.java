package md554f820690157a93aa62a7080599b5060;


public class ServerConnection
	extends md58a7c6d2d73840d58de532ef0e1fb3639.ServiceConnection
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkillStore.Service.Communication.ServerConnection, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ServerConnection.class, __md_methods);
	}


	public ServerConnection () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ServerConnection.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Communication.ServerConnection, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

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
