namespace JuliusSweetland.OptiKids.Models
{
    public class Question
    {
        public Question(string word, string letters, string imagePath)
        {
            Word = word;
            Letters = letters;
            ImagePath = imagePath;
        }

        public string Word { get; private set; }
        public string Letters { get; private set; }
        public string ImagePath { get; private set; }
    }
}
