package md5f6570ba49d893d0166d1f50f58ba930f;


public class SlideFragmentAdapter
	extends android.support.v13.app.FragmentPagerAdapter
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_getCount:()I:GetGetCountHandler\n" +
			"n_getItem:(I)Landroid/app/Fragment;:GetGetItem_IHandler\n" +
			"";
		mono.android.Runtime.register ("SkillStore.Help.Views.SlideFragmentAdapter, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SlideFragmentAdapter.class, __md_methods);
	}


	public SlideFragmentAdapter (android.app.FragmentManager p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == SlideFragmentAdapter.class)
			mono.android.TypeManager.Activate ("SkillStore.Help.Views.SlideFragmentAdapter, SkillStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.App.FragmentManager, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
	}


	public int getCount ()
	{
		return n_getCount ();
	}

	private native int n_getCount ();


	public android.app.Fragment getItem (int p0)
	{
		return n_getItem (p0);
	}

	private native android.app.Fragment n_getItem (int p0);

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
