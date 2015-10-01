package md5015b23c6fc64eb0b4a810929b418079d;


public class WavRecorder2
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.media.AudioRecord.OnRecordPositionUpdateListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onMarkerReached:(Landroid/media/AudioRecord;)V:GetOnMarkerReached_Landroid_media_AudioRecord_Handler:Android.Media.AudioRecord/IOnRecordPositionUpdateListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onPeriodicNotification:(Landroid/media/AudioRecord;)V:GetOnPeriodicNotification_Landroid_media_AudioRecord_Handler:Android.Media.AudioRecord/IOnRecordPositionUpdateListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("SkillStore.WavFileReader.WavRecorder2, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", WavRecorder2.class, __md_methods);
	}


	public WavRecorder2 () throws java.lang.Throwable
	{
		super ();
		if (getClass () == WavRecorder2.class)
			mono.android.TypeManager.Activate ("SkillStore.WavFileReader.WavRecorder2, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public WavRecorder2 (int p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == WavRecorder2.class)
			mono.android.TypeManager.Activate ("SkillStore.WavFileReader.WavRecorder2, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.Int32, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0 });
	}


	public void onMarkerReached (android.media.AudioRecord p0)
	{
		n_onMarkerReached (p0);
	}

	private native void n_onMarkerReached (android.media.AudioRecord p0);


	public void onPeriodicNotification (android.media.AudioRecord p0)
	{
		n_onPeriodicNotification (p0);
	}

	private native void n_onPeriodicNotification (android.media.AudioRecord p0);

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
