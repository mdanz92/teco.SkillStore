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
		mono.android.Runtime.register ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", StorageService_StorageThread.class, __md_methods);
	}


	public StorageService_StorageThread () throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public StorageService_StorageThread (java.lang.Runnable p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Java.Lang.IRunnable, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
	}


	public StorageService_StorageThread (java.lang.Runnable p0, java.lang.String p1) throws java.lang.Throwable
	{
		super (p0, p1);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Java.Lang.IRunnable, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1 });
	}


	public StorageService_StorageThread (java.lang.String p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0 });
	}


	public StorageService_StorageThread (java.lang.ThreadGroup p0, java.lang.Runnable p1) throws java.lang.Throwable
	{
		super (p0, p1);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Java.Lang.ThreadGroup, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Java.Lang.IRunnable, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0, p1 });
	}


	public StorageService_StorageThread (java.lang.ThreadGroup p0, java.lang.Runnable p1, java.lang.String p2) throws java.lang.Throwable
	{
		super (p0, p1, p2);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Java.Lang.ThreadGroup, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Java.Lang.IRunnable, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public StorageService_StorageThread (java.lang.ThreadGroup p0, java.lang.Runnable p1, java.lang.String p2, long p3) throws java.lang.Throwable
	{
		super (p0, p1, p2, p3);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Java.Lang.ThreadGroup, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Java.Lang.IRunnable, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e:System.Int64, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1, p2, p3 });
	}


	public StorageService_StorageThread (java.lang.ThreadGroup p0, java.lang.String p1) throws java.lang.Throwable
	{
		super (p0, p1);
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Java.Lang.ThreadGroup, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1 });
	}

	public StorageService_StorageThread (md515d084dca8ae2ee6860fb1e74170b178.StorageService p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == StorageService_StorageThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Storage.StorageService+StorageThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SkillStore.Service.Storage.StorageService, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
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
