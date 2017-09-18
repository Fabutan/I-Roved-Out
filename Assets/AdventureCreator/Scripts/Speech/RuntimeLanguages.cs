﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2017
 *	
 *	"RuntimeLanguage.cs"
 * 
 *	This script contains all language data for the game at runtime.
 *	It transfers data from the Speech Manaager to itself when the game begins.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_languages.html")]
	#endif
	public class RuntimeLanguages : MonoBehaviour
	{

		private Dictionary<int, SpeechLine> speechLinesDictionary = new Dictionary<int, SpeechLine>(); 
		private List<string> languages = new List<string>();


		public void OnAwake ()
		{
			TransferFromManager ();
		}


		/** The names of the game's languages. The first is always "Original". */
		public List<string> Languages
		{
			get
			{
				return languages;
			}
		}

		
		private void TransferFromManager ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager)
			{
				SpeechManager speechManager = AdvGame.GetReferences ().speechManager;
				
				languages.Clear ();
				foreach (string _language in speechManager.languages)
				{
					languages.Add (_language);
				}

				speechLinesDictionary.Clear ();
				foreach (SpeechLine speechLine in speechManager.lines)
				{
					speechLinesDictionary.Add (speechLine.lineID, speechLine);
				}
			}
		}


		/**
		 * <summary>Gets the translation of a line of text.</summary>
		 * <param name = "originalText">The line in its original language.</param>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <param name = "language">The index number of the language to return the line in, where 0 = the game's original language. </param>
		 * <returns>The translation of the line, if it exists. If a translation does not exist, then the original line will be returned.</returns>
		 */
		public string GetTranslation (string originalText, int _lineID, int language)
		{
			if (language == 0 || originalText == "")
			{
				return originalText;
			}
			
			if (_lineID == -1 || language <= 0)
			{
				ACDebug.Log ("Cannot find translation for '" + originalText + "' because the text has not been added to the Speech Manager.");
				return originalText;
			}
			else
			{
				SpeechLine speechLine;
				if (speechLinesDictionary.TryGetValue (_lineID, out speechLine))
				{
					if (speechLine.translationText.Count > (language-1))
					{
						return speechLine.translationText [language-1];
					}
					else
					{
						ACDebug.LogWarning ("A translation is being requested that does not exist!");
					}
				}
				else
				{
					ACDebug.LogWarning ("Cannot find translation for '" + originalText + "' because it's Line ID (" + _lineID + ") was not found in the Speech Manager.");
					return originalText;
				}
			}

			return "";
		}


		/**
		 * <summary>Gets all translations of a line of text.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <returns>All translations of the line, if they exist. If a translation does not exist, nothing will be returned.</returns>
		 */
		public string[] GetTranslations (int _lineID)
		{
			if (_lineID == -1)
			{
				return null;
			}
			else
			{
				SpeechLine speechLine;
				if (speechLinesDictionary.TryGetValue (_lineID, out speechLine))
				{
					return speechLine.translationText.ToArray ();
				}
			}
			return null;
		}


		/**
		 * <summary>Gets the custom AudioClip of a SpeechLine.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <param name = "_language">The ID number of the language</param>
		 * <returns>The custom AudioClip of the speech line</summary>
		 */
		public AudioClip GetLineCustomAudioClip (int _lineID, int _language = 0)
		{
			SpeechLine speechLine;
			if (speechLinesDictionary.TryGetValue (_lineID, out speechLine))
			{
				if (KickStarter.speechManager.translateAudio && _language > 0)
				{
					if (speechLine.customTranslationAudioClips != null && speechLine.customTranslationAudioClips.Count > (_language - 1))
					{
						return speechLine.customTranslationAudioClips [_language - 1];
					}
				}
				else
				{
					return speechLine.customAudioClip;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets the custom Object of a SpeechLine's lipsync.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <param name = "_language">The ID number of the language</param>
		 * <returns>The custom Object of the SpeechLine's lipsync</summary>
		 */
		public UnityEngine.Object GetLineCustomLipsyncFile (int _lineID, int _language = 0)
		{
			SpeechLine speechLine;
			if (speechLinesDictionary.TryGetValue (_lineID, out speechLine))
			{
				if (KickStarter.speechManager.translateAudio && _language > 0)
				{
					if (speechLine.customTranslationLipsyncFiles != null && speechLine.customTranslationLipsyncFiles.Count > (_language - 1))
					{
						return speechLine.customTranslationLipsyncFiles [_language - 1];
					}
				}
				else
				{
					return speechLine.customLipsyncFile;
				}
			}
			return null;
		}


		private void CreateLanguage (string name)
		{
			languages.Add (name);

			foreach (SpeechLine speechManagerLine in KickStarter.speechManager.lines)
			{
				int _lineID = speechManagerLine.lineID;

				SpeechLine speechLine = null;
				if (speechLinesDictionary.TryGetValue (_lineID, out speechLine))
				{
					speechLine.translationText.Add (speechLine.text);
					continue;
				}
			}
		}


		/**
		 * <summary>Imports a translation CSV file (as generated by the Speech Manager) into the game - either as a new language, or as an update to an existing one. The first column MUST be the ID number of each line, and the first row must be for the header.</summary>
		 * <param name = "textAsset">The CSV file as a text asset.</param>
		 * <param name = "languageName">The name of the language.  If a language by this name already exists in the system, the import process will update it.</param>
		 * <param name = "newTextColumn">The column number (starting from zero) that holds the new translation.  This must be greater than zero, as the first column should be occupied by the ID numbers.</param>
		 * <param name = "ignoreEmptyCells">If True, then empty cells will not be imported and the original language will be used instead</param>
		 */
		public void ImportRuntimeTranslation (TextAsset textAsset, string languageName, int newTextColumn, bool ignoreEmptyCells = false)
		{
			if (textAsset != null && !string.IsNullOrEmpty (textAsset.text))
			{
				if (newTextColumn <= 0)
				{
					ACDebug.LogWarning ("Error importing language from " + textAsset.name + " - newTextColumn must be greater than zero, as the first column is reserved for ID numbers.");
					return;
				}

				if (!languages.Contains (languageName))
				{
					CreateLanguage (languageName);
					int i = languages.Count - 1;
					ProcessTranslationFile (i, textAsset.text, newTextColumn, ignoreEmptyCells);
					ACDebug.Log ("Created new language " + languageName);
				}
				else
				{
					int i = languages.IndexOf (languageName);
					ProcessTranslationFile (i, textAsset.text, newTextColumn, ignoreEmptyCells);
					ACDebug.Log ("Updated language " + languageName);
				}
			}
		}
		
		
		private void ProcessTranslationFile (int i, string csvText, int newTextColumn, bool ignoreEmptyCells)
		{
			string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);
			
			int lineID = 0;
			string translationText = "";

			if (csvOutput.GetLength (0) <= newTextColumn)
			{
				ACDebug.LogWarning ("Cannot import translation file, as it does not have enough columns - searching for column index " + newTextColumn);
				return;
			}

			for (int y = 1; y < csvOutput.GetLength (1); y++)
			{
				if (csvOutput [0,y] != null && csvOutput [0,y].Length > 0)
				{
					lineID = -1;
					if (int.TryParse (csvOutput [0,y], out lineID))
					{
						translationText = csvOutput [newTextColumn, y];
						translationText = AddLineBreaks (translationText);

						if (!ignoreEmptyCells || !string.IsNullOrEmpty (translationText))
						{
							UpdateTranslation (i, lineID, translationText);
						}
					}
					else
					{
						ACDebug.LogWarning ("Error importing translation (ID:" + csvOutput [0,y] + ") - make sure that the CSV file is delimited by a '" + CSVReader.csvDelimiter + "' character.");
					}
				}
			}
		}


		private string AddLineBreaks (string text)
		{
			if (!string.IsNullOrEmpty (text))
			{
				return (text.Replace ("[break]", "\n"));
			}
			return "";
		}


		private void UpdateTranslation (int i, int _lineID, string translationText)
		{
			SpeechLine speechLine;
			if (speechLinesDictionary.TryGetValue (_lineID, out speechLine))
			{
				speechLine.translationText [i-1] = translationText;
			}
		}

	}

}