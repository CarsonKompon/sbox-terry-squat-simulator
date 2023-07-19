
using Sandbox;
using Sandbox.PostProcess;
using System.Linq;

namespace TSS
{
	public partial class TSSGame : GameManager
	{
		public TSSGame()
		{
			if ( Game.IsServer )
			{
				_ = new TSSHud();
				DequeueLoop();
			}

			if ( Game.IsClient )
			{
				// PAINDAY TODO: Re-implement this
				// PostProcess.Add( new VHSPostProcess() );
				// var vhsInvert = PostProcess.Get<VHSPostProcess>();
				// vhsInvert.Enabled = true;
			}
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			//Do nothing
		}

		public override void ClientJoined( IClient client )
		{
			base.ClientJoined( client );

			var player = new TSSPlayer();
			client.Pawn = player;
			player.Respawn();
		}

		// Helper field that casts game.
		public static new TSSGame Current => GameManager.Current as TSSGame;

		// Get the player, there should only be one.
		public static TSSPlayer Pawn => All.OfType<TSSPlayer>().First();
	}
}
