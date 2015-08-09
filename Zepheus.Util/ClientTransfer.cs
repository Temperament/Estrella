using System;

namespace Zepheus.Util
{
	public sealed class ClientTransfer
	{
		public string Hash { get; private set; }
		public ushort RandID { get; private set; }
		public string CharacterName { get; private set; }
        public int CharID { get; private set; }
		public int AccountID { get; private set; }
		public byte Admin { get; private set; }
		public string Username { get; private set; }
		public string HostIP { get; private set; }
		public DateTime Time { get; private set; }
		public TransferType Type { get; private set; }

		public ClientTransfer(int accountID, string userName,int CharID, byte admin, string hostIP, string hash)
		{
			this.Type = TransferType.World;
			this.AccountID = accountID;
			this.Username = userName;
            this.CharID = CharID;
			this.Admin = admin;
			this.HostIP = hostIP;
			this.Hash = hash;
			this.Time = DateTime.Now;
		}

		public ClientTransfer(int accountID, string userName, string charName,int CharID, ushort randid, byte admin, string hostIP)
		{
			this.Type = TransferType.Game;
			this.AccountID = accountID;
			this.Username = userName;
			this.Admin = admin;
			this.HostIP = hostIP;
			this.CharacterName = charName;
            this.CharID = CharID;
			this.RandID = randid;
			this.Time = DateTime.Now;
		}
	}
}
