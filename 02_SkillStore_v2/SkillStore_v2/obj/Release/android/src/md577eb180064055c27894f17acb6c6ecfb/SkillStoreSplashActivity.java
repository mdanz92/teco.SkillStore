package md577eb180064055c27894f17acb6c6ecfb;


public class SkillStoreSplashActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onStart:()V:GetOnStartHandler\n" +
			"";
		mono.android.Runtime.register ("SkillStore.Main.SkillStoreSplashActivity, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SkillStoreSplashActivity.class, __md_methods);
	}


	public SkillStoreSplashActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SkillStoreSplashActivity.class)
			mono.android.TypeManager.Activate ("SkillStore.Main.SkillStoreSplashActivity, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onStart ()
	{
		n_onStart ();
	}

	private native void n_onStart ();

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
