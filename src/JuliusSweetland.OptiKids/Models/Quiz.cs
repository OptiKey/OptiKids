using System.Collections.Generic;

namespace JuliusSweetland.OptiKids.Models
{
    public class Quiz
    {
        public Quiz(bool wordAudioHints, bool spellingAudioHints,
            int displayImageForXSeconds, bool randomiseWords,
            bool randomiseLetters, bool displayWordMasks,
            bool hintEveryXIncorrectLetters, List<Question> questions)
        {
            WordAudioHints = wordAudioHints;
            SpellingAudioHints = spellingAudioHints;
            DisplayImageForXSeconds = displayImageForXSeconds;
            RandomiseWords = randomiseWords;
            RandomiseLetters = randomiseLetters;
            DisplayWordMasks = displayWordMasks;
            HintEveryXIncorrectLetters = hintEveryXIncorrectLetters;
            Questions = questions;
        }

        public bool WordAudioHints { get; private set; }
        public bool SpellingAudioHints { get; private set; }
        public int DisplayImageForXSeconds { get; private set; }
        public bool RandomiseWords { get; private set; }
        public bool RandomiseLetters { get; private set; }
        public bool DisplayWordMasks { get; private set; }
        public bool HintEveryXIncorrectLetters { get; private set; }
        public List<Question> Questions { get; private set; }
    }
}
