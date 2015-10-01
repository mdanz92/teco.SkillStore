package md515d084dca8ae2ee6860fb1e74170b178;


public class StorageService_StorageThread
	extends java.lang.Thread
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_run:()V:GetRunHandler\n" +
			"";
		mono.android.Runtime.register ("SkillStore.Service.Storage.StorageService/StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", StorageService_StorageThread.class, __md_methods);
	}


	public StorageService_StorageThread () throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService/StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public StorageService_StorageThread (md515d084dca8ae2ee6860fb1e74170b178.StorageService p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService/StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SkillStore.Service.Storage.StorageService, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
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
