using CSharpAPIProject.Application.VoteSets.Exceptions;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.VoteSets.Helpers;

public static class VoteSetHelper
{

    /// <summary>
    /// Validates if the text exceeds the maximum number of words.
    /// </summary>
    /// <param name="wordLimit"></param>
    /// <param name="text"></param>
    /// <returns>'true' if within limit, 'false if exceeds limit'</returns>
    public static bool ValidateMaxXWords(int wordLimit, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        var wordCount = text
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return wordCount <= wordLimit;
    }
    public static void ValidateVoteItem(VoteItem voteItem)
	{
        if (!ValidateMaxXWords(200, voteItem.Title))
            throw new TextExceedsWordCountException(50, "Item Title");

        if (!ValidateMaxXWords(200, voteItem.Description))
            throw new TextExceedsWordCountException(200, "Item Description");

		//Check for No No Words
		string[] bannedWords = new string[] { "fuck", "bitch", "poes", "fucked" };
		string[] wordsInDescription = voteItem.Description.Split(" ").Select(x => x.Trim().ToLower()).ToArray();
		foreach (string word in wordsInDescription)
			if (bannedWords.Contains(word))
				throw new BannedWordException(voteItem.VoteItemId, word);
	}
	public static void ValidateVoteSubmission(VoteItem voteItem)
	{
		if (voteItem.Status != VoteItemStatus.Approve)
			throw new Exception("VoteItem must be approved before it can be submitted");

	}
}
