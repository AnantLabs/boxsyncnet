using BoxSync.Core.Statuses;


namespace BoxSync.Core.Primitives
{
	public class ExportTagsResponse : ResponseBase<ExportTagsStatus>
	{
		public TagPrimitiveCollection TagsList
		{
			get; 
			set;
		}
	}
}
