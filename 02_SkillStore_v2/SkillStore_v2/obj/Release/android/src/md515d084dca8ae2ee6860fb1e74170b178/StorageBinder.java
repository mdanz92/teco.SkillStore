package md515d084dca8ae2ee6860fb1e74170b178;


public class StorageBinder
	extends android.os.Binder
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkillStore.Service.Storage.StorageBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", StorageBinder.class, __md_methods);
	}


	public StorageBinder () throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageBinder.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public StorageBinder (md515d084dca8ae2ee6860fb1e74170b178.StorageService p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageBinder.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SkillStore.Service.Storage.StorageService, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
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
