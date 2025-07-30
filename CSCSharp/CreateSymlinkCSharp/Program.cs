using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Media;

namespace SymlinkCreator
{
    public static class CustomMessageBox
    {
        public static DialogResult Show(
            string text,
            string caption = "メッセージ",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            int padding = 20)
        {
            using (var form = new Form())
            {
                Screen? primaryScreen = Screen.PrimaryScreen ?? throw new NotSupportedException("Primary screen is not found");
                int screenWidth = primaryScreen.WorkingArea.Width;
                int screenHeight = primaryScreen.WorkingArea.Height;

                form.Text = caption;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowInTaskbar = false;
                form.AutoSize = true; // フォームのサイズを自動調整
                form.AutoSizeMode = AutoSizeMode.GrowAndShrink; // フォームのサイズを自動調整

                var mainPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Padding = new Padding(padding),
                    BackColor = Color.White,
                    AutoSize = true, // パネルのサイズを自動調整
                    AutoSizeMode = AutoSizeMode.GrowAndShrink // パネルのサイズを自動調整
                };
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                form.Controls.Add(mainPanel);

                // アイコンを追加（オプション）
                if (icon != MessageBoxIcon.None)
                {
                    var pictureBox = new PictureBox
                    {
                        Image = GetIcon(icon),
                        SizeMode = PictureBoxSizeMode.CenterImage,
                        Dock = DockStyle.Fill,
                        Width = 48,
                        Height = 48
                    };
                    mainPanel.Controls.Add(pictureBox, 0, 0);

                    // アイコンに応じた音を鳴らす
                    PlaySoundForIcon(icon);
                }

                

                // テキストを右揃えにして追加
                var label = new Label
                {
                    Text = text,
                    AutoSize = true,
                    MaximumSize = new Size(screenWidth - 400, screenHeight - 400),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };
                mainPanel.Controls.Add(label, 1, 0);

                // ボタンパネルを追加
                var panel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(5),
                    Height = 40,
                    AutoSize = true, // パネルのサイズを自動調整
                    AutoSizeMode = AutoSizeMode.GrowAndShrink // パネルのサイズを自動調整
                };
                form.Controls.Add(panel);

                DialogResult result = DialogResult.None;

                void AddButton(string buttonText, DialogResult dialogResult)
                {
                    var button = new Button
                    {
                        Text = buttonText,
                        DialogResult = dialogResult,
                        AutoSize = true
                    };
                    button.Click += (sender, e) => { result = dialogResult; form.Close(); };
                    panel.Controls.Add(button);
                }

                // ボタンを追加
                switch (buttons)
                {
                    case MessageBoxButtons.OK:
                        AddButton("OK", DialogResult.OK);
                        break;
                    case MessageBoxButtons.OKCancel:
                        AddButton("キャンセル", DialogResult.Cancel);
                        AddButton("OK", DialogResult.OK);
                        break;
                    case MessageBoxButtons.YesNo:
                        AddButton("いいえ", DialogResult.No);
                        AddButton("はい", DialogResult.Yes);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        AddButton("スキップ", DialogResult.Cancel);
                        AddButton("名前の変更", DialogResult.No);
                        AddButton("上書き", DialogResult.Yes);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported button type");
                }

                form.AcceptButton = panel.Controls[panel.Controls.Count - 1] as Button;
                form.CancelButton = panel.Controls[0] as Button;

                // Show the form and return the result
                form.ShowDialog();
                return result;
            }
        }

        // アイコンを取得するヘルパーメソッド
        private static Image? GetIcon(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Information => SystemIcons.Information.ToBitmap(),
                MessageBoxIcon.Warning => SystemIcons.Warning.ToBitmap(),
                MessageBoxIcon.Error => SystemIcons.Error.ToBitmap(),
                MessageBoxIcon.Question => SystemIcons.Question.ToBitmap(),
                MessageBoxIcon.None => SystemIcons.Information.ToBitmap(),
                _ => null
            };
        }

        // アイコンに応じた音を鳴らすヘルパーメソッド
        private static void PlaySoundForIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    SystemSounds.Asterisk.Play();  // 情報音
                    break;
                case MessageBoxIcon.Warning:
                    SystemSounds.Exclamation.Play();  // 警告音
                    break;
                case MessageBoxIcon.Error:
                    SystemSounds.Hand.Play();  // エラー音
                    break;
                case MessageBoxIcon.Question:
                    SystemSounds.Question.Play();  // 質問音
                    break;
                default:
                    break;
            }
        }
    }

    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        private const int SYMBOLIC_LINK_FLAG_FILE = 0x0;
        private const int SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                MessageBox.Show("引数が不足しています。ソースファイル/ディレクトリとリンク先ディレクトリを指定してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsUserAnAdmin())
            {
                ElevateToAdmin(args);
                return;
            }

            string targetDirectory = args[^1]; // 最後の引数がリンクを作成するディレクトリ
            string[] sourcePaths = args[..^1]; // 最後の引数以外がソースパス

            if (!Directory.Exists(targetDirectory))
            {
                MessageBox.Show("リンク先ディレクトリが存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (string sourcePath in sourcePaths)
            {
                if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
                {
                    CustomMessageBox.Show($"ソースパスが不正です: \n\"{sourcePath}\"", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }


                string sourceName = Path.GetFileName(sourcePath); // ソースの名前を取得
                string linkPath = Path.Combine(targetDirectory, sourceName);

                // リンクが既に存在する場合
                while (File.Exists(linkPath) || Directory.Exists(linkPath))
                {
                    var result = CustomMessageBox.Show(
                        $"{linkPath} \nは既に存在します。どうしますか？",
                        "確認",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Exclamation
                    );

                    if (result == DialogResult.Yes) // 上書き
                    {
                        try
                        {
                            if (File.Exists(linkPath))
                                File.Delete(linkPath); // ファイルを削除
                            if (Directory.Exists(linkPath))
                                Directory.Delete(linkPath, true); // ディレクトリを削除
                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Show($"削除に失敗しました: \n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        break; // 上書きの場合、ループを抜ける
                    }
                    else if (result == DialogResult.No) // 番号を付与
                    {
                        string baseName = Path.GetFileNameWithoutExtension(sourceName);
                        string extension = Path.GetExtension(sourceName);
                        int counter = 1;

                        do
                        {
                            linkPath = Path.Combine(targetDirectory, $"{baseName}_{counter}{extension}");
                            counter++;
                        } while (File.Exists(linkPath) || Directory.Exists(linkPath));
                    }
                    else if (result == DialogResult.Cancel) // キャンセル
                    {
                        linkPath = string.Empty;
                        break;
                    }
                    else
                    {
                        CustomMessageBox.Show("不明なエラーが発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        linkPath = string.Empty;
                        break;
                    }
                }

                if (linkPath == string.Empty)
                {
                    continue;
                }

                // シンボリックリンクの作成
                bool isDirectory = Directory.Exists(sourcePath);
                bool success = CreateSymbolicLink(linkPath, sourcePath, isDirectory ? SYMBOLIC_LINK_FLAG_DIRECTORY : SYMBOLIC_LINK_FLAG_FILE);

                if (!success)
                {
                    CustomMessageBox.Show($"シンボリックリンクの作成に失敗しました: \n{sourcePath}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("操作が完了しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static bool IsUserAnAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void ElevateToAdmin(string[] args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Application.ExecutablePath,
                Arguments = string.Join(" ", Array.ConvertAll(args, arg => $"\"{arg}\"")), // 引数を引用符で囲む
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(processInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"管理者権限での起動に失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
