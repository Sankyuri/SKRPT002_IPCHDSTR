using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace IpchDestroyer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title            = TextResources.item_windowtitle;
            itemTBLTarget.Text    = TextResources.item_tbltarget;
            itemBTNSubmit.Content = TextResources.item_btndestroy;
        }




        public bool IsDirectory( in string a_path )
        {
            if (a_path.Equals( "" )) {
                return false;
            }
            return new FileInfo( a_path )
                    .Attributes.HasFlag( FileAttributes.Directory );
        }




        public bool DestroyIpchs()
        {
            try {
                DestroyIpch( itemTBXDirectory.Text );

                _ = MessageBox.Show(
                        TextResources.info_destroy_done,
                        TextResources.info_common_destroy,
                        MessageBoxButton.OK, MessageBoxImage.Information );
                return true;

            } catch (Exception e) {
                _ = MessageBox.Show(
                        TextResources.info_destroy_fail + e.Message,
                        TextResources.info_common_destroy,
                        MessageBoxButton.OK, MessageBoxImage.Error );
                return false;
            }
        }




        public void DestroyIpch( in string a_directory )
        {
            IReadOnlyCollection<string> dirs
                    = FileSystem.GetDirectories(
                        a_directory,
                        Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly );
            foreach (string dir in dirs) {
                // フォルダなら処理
                if (IsDirectory( dir )) {
                    // ipch フォルダなら削除
                    if (dir.Contains( @"\ipch" )) {
                        FileSystem.DeleteDirectory(
                                dir,
                                UIOption.OnlyErrorDialogs,
                                RecycleOption.SendToRecycleBin );
                    } else {
                        // それ以外のフォルダなので中を探索
                        DestroyIpch( dir );
                    }
                } // フォルダでないなら何もしない
            }
        }








        // itemTBXDirectory に対するドラッグ
        private void itemTBXDirectory_PreviewDragOver( object sender, DragEventArgs e )
        {
            if (e.Data.GetDataPresent( DataFormats.FileDrop )) {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }




        // ウィンドウに対するドロップ
        private void Window_Drop( object sender, DragEventArgs e )
        {
            // ファイルのドラッグアンドドロップでないなら何もしない
            if ( ! e.Data.GetDataPresent( DataFormats.FileDrop )) {
                return;
            }

            string path = ( e.Data.GetData( DataFormats.FileDrop, false ) as string[] )[0];
            // フォルダか確認
            if (IsDirectory( path )) {
                itemTBXDirectory.Text = path;
            } else {
                _ = MessageBox.Show(
                        TextResources.error_open_notfolder.Replace( "{0}", path ),
                        TextResources.error_common_error,
                        MessageBoxButton.OK, MessageBoxImage.Error );
            }
        }




        // 参照ボタンをクリックした時の処理
        private void BTNRefer_Click( object sender, RoutedEventArgs e )
        {
            CommonOpenFileDialog ofd = new(){
                Title            = TextResources.text_open_pickfolder,
                IsFolderPicker   = true,
            };
            if (CommonFileDialogResult.Ok == ofd.ShowDialog()) {
                // ディレクトリではない
                if ( ! IsDirectory( ofd.FileName )) {
                    _ = MessageBox.Show(
                            TextResources.error_common_plzfolder,
                            TextResources.error_common_error,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                itemTBXDirectory.Text = ofd.FileName;
            }
        }




        //
        private void itemBTNSubmit_Click( object sender, RoutedEventArgs e )
        {
            // フォルダではない
            if ( ! IsDirectory( itemTBXDirectory.Text )) {
                _ = MessageBox.Show(
                        TextResources.error_common_plzfolder,
                        TextResources.error_common_error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBoxResult.OK
                    == MessageBox.Show(
                        TextResources.info_execute_check
                            .Replace( "{0}", itemTBXDirectory.Text ),
                        TextResources.info_common_destroy,
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning ))
            {
                // ファイル駆逐
                DestroyIpchs();
            }
        }
    }
}
