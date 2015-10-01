package md50dd9a1ea6cec4e69ac111f567e48bf8b;


public class AudioRecorderBinder
	extends android.os.Binder
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("SkillStore.Service.Recorder.AudioRecorderBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", AudioRecorderBinder.class, __md_methods);
	}


	public AudioRecorderBinder () throws java.lang.Throwable
	{
		super ();
		if (getClass () == AudioRecorderBinder.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Recorder.AudioRecorderBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public AudioRecorderBinder (md50dd9a1ea6cec4e69ac111f567e48bf8b.AudioRecorderService p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == AudioRecorderBinder.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Recorder.AudioRecorderBinder, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SkillStore.Service.Recorder.AudioRecorderService, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
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
