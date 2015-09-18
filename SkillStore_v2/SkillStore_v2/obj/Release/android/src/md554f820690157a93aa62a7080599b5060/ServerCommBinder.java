package md554f820690157a93aa62a7080599b5060;


public class ServerCommBinder
	extends android.os.Binder
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkillStore.Service.Communication.ServerCommBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ServerCommBinder.class, __md_methods);
	}


	public ServerCommBinder () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ServerCommBinder.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Communication.ServerCommBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public ServerCommBinder (md554f820690157a93aa62a7080599b5060.ServerCommService p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == ServerCommBinder.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Communication.ServerCommBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SkillStore.Service.Communication.ServerCommService, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
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
