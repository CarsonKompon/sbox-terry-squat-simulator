﻿using Sandbox;
using Sandbox.UI;
using TSS.UI;

namespace TSS
{
	public partial class TSSHud : Sandbox.HudEntity<RootPanel>
	{
		public static TSSHud Instance;

		public TSSHud()
		{
			if ( Game.IsClient )
			{
				Instance = this;
				RootPanel.AddChild<UIPanel>();
				RootPanel.AddChild<IntroPanel>();
				RootPanel.AddChild<EndingPanel>();
				RootPanel.AddChild<TwitchPanel>();
			}
		}
	}
}

