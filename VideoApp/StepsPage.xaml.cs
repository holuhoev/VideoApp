using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Imaging;
using EncodeImages;
using System.Threading.Tasks;
using VideoApp.Classes;

// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556

namespace VideoApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class StepsPage : Page
    {
        PictureWriter m_picture;
        double _durationSec=2;
        string _videoName="";
        bool _isDateNeeded=false;
        private const int m_videoWidth = 640;
        private const int m_videoHeight = 480;
        private List<StorageFile> m_files = new List<StorageFile>();
        CoreApplicationView view = CoreApplication.GetCurrentView();
        public StepsPage()
        {
            this.InitializeComponent();
            
            statusText.Text = "";
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        { 
            if (e.Parameter!=null && e.Parameter is List<StorageFile>)
            {             
               m_files = e.Parameter as List<StorageFile>;                
            }
        }

        private void PickPhotoBtn_Click(object sender, RoutedEventArgs e)
        {         
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.PickMultipleFilesAndContinue();
            view.Activated += ContinueFileOpen;
        }
        private void ContinueFileOpen(CoreApplicationView sender, IActivatedEventArgs args1)
        {
            FileOpenPickerContinuationEventArgs args = args1 as FileOpenPickerContinuationEventArgs;

            if (args != null)
            {
                IReadOnlyList<StorageFile> files = args.Files;
                if (files.Count > 0)
                { 
                    
                    m_files = files.ToList();
                    statusText.Text = "Фотографии выбраны! ("+m_files.Count+")";
                }
                view.Activated -= ContinueFileOpen;              
            }            
        }      

        private void Encode_Click(object sender, RoutedEventArgs e)
        {
            if (m_files != null && m_files.Count > 0 && _videoName!="")
            {
                CreateAndSaveVideo();
            }
            else
            {
                statusText.Text = "Выберете фотографии и выполните настройку видеоролика";    
            }
        }
        
        private async void CreateAndSaveVideo()
        {
            if (m_files != null && m_files.Count > 0)
            {
                
                if (_isDateNeeded) SortPhoto();
                StorageFolder myVideoLibrary;
                //если папка существует открываем ,если нет создаем
                try
                {
                     myVideoLibrary = await KnownFolders.PicturesLibrary.GetFolderAsync("ImageConvertor");
                }
                catch
                {
                    myVideoLibrary = await KnownFolders.PicturesLibrary.CreateFolderAsync("ImageConvertor");
                }

                StorageFile videoFile = await myVideoLibrary.CreateFileAsync(_videoName+".mp4", CreationCollisionOption.GenerateUniqueName);               
                if (videoFile != null)
                {
                    PickPhotoBtn.IsEnabled = false;
                    Encode.IsEnabled = false;
                    dateCheckBox.IsChecked = false;
                    durationList.IsEnabled = false;
                    nameBox.IsEnabled = false;
                    BackBar.IsEnabled = false;
                    progressRing.IsActive = true;
                    titleBlock.Text = "Конвертирование . . .";
                    try
                    {
                        using (IRandomAccessStream videoStream = await videoFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            m_picture = new PictureWriter(videoStream, m_videoWidth, m_videoHeight);
                            foreach (StorageFile file in m_files)
                            {
                                Windows.Storage.FileProperties.ImageProperties properties = await file.Properties.GetImagePropertiesAsync();
                                var longitude = properties.Longitude;
                                float scaleOfWidth = (float)m_videoWidth / properties.Width;
                                float scaleOfHeight = (float)m_videoHeight / properties.Height;
                                float scale = scaleOfHeight > scaleOfWidth ?
                                scaleOfWidth : scaleOfHeight;
                                uint width = (uint)(properties.Width * scale);
                                uint height = (uint)(properties.Height * scale);

                                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                                {
                                    for (int i = 0; i < 10 * _durationSec; ++i)
                                    {
                                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                                        PixelDataProvider dataProvider = await decoder.GetPixelDataAsync(
                                            BitmapPixelFormat.Bgra8,
                                            BitmapAlphaMode.Straight,
                                            new BitmapTransform { ScaledWidth = width, ScaledHeight = height },
                                            ExifOrientationMode.RespectExifOrientation,
                                            ColorManagementMode.ColorManageToSRgb);
                                        m_picture.AddFrame(dataProvider.DetachPixelData(), (int)width, (int)height);
                                    }
                                }
                            }
                            m_picture.Finalize();
                            m_picture = null;

                        }
                    }
                    catch (Exception ex)
                    {
                        CreateAndSaveVideo();
                    }
                    progressRing.IsActive = false;

                    await CreateVideoModelAndWrite(videoFile); //сохраняем данные о файле в локальную папку
                    
                    Frame.Navigate(typeof(MainPage));
                    
                }
            }
        }
        private void SortPhoto()
        {
            if (m_files != null && m_files.Count > 0)
            {
                int count = m_files.Count;
                ImageModel[] photoModels = new ImageModel[count];
                for (int i = 0; i < count; i++)
                {
                    string[] date = m_files[i].DateCreated.ToString().Split(' ');
                    photoModels[i] = new ImageModel(m_files[i], date[0] + " " + date[1]);
                }
                Array.Sort(photoModels);
                m_files = null;
                for(int i = 0; i < count; i++)
                {
                    m_files[i] = photoModels[i].File;
                }
            }
        }
        private async Task CreateVideoModelAndWrite(StorageFile file)
        {
            byte[] imageBytes = await ImageByteConvertor.ImageToByteArray(m_files[0]);
            string[] date = file.DateCreated.ToString().Split(' ');            
            VideoModel myvideo = new VideoModel() { DateCreated= date[0] + " " + date[1], Name = file.DisplayName, HeadPiece = imageBytes };
            await WriteReadVideoModel.WriteVideoModelToFile(myvideo);
        }

        private void BackBar_Click(object sender, RoutedEventArgs e)
        {
            m_files = null;
            statusText.Text = "";
            Frame.Navigate(typeof(MainPage));
        }

      

        private void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _videoName = nameBox.Text;
        }

        private void durationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock value =(TextBlock)durationList.SelectedItem;
            _durationSec = double.Parse(value.Text);
        }
    }
}
