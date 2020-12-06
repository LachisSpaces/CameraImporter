using System.Windows;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;

namespace CameraImporter
{
   /// <summary>
   /// Interaktionslogik für MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      const int _PropertyTagDateTime = 0x0132;

      public enum MakeBackupResult { OK, ErrorFileExists, ErrorOther };
      public enum FileType { Unknown, Photo, Photo2D, Photo3D, Movie, MovieFuji, MoviePanasonic };

      static string _strApplicationPath = null;

      string _strSDCardSorting = "";
      FileInfo[] _fiSDCardFiles;


      public MainWindow()
      {
         _strApplicationPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
         InitializeComponent();
      }


      #region SDCard-Panel

      private void cmdSDCardPath_Select_Click(object sender, RoutedEventArgs e)
      {
         string strPath = "";
         if (this.SelectPath(out strPath))
         {
            this.txtSDCardPath.Text = strPath;
            DirectoryInfo dir = new DirectoryInfo(strPath);
            _fiSDCardFiles = dir.GetFiles("*.*", SearchOption.AllDirectories);
            this.lvSDCard.ItemsSource = _fiSDCardFiles;
         }
      }

      private void SDCardNameHeader_Click(object sender, RoutedEventArgs e)
      {
         this.SortSDCardFiles("Name");
      }

      private void SDCardExtensionHeader_Click(object sender, RoutedEventArgs e)
      {
         this.SortSDCardFiles("Extension");
      }

      private void SDCardLastWriteTimeHeader_Click(object sender, RoutedEventArgs e)
      {
         this.SortSDCardFiles("LastWriteTime");
      }

      private void SDCardCreationTimeHeader_Click(object sender, RoutedEventArgs e)
      {
         this.SortSDCardFiles("CreationTime");
      }

      private void SortSDCardFiles(string strSorting)
      {
         this.lvSDCard.ItemsSource = null;
         if (strSorting == _strSDCardSorting)
            strSorting += "Desc";
         switch (strSorting)
         {
            case "Name":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f1.Name.CompareTo(f2.Name); });
               break;
            case "NameDesc":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f2.Name.CompareTo(f1.Name); });
               break;
            case "Extension":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f1.Extension.CompareTo(f2.Extension); });
               break;
            case "ExtensionDesc":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f2.Extension.CompareTo(f1.Extension); });
               break;
            case "LastWriteTime":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f1.LastWriteTime.CompareTo(f2.LastWriteTime); });
               break;
            case "LastWriteTimeDesc":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f2.LastWriteTime.CompareTo(f1.LastWriteTime); });
               break;
            case "CreationTime":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f1.CreationTime.CompareTo(f2.CreationTime); });
               break;
            case "CreationTimeDesc":
               Array.Sort(_fiSDCardFiles, delegate(FileInfo f1, FileInfo f2)
               { return f2.CreationTime.CompareTo(f1.CreationTime); });
               break;
         }
         this.lvSDCard.ItemsSource = _fiSDCardFiles;
         _strSDCardSorting = strSorting;
      }

      #endregion


      #region Backup-Panel

      private void cmdBackupPath_Select_Click(object sender, RoutedEventArgs e)
      {
         string strPath = "";
         if (this.SelectPath(out strPath))
         {
            this.txtBackupPath.Text = strPath;
            this.ckbBackup_Save.IsChecked = true;
         }
         this.txtBackupPath.Text = strPath;
      }

      private void txtBackupNewFolder_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         this.ckbBackupNewFolder.IsChecked = !string.IsNullOrEmpty(this.txtBackupNewFolder.Text);
      }

      #endregion


      #region Andere Panels

      private void cmdPhotos2DPath_Select_Click(object sender, RoutedEventArgs e)
      {
         string strPath = "";
         if (this.SelectPath(out strPath))
         {
            this.txtPhotos2DPath.Text = strPath;
            this.ckbPhotos2D_Save.IsChecked = true;
         }
      }

      private void cmdPhotos3DPath_Select_Click(object sender, RoutedEventArgs e)
      {
         string strPath = "";
         if (this.SelectPath(out strPath))
         {
            this.txtPhotos3DPath.Text = strPath;
            this.ckbPhotos3D_Save.IsChecked = true;
         }
      }

      private void cmdMoviesPath_Select_Click(object sender, RoutedEventArgs e)
      {
         string strPath = "";
         if (this.SelectPath(out strPath))
         {
            this.txtMoviesPath.Text = strPath;
            this.ckbMovies_Save.IsChecked = true;
         }
      }

      private void txtFileNameDescription_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         this.ckbFileNameDescription.IsChecked = !string.IsNullOrEmpty(this.txtFileNameDescription.Text);
      }

      #endregion


      #region Import-Funktionen

      private void cmdTransferData_Click(object sender, RoutedEventArgs e)
      {
         bool blnMakeBackup = (this.ckbBackup_Save.IsChecked == true) && !string.IsNullOrEmpty(this.txtBackupPath.Text);
         bool blnImportMovies = (this.ckbMovies_Save.IsChecked == true) && !string.IsNullOrEmpty(this.txtMoviesPath.Text);
         bool blnImportPhotos2D = (this.ckbPhotos2D_Save.IsChecked == true) && !string.IsNullOrEmpty(this.txtPhotos2DPath.Text);
         bool blnImportPhotos3D = (this.ckbPhotos3D_Save.IsChecked == true) && !string.IsNullOrEmpty(this.txtPhotos3DPath.Text);
         
         string strDescription = null;
         if ((this.ckbFileNameDescription.IsChecked == true) && !string.IsNullOrEmpty(this.txtFileNameDescription.Text))
            strDescription = '_' + this.txtFileNameDescription.Text;

         // Backup erstellen, falls gewünscht
         if (blnMakeBackup)
         {
            bool blnNewFolder = false;
            string strDestination = this.txtBackupPath.Text;
            if (this.ckbBackupNewFolder.IsChecked == true && !string.IsNullOrEmpty(this.txtBackupNewFolder.Text))
            {
               strDestination += this.txtBackupNewFolder.Text;
               blnNewFolder = true;
            }
            if (!strDestination.EndsWith("\\"))
               strDestination += "\\";
            // Folder anlegen, falls gewünscht
            if (blnNewFolder)
            {
               DirectoryInfo di = new DirectoryInfo(strDestination);
               if (!di.Exists)
                  di.Create();
               else if (MessageBoxResult.Yes != MessageBox.Show("The defined backup folder exists already, \r\ndo you really want to use this folder?", "Make backup", MessageBoxButton.YesNo))
                  return;
            }
            // Alle Dateien inkl. Ordner kopieren
            switch (this.MakeBackupFromSDCard(this.txtSDCardPath.Text, strDestination))
            {
               case MakeBackupResult.ErrorFileExists:
                  if (MessageBoxResult.Yes != MessageBox.Show("Unable to backup the files because one or more files exist already. \r\nDo you want to continue with the next step?", "Make backup", MessageBoxButton.YesNo))
                     return;
                  break;
               case MakeBackupResult.ErrorOther:
                  if (MessageBoxResult.Yes != MessageBox.Show("An error occured while making the backup,\r\nplease check the error log. \r\nDo you want to continue with the next step?", "Make backup", MessageBoxButton.YesNo))
                     return;
                  break;
            }
         }

         // Die Dateien in die entsprechenden Ordner kopieren, falls gewünscht
         foreach (FileInfo fiFile in _fiSDCardFiles)
         {
            DateTime dtmDate;
            string strExt = fiFile.Extension.ToLower();
            // Der User kann wählen, welches Datum verwendet werden soll
            if (this.rbFileNameLastWriteTime.IsChecked == true)
               dtmDate = fiFile.LastWriteTime;
            else 
               dtmDate = fiFile.CreationTime;
            // Der Kopiervorgang (Falls die FileTypes nicht ausschliessend wären, musste es anders programmiert werden)
            switch (this.GetFileType(strExt, false))
            {
               case FileType.Photo2D:
                  if (blnImportPhotos2D)
                     fiFile.CopyTo(this.MakeFilename(this.txtPhotos2DPath.Text, dtmDate, strDescription, strExt), false);
                  break;
               case FileType.Photo3D:
                  if (blnImportPhotos3D)
                     fiFile.CopyTo(this.MakeFilename(this.txtPhotos3DPath.Text, dtmDate, strDescription, strExt), false);
                  break;
               case FileType.MovieFuji:
               case FileType.MoviePanasonic:
                  if (blnImportMovies)
                     fiFile.CopyTo(this.MakeFilename(this.txtMoviesPath.Text, dtmDate, strDescription, strExt), false);
                  break;
            }
         }

         // Hinweis, dass alles korrekt abgelaufen ist
         MessageBox.Show("Data transfer completed successfully.");
      }

      /// <summary>
      /// Erstellt das Backup anhand von Source und Destionation. 
      /// Gibt OK oder einen Fehler gemäss MakeBackupResult zurück.
      /// </summary>
      private MakeBackupResult MakeBackupFromSDCard(string strSDCard, string strDestinationFolder)
      {
         // Alle Ordner anlegen, die auf der SD-Karte sind
         foreach (string strSubdir in Directory.GetDirectories(strSDCard, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(strDestinationFolder + strSubdir.Substring(strSDCard.Length));

         // Alle Dateien kopieren und Datum bei Fotos anpassen
         try
         {
            foreach (string strFile in Directory.GetFiles(strSDCard, "*.*", SearchOption.AllDirectories))
            {
               bool blnSetDates = false;
               DateTime dtmCreationDate = new DateTime(0);
               string strDestinationPath = strDestinationFolder + strFile.Substring(strSDCard.Length);
               //Datei kopieren
               File.Copy(strFile, strDestinationPath , false);
               // Bei einem Foto noch das Datum der Erzeugung ermitteln und das Backup entsprechend anpassen, sonst CreationTime verwenden
               switch (this.GetFileType(Path.GetExtension(strFile).ToLower(), true))
               {
                  case FileType.Photo:
                     blnSetDates = this.JPG_GetTagValueAsDateTime(strFile, _PropertyTagDateTime, out dtmCreationDate);
                     break;
                  case FileType.Movie:
                     FileInfo fi = new FileInfo(strFile);
                     dtmCreationDate = fi.CreationTime;
                     blnSetDates = true;
                     break;
               }
               if (blnSetDates)
               {
                  FileInfo fi = new FileInfo(strDestinationPath);
                  fi.LastWriteTime = dtmCreationDate;
                  fi.CreationTime = dtmCreationDate;
               }
            }
         }
         catch (IOException) { return MakeBackupResult.ErrorFileExists; }
         catch (Exception e) 
         { 
            LogException(e);
            return MakeBackupResult.ErrorOther;
         }
         //
         return MakeBackupResult.OK;
      }

      /// <summary>
      /// Gibt den spezifischen FileType der Datei anhand der Dateierweiterung zurück.
      /// Die Erweiterung muss klein geschrieben sein. 
      /// Rückgabe nur die Klasse (Photo, Movie) oder der detailierte Type.
      /// </summary>
      private FileType GetFileType(string strExtension, bool blnClassOnly)
      {
         if (blnClassOnly)
            switch (strExtension)
            {
               case ".jpg":
               case ".mpo":
                  return FileType.Photo;
               case ".avi":
               case ".mod":
                  return FileType.Movie;
            }
         else
            switch (strExtension)
            {
               case ".jpg":
                  return FileType.Photo2D;
               case ".mpo":
                  return FileType.Photo3D;
               case ".avi":
                  return FileType.MovieFuji;
               case ".mod":
                  return FileType.MoviePanasonic;
            }
         return FileType.Unknown;
      }

      /// <summary>
      /// Gibt den Wert einer Tag-Eigenschaft eines JPG-Bildes als DateTime zurück
      /// </summary>
      private bool JPG_GetTagValueAsDateTime(string strFile, int intItemType, out DateTime dtmDate)
      {
         string strTag = null;
         Bitmap bmFoto = new Bitmap(strFile);
         DateTime dtmReturn = new DateTime(0);
         if (this.JPG_GetTagValueAsString(bmFoto, intItemType, out strTag))
         {
            // Versuch, den im Format yyyy:MM:dd hh:mm:ss ermittelten String in ein Datum zu konvertieren
            if (strTag != "0000:00:00 00:00:00")
            {
               try
               {
                  int intYear = Convert.ToInt32(strTag.Substring(0, 4));
                  int intMonth = Convert.ToInt32(strTag.Substring(5, 2));
                  int intDay = Convert.ToInt32(strTag.Substring(8, 2));
                  int intHour = Convert.ToInt32(strTag.Substring(11, 2));
                  int intMinute = Convert.ToInt32(strTag.Substring(14, 2));
                  int intSecond = Convert.ToInt32(strTag.Substring(17, 2));
                  dtmReturn = new DateTime(intYear, intMonth, intDay, intHour, intMinute, intSecond);
               }
               catch (Exception e) { LogException(e); }
            }
         }
         bmFoto.Dispose();
         dtmDate = dtmReturn;
         return false;
      }

      /// <summary>
      /// Gibt den Wert einer Tag-Eigenschaft eines JPG-Bildes als String zurück
      /// </summary>
      private bool JPG_GetTagValueAsString(Bitmap bitmap, int intItemType, out string strTag)
      {
         string strResult = null;
         for (int i = 0; i < bitmap.PropertyItems.Length; i++)
         {
            PropertyItem item = bitmap.PropertyItems[i];
            if (item.Id == intItemType)
            {
               for (int j = 0; j < item.Len - 1; j++)
                  strResult += (char)item.Value[j];
               break;
            }
         }
         strTag = strResult;
         return !string.IsNullOrEmpty(strResult);
      }

      /// <summary>
      /// Gibt einen eindeutigen Dateinamen anhand des Datums und ev. der Beschreibung zurück. 
      /// Der Dateiname ist eindeutig innerhalb des definierten Pfades.
      /// strDescription muss den Trenner nach vorne beinhalten. 
      /// </summary>
      private string MakeFilename(string strPath, DateTime dtmDate, string strDescription, string strExt)
      {
         int intFileNo = 0;
         string strFile = "";
         do
         {
            intFileNo += 1;
            strFile = strPath + dtmDate.ToString("yyyy-MM-dd_HH-mm") + strDescription + '_' + intFileNo.ToString("00") + strExt;
         }
         while (File.Exists(strFile));
         return strFile;
      }

      #endregion


      /// <summary>
      /// Wrapper für OpenFileDialog, mit dem man auch einen Ordern auswählen kann. 
      /// Gibt TRUE zurück, falls der User einen sinnvollen Pfad ausgewählt hat.
      /// </summary>
      private bool SelectPath(out string strPathOut)
      {
         string strDummy = "Select folder or file", strPath = "";
         OpenFileDialog dlg = new OpenFileDialog();
         dlg.CheckFileExists = false;
         dlg.CheckPathExists = true;
         dlg.ValidateNames = false;
         dlg.FileName = strDummy;
         //dlg.InitialDirectory = _strInitialDirectoryFolder;
         if ((bool)dlg.ShowDialog())
            strPath = dlg.FileName;
         if (!string.IsNullOrEmpty(strPath))
         {
            if (strPath.Contains(strDummy))
               strPath = strPath.Replace(strDummy, "");
            else
               strPath = Path.GetDirectoryName(strPath);
            if (!new DirectoryInfo(strPath).Exists)
               strPath = null;
         }
         strPathOut = strPath;
         return !string.IsNullOrEmpty(strPath);
      }


      private static void LogException(Exception e)
      {
         string strLastExceptionMessage = e.Message;
         string strExceptionStackTrace = e.StackTrace;
         Exception eInnerException = e.InnerException;
         System.Text.StringBuilder msg = new System.Text.StringBuilder("----An error occured----\r\n");
         msg.Append(e.Message);
         while (eInnerException != null)
         {
            if (strLastExceptionMessage != eInnerException.Message)
            {
               strLastExceptionMessage = eInnerException.Message;
               msg.AppendFormat("\r\n\r\n----Inner error----\r\n{0}", strLastExceptionMessage);
            }
            strExceptionStackTrace = eInnerException.StackTrace;
            eInnerException = eInnerException.InnerException;
         }
         msg.AppendFormat("\r\n\r\n----Stacktrace----\r\n{0}", strExceptionStackTrace);
         StreamWriter sw = new StreamWriter(string.Format("{0}{1}_Error.txt", _strApplicationPath, System.DateTime.Now.ToString("yyyyMMdd_HHhmmss")));
         sw.Write(msg.ToString());
         sw.Close();
         sw.Dispose();
      }

   }
}
