package md515d084dca8ae2ee6860fb1e74170b178;


public class StorageConnection
	extends md58a7c6d2d73840d58de532ef0e1fb3639.ServiceConnection
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkillStore.Service.Storage.StorageConnection, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", StorageConnection.class, __md_methods);
	}


	public StorageConnection () throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageConnection.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageConnection, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
