using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoDateSetter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PropertyTagDateTime = 0x0132;
        const int PropertyTagDateTimeOriginal = 0x9003;
        const int PropertyTagDateTimeDigitized = 0x9004;
        const int PropertyTagTypeASCII = 2;

        private string filePath;
        private Bitmap bitmap;
        private PropertyItem dateTimeProperty;
        private PropertyItem dateTimeOriginalProperty;
        private PropertyItem dateTimeDigitizedProperty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSetDate_Click(object sender, RoutedEventArgs e)
        {
            if (this.filePath != null && this.datePicker.SelectedDate.HasValue)
            {
                var date = this.datePicker.SelectedDate.Value;
                Regex timeRegex = new Regex(@"(\d\d):(\d\d):(\d\d)");
                Match match = timeRegex.Match(this.txtTime.Text);
                if (!match.Success) return;
                DateTime newDateTime = new DateTime(date.Year, date.Month, date.Day,
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));

                SetDateTime(this.bitmap, PropertyTagDateTime, newDateTime);
                SetDateTime(this.bitmap, PropertyTagDateTimeDigitized, newDateTime);
                SetDateTime(this.bitmap, PropertyTagDateTimeOriginal, newDateTime);

                string newFileName = Path.GetFileNameWithoutExtension(this.filePath) +
                    Guid.NewGuid().ToString() +
                    Path.GetExtension(this.filePath);
                newFileName = Path.Combine(
                    Path.GetDirectoryName(this.filePath), 
                    newFileName);
                this.bitmap.Save(newFileName);
            }
        }

        private void btnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.jpg;*.png|All files|*.*";
            if (dlg.ShowDialog() == true)
            {
                string dateTime = "", original = "", digitized = "";
                this.dateTimeDigitizedProperty = this.dateTimeOriginalProperty = this.dateTimeProperty = null;
                this.filePath = dlg.FileName;
                this.bitmap = (Bitmap)Bitmap.FromFile(this.filePath);
                this.imgPhoto.Source = ConvertBitmapToBitmapSource(bitmap);
                foreach (var propertyItem in bitmap.PropertyItems)
                {
                    switch (propertyItem.Id)
                    {
                        case PropertyTagDateTime:
                            this.dateTimeProperty = propertyItem;
                            dateTime = this.GetDateTime(propertyItem);
                            break;
                        case PropertyTagDateTimeOriginal:
                            this.dateTimeOriginalProperty = propertyItem;
                            original = this.GetDateTime(propertyItem);
                            break;
                        case PropertyTagDateTimeDigitized:
                            this.dateTimeDigitizedProperty = propertyItem;
                            digitized = this.GetDateTime(propertyItem);
                            break;
                    }
                }

                if (dateTime != null)
                {
                    this.lblCurrentDateTime.Content = "DateTime: " + dateTime;
                }

                if (original != null)
                {
                    this.lblCurrentDateTimeOriginal.Content = "Original: " + original;
                }

                if (digitized != null)
                {
                    this.lblCurrentDateTimeDigitized.Content = "Digitized: " + digitized;
                }
            }
        }

        private string GetDateTime(PropertyItem propItem)
        {
            string dateTimeStr = Encoding.ASCII.GetString(propItem.Value, 0, 19);
            DateTime dateTime = new DateTime(
                int.Parse(dateTimeStr.Substring(0, 4)),
                int.Parse(dateTimeStr.Substring(5, 2)),
                int.Parse(dateTimeStr.Substring(8, 2)),
                int.Parse(dateTimeStr.Substring(11, 2)),
                int.Parse(dateTimeStr.Substring(14, 2)),
                int.Parse(dateTimeStr.Substring(17, 2)));
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void SetDateTime(Bitmap bitmap, int propertyItemId, DateTime newDate)
        {
            PropertyItem propItem;
            if (bitmap.PropertyIdList.Contains(propertyItemId))
            {
                propItem = bitmap.GetPropertyItem(propertyItemId);
                bitmap.RemovePropertyItem(propertyItemId);
            }
            else
            {
                propItem = bitmap.PropertyItems[0];
                propItem.Id = propertyItemId;
                propItem.Type = PropertyTagTypeASCII;
            }
            string newDateStr = newDate.ToString("yyyy:MM:dd HH:mm:ss\0");
            propItem.Value = Encoding.ASCII.GetBytes(newDateStr);
            bitmap.SetPropertyItem(propItem);
        }

        private ImageSource ConvertBitmapToBitmapSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
