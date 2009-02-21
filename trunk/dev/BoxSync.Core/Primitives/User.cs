using System;
using System.Linq.Expressions;

using BoxSync.Core.ServiceReference;


namespace BoxSync.Core.Primitives
{
	public class User
	{
		private Expression<Func<int, SOAPUser>> _materialize;
		private bool isMaterialized;
		private int _id;
		private int _accessID;
		private string _email;
		private string _login;
		private long _maxUploadSize;
		private long _spaceAmount;
		private long _spaceUsed;

		public User()
		{
		}

		internal User(SOAPUser user)
		{
			Initialize(user);
		}

		public User(int id, Expression<Func<int, SOAPUser>> materialize)
		{
			_id = id;
			_materialize = materialize;
		}

		private void Materialize()
		{
			SOAPUser user = _materialize.Compile()(_id);

			Initialize(user);
		}

		private void Initialize(SOAPUser user)
		{
			isMaterialized = true;

			_id = user.user_id;
			_accessID = user.access_id;
			_email = user.email;
			_login = user.login;
			_maxUploadSize = user.max_upload_size;
			_spaceAmount = user.space_amount;
			_spaceUsed = user.space_used;
		}

		public int ID
		{
			get { return _id; }
		}

		public string Email
		{
			get
			{
				if (!isMaterialized)
				{
					Materialize();
				}

				return _email;
			}
		}

		public string Login
		{
			get
			{
				if (!isMaterialized)
				{
					Materialize();
				}

				return _login;
			}
		}

		public long MaxUploadSize
		{
			get
			{
				return _maxUploadSize;
			}
		}

		public long SpaceAmount
		{
			get
			{
				return _spaceAmount;
			}
		}

		public long SpaceUsed
		{
			get
			{
				return _spaceUsed;
			}
		}

		public int AccessID
		{
			get { return _accessID; }
			set { _accessID = value; }
		}
	}
}
