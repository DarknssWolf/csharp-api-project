namespace CSharpAPIProject.Domain.Enums
{
	public enum QuestionType
	{
		/// <summary>
		/// Free Text
		/// </summary>
		OpenText = 0,
		/// <summary>
		/// Only One option allowed (radio, yes/no)
		/// </summary>
		MultipleChoice = 1,
		///<summary>
		///Multiple answers allowed (click to select)
		///</summary>
		MultiSelect = 2,
	}
}
