using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerIOClient;
using System.Threading;

namespace MultiAlt
{
	public delegate void Recieve(Message e, Connection con, Alt id);
	public delegate void Disconnect(object sender, string message);

	public class AltManager
	{
		private List<Alt> alts;
		public event Recieve GotMessage;
		public event Disconnect Disconnected;

		public AltManager()
		{
			alts = new List<Alt>();
		}

		#region Split Work
		public void SplitWork(Action BeforeSend, List<Message> Work, Action AfterSend, Func<Message, bool> DetermingSend)
		{
			List<List<Message>> AltWork = new List<List<Message>>();

			int Alts = alts.Count;
			int WorkLoad = Work.Count;

			int MessagesPerAlt = WorkLoad / Alts;

			for (int a = 0; a < Alts; a++)
			{
				List<Message> WorkPayload = new List<Message>();
				for (int b = 0; b < MessagesPerAlt; b++)
				{
					WorkPayload.Add(Work[0]);
					Work.RemoveAt(0);
				}
				if (a == Alts - 1)
				{
					while (Work.Count > 0)
					{
						WorkPayload.Add(Work[0]);
						Work.RemoveAt(0);
					}
				}
				AltWork.Add(WorkPayload);
			}

			foreach (Alt i in alts)
			{
					List<Message> seperateWork = new List<Message>();
					foreach (Message x in AltWork[0])
					{
						seperateWork.Add(x);
					}
					new Thread(delegate()
					{
						foreach (Message x in seperateWork)
						{
							if (DetermingSend == null)
							{
								if (BeforeSend != null)
								{ BeforeSend(); }
								i.Send(x);
								if (AfterSend != null)
								{ AfterSend(); }
							}
							else if(DetermingSend(x))
							{
								if (BeforeSend != null)
								{ BeforeSend(); }
								i.Send(x);
								if (AfterSend != null)
								{ AfterSend(); }
							}
						}
					}).Start();
				AltWork.RemoveAt(0);
			}
		}

		#region Overloads
		public void SplitWork(Action BeforeSend, List<Message> Work)
		{
			SplitWork(BeforeSend, Work, null, null);
		}

		public void SplitWork(List<Message> Work, Action AfterSend)
		{
			SplitWork(null, Work, AfterSend, null);
		}

		public void SplitWork(List<Message> Work)
		{
			SplitWork(null, Work, null, null);
		}

		public void SplitWork(Action BeforeSend, List<Message> Work, Func<Message, bool> DetermineSend)
		{
			SplitWork(BeforeSend, Work, null, DetermineSend);
		}

		public void SplitWork(List<Message> Work, Action AfterSend, Func<Message, bool> DetermineSend)
		{
			SplitWork(null, Work, AfterSend, DetermineSend);
		}

		public void SplitWork(List<Message> Work, Func<Message, bool> DetermineSend)
		{
			SplitWork(null, Work, null, DetermineSend);
		}

		#endregion
		#endregion

		internal void AltGotDisconnected(object sender, string message, Alt id)
		{
			if (alts.Contains(id))
			{
				Disconnected(sender, message);
			}
		}

		internal void AltGotMessage(Message e, Alt id)
		{
			if(alts.Contains(id))
			{
				GotMessage(e, id.connection, id);
			}
		}

		public Alt GetAlt(int index)
		{
			if(alts.Count > index)
			{
				return alts[index];
			}
			throw new IndexOutOfRangeException();
		}

		public Alt GetAlt(string AltId)
		{
			foreach(Alt i in alts)
			{
				if(i.AltId == AltId)
				{
					return i;
				}
			}
			return null;
		}
		
		public void AddAlt(Alt e)
		{
			foreach(Alt i in alts)
			{
				if(i.AltId == e.AltId)
				{
					throw new ArgumentException("An alt with the same id already exists in the Alt Manager.");
				}
			}
			alts.Add(e);
		}

		public List<Alt> GetAlts()
		{
			List<Alt> ret = new List<Alt>();
			foreach(Alt i in alts)
			{
				ret.Add(i);
			}
			return ret;
		}

		public void RemoveAlt(Alt e)
		{
			alts.Remove(e);
		}

		public void Send(Message e)
		{
			foreach (Alt i in alts)
			{
				if(i.connection != null)
				{
					if(i.connection.Connected)
					{
						i.connection.Send(e);
					}
				}
			}
		}

		public void Send(string type, params object[] values)
		{
			foreach (Alt i in alts)
			{
				if (i.connection != null)
				{
					if (i.connection.Connected)
					{
						i.connection.Send(type, values);
					}
				}
			}
		}
		
		public void JoinRoom(string roomId, Dictionary<string, string> joinData)
		{
			foreach(Alt i in alts)
			{
				Connection joinWorld = i.client.Multiplayer.JoinRoom(roomId, joinData);
				i.JoinWorld(joinWorld);
			}
		}

		public void CreateJoinRoom(string roomId, string roomType, bool visible, Dictionary<string, string> roomData, Dictionary<string, string> joinData)
		{
			foreach (Alt i in alts)
			{
				if (i.connection != null)
				{
					if (i.connection.Connected)
					{
						i.connection.Disconnect();
					}
				}
				Connection joinWorld = i.client.Multiplayer.CreateJoinRoom(roomId, roomType, visible, roomData, joinData);
				i.JoinWorld(joinWorld);
			}
		}
	}
}
