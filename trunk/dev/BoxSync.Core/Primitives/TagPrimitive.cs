using System;
using System.Linq.Expressions;


namespace BoxSync.Core.Primitives
{
	/// <summary>
	/// Represents Box.NET tag entity
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("ID = {ID}")]
	public struct TagPrimitive
	{
		private readonly long _id;
		private string _text;
		private readonly Expression<Func<long, TagPrimitive>> _materialize;

		public TagPrimitive(long id, Expression<Func<long, TagPrimitive>> materialize)
		{
			_id = id;
			_text = null;
			_materialize = materialize;
		}

		public TagPrimitive(long id, string text)
		{
			_id = id;
			_text = text ?? string.Empty;
			_materialize = null;
		}

		/// <summary>
		/// ID of the tag
		/// </summary>
		public long ID
		{
			get
			{
				return _id;
			}
		}

		/// <summary>
		/// Tag text
		/// </summary>
		public string Text
		{
			get
			{
				if (_text == null && _materialize != null)
				{
					TagPrimitive tag = _materialize.Compile()(_id);

					_text = tag.Text;
				}
				
				return _text;
			}
			set
			{
				_text = value ?? string.Empty;
			}
		}
	}
}
