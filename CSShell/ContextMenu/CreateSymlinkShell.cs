using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CreateSymlinkShell
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.DirectoryBackground)]
    public class CreateSymlinkShell : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            if (SelectedItemPaths.Count() > 0) {
                return true;
            } else {
                return false;
            }
        }

        protected override ContextMenuStrip CreateMenu()
        {
            // メニューを生成して項目を追加する
            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem
            {
                Text = "シンボリックリンクを作成"
            };
            item.Click += (sender, args) => CountLines();
            menu.Items.Add(item);
            // メニューを返す
            return menu;
        }

        public string GetDllDirectory()
        {
            // 現在実行中のアセンブリのパスを取得
            string dllPath = Assembly.GetExecutingAssembly().Location;

            // アセンブリのディレクトリを取得
            string dllDirectory = Path.GetDirectoryName(dllPath);

            return dllDirectory;
        }

        private void CountLines()
        {
            //var builder = new StringBuilder();
            var filePaths = new StringBuilder();

            foreach (var path in SelectedItemPaths)
            {
                //builder.AppendLine(string.Format("FilePath: {0}",
                //    System.IO.Path.GetFullPath(path)));

                // 引数用のファイルパスを追加（ダブルクオートで囲む）
                filePaths.Append($"\"{path}\" ");
            }
            //// add target path
            //builder.AppendLine($"Target: {FolderPath}");

            //// メッセージボックスで表示する
            //MessageBox.Show(builder.ToString());

            string exePath = $"{GetDllDirectory()}\\CreateSymlinkCSharp.exe";  // 実行するexeのパス
            string arguments = $"{filePaths.ToString().Trim()} \"{FolderPath}\"";  // 引数文字列を構築

            try
            {
                // プロセスの起動
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,    // コンソールアプリの場合はfalse
                    CreateNoWindow = true      // コンソールウィンドウを非表示
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"プロセスの起動に失敗しました: {ex.Message}");
            }
        }

    }
}
