# OptiKids
**Spelling tool built upon OptiKey**

Based on OptiKey, OptiKids is a spelling quiz that can be played using a mouse, an eye tracker, or webcam (using a 3rd party application such as eViaCam). The [OptiKey Wiki](http://www.optikey.org) includes a lot of common info, such as how to change settings in the Management Console (some settings differ between the two applications, but the broad strokes are similar enough for the wiki to be useful).

When you run OptiKids you will be asked for a Quiz File, which is a .json file describing the quiz and words to spell. If you copy the DemoQuiz.json file, you can rename and customise it to create your own quizes. You then need to download and specify accompanying image files for each Question (word), although you can leave the *"ImagePath"* out (or leave it blank) if you don't want to display an image for a word.

**N.B. The file path you specify in *"ImagePath"* should be a full path (e.g. "C:\\Users\\Julius\\Download\\Images\\dog.jpg"), rather than the shortened path shown in the DemoQuiz.json. All backslashes "\\" should be doubled up "\\\\" - this is a JSON file requirement.**

*"Word"* specifies the word that the player needs to spell. If *"SpellingAudioHints"* is true the word will be spelled out for the player. If the *"Word"* includes capital letters then these will be spelled out using "adult" pronunciation, whereas lowercase letters will be spelled in "infant" pronunciation. Pronunciation can be customised by amending the Pronunciation\EnglishUK.json file (in the OptiKids install directory), or by creating a new version of this file and changing the "Pronunciation File" setting in the OptiKids Management Console (settings). This file specifies a collection of letter/pronunciation pairs, which are used to create phoneme elements in x-microsoft-ups format. Don't worry if this doesn't make sense, but if it does you can find more info here: https://msdn.microsoft.com/en-us/library/hh378516.aspx with pronunciations listed under the "UPS Phone Tables" section of this document: https://msdn.microsoft.com/en-us/library/office/hh361601(v=office.14).aspx

The *"Letters"* define the available letters from which you will spell the word and will be displayed in order (unless "RandomiseLetters" is set to true). Spaces are accepted in the *"Letters"* and indicate a disabled key - this allows you to space the letters if this is desired (please note that spaces should NOT be included if *"RandomiseLetters"* : true, as this will result in a weird looking onscreen keyboard with random spaces!).

*"SpellingAudioHints"*, if set to true, spells the word out loud for the player before they attempt to type the word. Additionally, if *"HintEveryXIncorrectLetters"* is a positive number, the player will receive an audio hint after they select X (e.g. 3) incorrect letters in a row, as this indicates that they are having trouble spelling the word.

Here is the DemoQuiz.json file for reference:
~~~~
{
  "Description": "Sample quiz",
  "SpellingAudioHints": true,
  "RandomiseWords": true,
  "RandomiseLetters": false,
  "DisplayWordMasks": true,
  "HintEveryXIncorrectLetters": 3,
  "Questions": [
    {
      "Word": "dog",
      "Letters": " adeg | imot ",
      "ImagePath": "\\Resources\\Images\\dog.jpg"
    },
    {
      "Word": "DOG",
      "Letters": "ADEG|IMOT",
      "ImagePath": "\\Resources\\Images\\dog.jpg"
    },
    {
      "Word": "cat",
      "Letters": "achi|knpt",
      "ImagePath": "\\Resources\\Images\\cat.jpg"
    },
    {
      "Word": "CAT",
      "Letters": "ACHI|KNPT",
      "ImagePath": "\\Resources\\Images\\cat.jpg"
    },
    {
      "Word": "ball",
      "Letters": "abeg|hjlp",
      "ImagePath": "\\Resources\\Images\\ball.jpg"
    },
    {
      "Word": "BALL",
      "Letters": "ABEG|HJLP",
      "ImagePath": "\\Resources\\Images\\ball.jpg"
    }
  ]
}
~~~~
**N.B. The format of the quiz file is very important, as it must be a valid JSON file. If you're having trouble or have a question please email me: optikeyfeedback@gmail.com**

Happy spelling!
Julius
