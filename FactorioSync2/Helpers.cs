using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

using NLog;

namespace FactorioSync2
{
    public class Helpers
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] _args = Environment.GetCommandLineArgs();

        private enum ErrorLvl
        {
            Fatal,
            Error,
            Warn,
            Info,
            Debug,
            Trace
        }

        private static void LogString(string text, ListBox lstBox = null, ErrorLvl error = ErrorLvl.Debug, string color = "")
        {
            if ((lstBox != null) && !((_args.Length > 1) && _args[1] == "/silent"))
            {
                // On log sur l'interface
                var defaultBrushColor = Brushes.Blue;
                var successBrushColor = Brushes.Green;
                var errorBrushColor = Brushes.Red;
                var warningBrushColor = Brushes.Orange;

                var currentBrushe = defaultBrushColor;

                if (color == "success")
                {
                    currentBrushe = successBrushColor;
                }
                else if (color == "error")
                {
                    currentBrushe = errorBrushColor;
                }
                else if (color == "warning")
                {
                    currentBrushe = warningBrushColor;
                }

                lstBox.Items.Add(new ListBoxItem
                {
                    Content = String.Format(text),
                    Foreground = currentBrushe
                });
            }
            else
            {
                // Sinon on log dans le fichier de Log
                if (error == ErrorLvl.Debug)
                {
                    _logger.Debug(text);
                }
                else if (error == ErrorLvl.Error)
                {
                    _logger.Error(text);
                }
                else
                {
                    _logger.Info(text);
                }

            }
        }

        public static void CopyFile(string srcFolder, string destFolder, ListBox lstBox = null)
        {
            string[] originalFiles = Directory.GetFiles(srcFolder, "*", SearchOption.AllDirectories);

            LogString($"Début : {DateTime.Now}", lstBox);
            LogString($"Il y a {originalFiles.Length} fichiers dans {srcFolder}", lstBox);

            Array.ForEach(originalFiles, (originalFileLocation) =>
            {
                try
                {
                    FileInfo originalFile = new FileInfo(originalFileLocation);
                    FileInfo destFile = new FileInfo(originalFileLocation.Replace(srcFolder, destFolder));

                    LogString($"Vérification du fichier {originalFile} vers le répertoire {destFolder}", lstBox);

                    if (destFile.Exists)
                    {
                        LogString($"Le fichier {destFile} existe déja !", lstBox);
                        if (originalFile.LastWriteTimeUtc > destFile.LastWriteTimeUtc)
                        {
                            LogString($"Le fichier d'origine \"{originalFile}\" >{originalFile.LastWriteTimeUtc}< est plus récent que le fichier de destination \"{destFile}\" >{destFile.LastWriteTimeUtc}< : On l'écrase", lstBox, ErrorLvl.Info, "succes");
                            try
                            {
                                originalFile.CopyTo(destFile.FullName, true);
                                LogString($"Copie effectuée : \"{originalFile}\" -> \"{destFile}\" !", lstBox, ErrorLvl.Info, "succes");
                            }
                            catch (Exception e)
                            {
                                LogString($"Erreur lors de la copie du fichier {originalFile} : {e.Message}", lstBox, ErrorLvl.Error, "error");
                            }
                        }
                        else
                        {
                            LogString($"Les fichiers sont identiques", lstBox, ErrorLvl.Info, "succes");
                        }
                    }
                    else
                    {
                        LogString($"Création/Sélection du répertoire \"{destFile.DirectoryName}\"", lstBox, ErrorLvl.Debug, "warning");
                        Directory.CreateDirectory(destFile.DirectoryName);
                        try
                        {
                            LogString($"Copie du fichier {destFile.FullName}", lstBox, ErrorLvl.Debug, "success");
                            originalFile.CopyTo(destFile.FullName, false);
                        }
                        catch (Exception ex)
                        {
                            LogString($"Erreur lors de la copie du fichier {destFile.FullName} : {ex.Message}", lstBox, ErrorLvl.Error, "error");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogString(e.Message, lstBox, ErrorLvl.Error, "error");
                }
            });

            LogString($"Fin : {DateTime.Now}", lstBox);
            LogString("*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*", lstBox);
        }
    }
}
