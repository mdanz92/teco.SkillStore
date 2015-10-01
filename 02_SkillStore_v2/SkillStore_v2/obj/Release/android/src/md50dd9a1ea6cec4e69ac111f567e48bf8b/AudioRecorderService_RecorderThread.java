package md50dd9a1ea6cec4e69ac111f567e48bf8b;


public class AudioRecorderService_RecorderThread
	extends java.lang.Thread
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_run:()V:GetRunHandler\n" +
			"";
		mono.android.Runtime.register ("SkillStore.Service.Recorder.AudioRecorderService/RecorderThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", AudioRecorderService_RecorderThread.class, __md_methods);
	}


	public AudioRecorderService_RecorderThread () throws java.lang.Throwable
	{
		super ();
		if (getClass () == AudioRecorderService_RecorderThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Recorder.AudioRecorderService/RecorderThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public AudioRecorderService_RecorderThread (md50dd9a1ea6cec4e69ac111f567e48bf8b.AudioRecorderService p0, java.lang.String p1) throws java.lang.Throwable
	{
		super ();
		if (getClass () == AudioRecorderService_RecorderThread.class)
			mono.android.TypeManager.Activate ("SkillStore.Service.Recorder.AudioRecorderService/RecorderThread, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "SkillStore.Service.Recorder.AudioRecorderService, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null:System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1 });
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
