using System.Collections.Generic;

namespace JuliusSweetland.OptiKids.Models
{
    public class Quiz
    {
        public Quiz(string description,
            bool wordAudioHints, bool spellingAudioHints,
            int displayImageForXSeconds, bool randomiseWords,
            bool randomiseLetters, bool displayWordMasks,
            int hintEveryXIncorrectLetters, List<Question> questions)
        {
            Description = description;
            WordAudioHints = wordAudioHints;
            SpellingAudioHints = spellingAudioHints;
            DisplayImageForXSeconds = displayImageForXSeconds;
            RandomiseWords = randomiseWords;
            RandomiseLetters = randomiseLetters;
            DisplayWordMasks = displayWordMasks;
            HintEveryXIncorrectLetters = hintEveryXIncorrectLetters;
            Questions = questions;
        }

        public string Description { get; private set; }
        public bool WordAudioHints { get; private set; }
        public bool SpellingAudioHints { get; private set; }
        public int DisplayImageForXSeconds { get; private set; }
        public bool RandomiseWords { get; private set; }
        public bool RandomiseLetters { get; private set; }
        public bool DisplayWordMasks { get; private set; }
        public int HintEveryXIncorrectLetters { get; private set; }
        public List<Question> Questions { get; private set; }
    }
}
